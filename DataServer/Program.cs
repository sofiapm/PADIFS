using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Collections;
using System.IO;
using System.Threading;

namespace DataServer
{
    class Program
    {

        Program()
        {
        }

        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(Int32.Parse(args[1]));

            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(DataServerPuppet),
            args[0] + "DataServerPuppet",
            WellKnownObjectMode.Singleton);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(DataServerClient),
            args[0] + "DataServerClient",
            WellKnownObjectMode.Singleton);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(DataServerMS),
            args[0] + "DataServerMS",
            WellKnownObjectMode.Singleton);

            Hashtable metaDataServers = new Hashtable();
            metaDataServers.Add("1", "m-0");
            metaDataServers.Add("2", "m-1");
            metaDataServers.Add("3", "m-2");

            DataServer ds = new DataServer(args[0], channel, metaDataServers);
            DataServerClient.ctx = ds;
            DataServerMS.ctx = ds;
            DataServerPuppet.ctx = ds;

            foreach (DictionaryEntry c in metaDataServers)
            {
                IDSToMS ms = (IDSToMS)Activator.GetObject(
                       typeof(IDSToMS),
                       "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerDS");
                System.Console.WriteLine("Vou tentar falar com: " + c.Value.ToString());

                try
                {
                    ms.registarDS(args[0], args[1].Last().ToString());
                    break;
                }
                catch (Exception e)
                {
                    //System.Console.WriteLine(e.ToString());
                    System.Console.WriteLine("[REGISTARDS]: Não conseguiu aceder ao MS: " + c.Value.ToString() + " E " + c.Key.ToString());
                }
            }

            System.Console.WriteLine(args[0] + " DataServer no port: " + args[1]);
            System.Console.ReadLine();
        }

    }

    public class FileStructure
    {
        private string FileName;
        private int version;
        private bool isWriting;
        private bool isReading;
        private bool isDeleting;

        public FileStructure(string name)
        {
            FileName = name;
            version = 0;
            isWriting = false;
            isReading = false;
        }

        public string getFileName()
        {
            return FileName;
        }

        public void lockWrite()
        {
            isWriting = true;
        }

        public void lockRead()
        {
            isReading = true;
        }

        public void lockDelete()
        {
            isDeleting = true;
        }

        public void unlockWrite()
        {
            isWriting = false;
        }

        public void unlockRead()
        {
            isReading = false;
        }

        public void unlockDelete()
        {
            isDeleting = false;
        }

        public bool getLockWrite()
        {
            return isWriting;
        }

        public bool getLockRead()
        {
            return isReading;
        }

        public bool getLockDelete()
        {
            return isDeleting;
        }

        public void resetLockState()
        {
            isWriting = false;
            isReading = false;
            isDeleting = false;
        }

        public int getVersion()
        {
            return version;
        }

        public int incrementVersion()
        {
            return ++version;
        }
    }

    class DataServer
    {
        TcpChannel channel;
        Hashtable metaDataServers;
        Hashtable files;
        Queue<Action> actionQueue;
        bool freezed;
        bool failed;
        string dataServerID;

        public DataServer(string id, TcpChannel channel, Hashtable md)
        {
            this.channel = channel;
            metaDataServers = md;
            files = new Hashtable();
            actionQueue = new Queue<Action>();
            freezed = true;
            failed = false;
            dataServerID = id;
        }

        /********Puppet To DataServer***********/

        //starts buffering read and write requests, without answering
        public void freeze()
        {
            System.Console.WriteLine("Puppet mandou o DS freeze");
            freezed = true;
        }

        //responds to all buffered requests from clients and restarts replying new requests
        public void unfreeze()
        {
            System.Console.WriteLine("Puppet mandou o DS unfreeze");
            freezed = false;
            processaQueue();
        }

        public void processaQueue()
        {
            System.Console.WriteLine("DataServer processaQueue");
            while (actionQueue.Count != 0)
            {
                Action action = actionQueue.Dequeue();
                action();
            }
        }

        //DS ignores requests from Clients or messages from MS
        public void fail()
        {
            System.Console.WriteLine("Puppet mandou o DS falhar");
            failed = true;
        }

        //DS starts receiving requests from Clients and MS
        public void recover()
        {
            System.Console.WriteLine("Puppet mandou o DS recuperar");
            failed = false;
        }

        public string dump()
        {
            string st="Puppet mandou o DS fazer Dump\n";
            st += "---------------------BEGIN DUMP DataServer: " + dataServerID + " ------------------------\n";
            st += "---------------------Hashtable Files------------------------\n";
            foreach (DictionaryEntry entry in files)
            {
                FileStructure aux;
                aux = (FileStructure)entry.Value;
                st += "DataServer id: " + dataServerID + ", File name: " + aux.getFileName() + ", Version: " + aux.getVersion() + 
                    ", Write lock: " + aux.getLockWrite() + ", Read lock: " + aux.getLockRead() + ", Delete lock: " + aux.getLockDelete() + "\n";
            }

            st += "---------------------Priority Action Queue------------------------\n";
            foreach (Action action in actionQueue)
            {
                st += "DataServer id: " + dataServerID + ", Method: "+ action.Method.Name + "\n";
            }

            st += "--------------------END DUMP DataServer: " + dataServerID + " ------------------------\n";

            System.Console.WriteLine(st);
            
            return st;
        }

        public DadosFicheiroDS readFile(string fileName, string semantics)
        {
            System.Console.WriteLine("DataServer readFile");
            if (files.ContainsKey(fileName))
            {
                FileStructure newFile = (FileStructure)files[fileName];
                if (!newFile.getLockWrite() && !newFile.getLockDelete())
                {
                    newFile.lockRead();
                    DadosFicheiroDS ffds = new DadosFicheiroDS(newFile.getVersion(), File.ReadAllBytes(fileName));
                    newFile.unlockRead();
                    return ffds;
                }
                else
                {
                    System.Console.WriteLine("O ficheiro " + fileName + " encontra-se em escrita");
                    return null;
                }
            }
            else
            {
                System.Console.WriteLine("O ficheiro " + fileName + " não existe em sistema");
                return null;
            }
        }

        public void writeFile(string fileName, byte[] array)
        {
            System.Console.WriteLine("DataServer writeFile");
            if (files.ContainsKey(fileName))
            {
                FileStructure newFile = (FileStructure)files[fileName];
                if (!newFile.getLockRead() & !newFile.getLockWrite() & !newFile.getLockDelete())
                {
                    newFile.lockWrite();
                    newFile.incrementVersion();

                    //overwrites local file
                    File.WriteAllBytes(fileName, array);

                    files.Remove(fileName);
                    newFile.unlockWrite();
                    files.Add(fileName, newFile);
                }

            }
            else
            {
                //new file
                FileStructure newFile = new FileStructure(fileName);
                newFile.lockWrite();

                //writes local file
                File.WriteAllBytes(fileName, array);

                newFile.unlockWrite();
                files.Add(fileName, newFile);
            }
        }

        /********Client To DataServer***********/

        //returns the version and content of local file
        public DadosFicheiroDS read(string fileName, string semantics)
        {
            System.Console.WriteLine("Cliente está a ler ficheiro do DS");

            if (!failed)
            {
                if (freezed)
                {
                    System.Console.WriteLine("DS está freezed. Adiciona pedidos a threadpool");
                    string aux1 = fileName;
                    string aux2 = semantics;
                    actionQueue.Enqueue(() => readFile(aux1, aux2));
                }
                else
                {
                    if (actionQueue.Count > 0)
                    {
                        string aux1 = fileName;
                        string aux2 = semantics;
                        actionQueue.Enqueue(() => readFile(aux1, aux2));
                    }
                    else
                    {
                        return readFile(fileName, semantics);
                    }
                }

            }
            else
            {
                System.Console.WriteLine("DataServer está failed. Ignora pedidos de clientes");
            }
            return null;
        }

        //overwrites the content of file, creates new version
        public void write(string fileName, byte[] array)
        {
            System.Console.WriteLine("Cliente está a escrever ficheiro do DS");

            if (!failed)
            {
                if (freezed)
                {
                    System.Console.WriteLine("DS está freezed. Adiciona pedidos a threadpool");
                    string auxi1 = fileName;
                    byte[] auxi2 = array;
                    actionQueue.Enqueue(() => writeFile(auxi1, auxi2));
                    throw new NullReferenceException();
                }
                else
                {
                    if (actionQueue.Count > 0)
                    {
                        string auxi1 = fileName;
                        byte[] auxi2 = array;
                        actionQueue.Enqueue(() => writeFile(auxi1, auxi2));
                    }
                    else
                    {
                        writeFile(fileName, array);
                    }
                }

            }
            else
            {
                System.Console.WriteLine("DataServer está failed. Ignora pedidos de clientes");
                throw new NullReferenceException();
            }
        }

        public bool delete(string fileName)
        {
            if (!failed && !freezed)
            {
                if (files.ContainsKey(fileName))
                {
                    FileStructure newFile = (FileStructure)files[fileName];
                    if (!newFile.getLockRead() && !newFile.getLockWrite())
                    {
                        newFile.lockDelete();
                        files.Remove(fileName);
                        files.Add(fileName, newFile);
                        return true;
                    }
                }
                else return true;
            }
            return false;
        }

        public void confirmarDelete(string fileName, bool resposta)
        {
            if (resposta == true)
            {
                if (files.ContainsKey(fileName))
                {
                    File.Delete(fileName);
                    files.Remove(fileName);
                }
            }
            else
            {
                if (files.ContainsKey(fileName))
                {
                    FileStructure newFile = (FileStructure)files[fileName];
                    newFile.unlockDelete();
                    files.Remove(fileName);
                    files.Add(fileName, newFile);
                }
            }

        }

        /********MS To DataServer***********/
        public void areYouAlive()
        {
            System.Console.WriteLine("MS pergunta se DS esta vivo.");
        }

        public void respostaMS(string resp)
        {
            System.Console.WriteLine("MS diz: " + resp);
        }
    }

    class DataServerPuppet : MarshalByRefObject, IPuppetToDS
    {
        public static DataServer ctx;

        //starts buffering read and write requests, without answering
        public void freeze()
        {
            ctx.freeze();
        }

        //responds to all buffered requests from clients and restarts replying new requests
        public void unfreeze()
        {
            ctx.unfreeze();
        }

        //DS ignores requests from Clients or messages from MS
        public void fail()
        {
            ctx.fail();
        }

        //DS starts receiving requests from Clients and MS
        public void recover()
        {
            ctx.recover();
        }

        public string dump()
        {
           return ctx.dump();
        }

    }

    class DataServerClient : MarshalByRefObject, IClientToDS
    {
        public static DataServer ctx;

        //returns the version and content os local file
        public DadosFicheiroDS read(string fileName, string semantics)
        {
            return ctx.read(fileName, semantics);

        }

        //overwrites the content of file, creates new version
        public void write(string fileName, byte[] array)
        {
            ctx.write(fileName, array);
        }

        public bool delete(string fileName)
        {
            return ctx.delete(fileName);
        }

        public void confirmarDelete(string fileName, bool resposta)
        {
            ctx.confirmarDelete(fileName, resposta);
        }

    }

    class DataServerMS : MarshalByRefObject, IMSToDS
    {
        public static DataServer ctx;

        public void areYouAlive()
        {
            ctx.areYouAlive();
        }

        public void respostaMS(string resp)
        {
            ctx.respostaMS(resp);
        }
    }
}
