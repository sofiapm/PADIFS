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
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Text.RegularExpressions;

namespace DataServer
{
    class Program
    {

        Program()
        {
        }

        public static void writeToDisk(string dataServerName, Hashtable fs, Queue<object> pQ)
        {
            while (true)
            {
                try
                {
                    string currentDirectory = Environment.CurrentDirectory;
                    string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
                    string strpathDSFiles = newDirectory[0] + "Disk\\" + "DSFiles" + dataServerName + ".xml";
                    string strpathDSPq = newDirectory[0] + "Disk\\" + "DSPq" + dataServerName + ".xml";

                    BinaryFormatter bfw = new BinaryFormatter();
                    StreamWriter ws = new StreamWriter(@"" + strpathDSFiles);
                    bfw.Serialize(ws.BaseStream, fs);
                    ws.Close();

                    BinaryFormatter bfw2 = new BinaryFormatter();
                    StreamWriter ws2 = new StreamWriter(@"" + strpathDSPq);
                    bfw2.Serialize(ws2.BaseStream, pQ);
                    ws2.Close();
                }
                catch
                {
                }
            }
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

            //delete files if they exists
            string currentDirectory = Environment.CurrentDirectory;
            string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
            string strpathDSFiles = newDirectory[0] + "Disk\\" + "DSFiles" + args[0] + ".xml";
            string strpathDSPq = newDirectory[0] + "Disk\\" + "DSPq" + args[0] + ".xml";
            File.Delete(strpathDSFiles);
            File.Delete(strpathDSPq);

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

            //thread de backup para disco
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            new Thread(delegate()
            {
                String d = args[0];
                Hashtable fs = ds.getFiles();
                Queue<object> pq = ds.getPriorityQueue();
                writeToDisk(d, fs, pq);
            }).Start();

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
        Queue<object> priorityQueue;
        bool freezed;
        bool failed;
        string dataServerID;

        public DataServer(string id, TcpChannel channel, Hashtable md)
        {
            this.channel = channel;
            metaDataServers = md;
            files = new Hashtable();
            priorityQueue = new Queue<object>();
            freezed = true;
            failed = false;
            dataServerID = id;
        }

        public Hashtable getFiles()
        {
            return files;
        }

        public Queue<object> getPriorityQueue()
        {
            return priorityQueue;
        }

        /********Puppet To DataServer***********/

        //starts buffering read and write requests, without answering
        public void freeze()
        {
            System.Console.WriteLine("DS: " + dataServerID + " - PuppetMaster: enviou comando freeze");
            freezed = true;
        }

        //responds to all buffered requests from clients and restarts replying new requests
        public void unfreeze()
        {
            System.Console.WriteLine("DS: " + dataServerID + " - PuppetMaster: enviou comando unfreeze");
            processaQueue();
            freezed = false;
        }

        public void processaQueue()
        {
            System.Console.WriteLine("DS: " + dataServerID + " - processaQueue()");
            object remoteObject;
            lock (priorityQueue)
            {
                while (priorityQueue.Count() > 0)
                {
                    remoteObject = priorityQueue.Dequeue();
                    lock (remoteObject)
                    {
                        Monitor.Pulse(remoteObject);
                    }

                }
            }
        }

        //DS ignores requests from Clients or messages from MS
        public void fail()
        {
            System.Console.WriteLine("DS: " + dataServerID + " - PuppetMaster: enviou comando fail");
            failed = true;
        }

        //DS starts receiving requests from Clients and MS
        public void recover()
        {
            System.Console.WriteLine("DS: " + dataServerID + " - PuppetMaster: enviou comando recover");
            try
            {
                readFromDisk();
            }
            catch
            {
            }
            failed = false;
        }

        public string dump()
        {
            string st = "Puppet mandou o DS fazer Dump\n";
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

            st += "DataServer id: " + dataServerID + ", Número total de objectos na fila de prioridades: " + priorityQueue.Count() + "\n";

            st += "--------------------END DUMP DataServer: " + dataServerID + " ------------------------\n";

            System.Console.WriteLine(st);

            return st;
        }

        /********Client To DataServer***********/

        //returns the version and content of local file
        public DadosFicheiroDS read(string fileName, string semantics)
        {
            System.Console.WriteLine("DS: " + dataServerID + " - Client: enviou comando READ - ficheiro: " + fileName);
            if (!failed)
            {
                if (freezed)
                {
                    System.Console.WriteLine("DS: " + dataServerID + " - READ: encontra-se no modo freeze");
                    object remoteObject = new object();
                    lock (this)
                    {
                        priorityQueue.Enqueue(remoteObject);
                    }
                    lock (remoteObject)
                    {
                        Monitor.Wait(remoteObject);
                    }
                }
                else
                {
                    if (priorityQueue.Count > 0)
                    {
                        object remoteObject = new object();
                        lock (this)
                        {
                            priorityQueue.Enqueue(remoteObject);
                        }
                        lock (remoteObject)
                        {
                            Monitor.Wait(remoteObject);
                        }
                    }
                }

                System.Console.WriteLine("DS: " + dataServerID + " - READ: inicia leitura do ficheiro: " + fileName);
                if (files.ContainsKey(fileName))
                {
                    FileStructure newFile = (FileStructure)files[fileName];
                    if (!newFile.getLockWrite() && !newFile.getLockDelete())
                    {
                        newFile.lockRead();

                        string currentDirectory = Environment.CurrentDirectory;
                        string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
                        string strpathFiles = newDirectory[0] + "Disk\\" + dataServerID + "-" + fileName;
                        DadosFicheiroDS ffds = new DadosFicheiroDS(newFile.getVersion(), File.ReadAllBytes(strpathFiles));
                        newFile.unlockRead();
                        return ffds;
                    }
                    else
                    {
                        System.Console.WriteLine("DS: " + dataServerID + " - READ: o ficheiro: " + fileName + " encontra-se em modo de escrita");
                        throw new NullReferenceException();
                    }
                }
                else
                {
                    System.Console.WriteLine("DS: " + dataServerID + " - READ: o ficheiro: " + fileName + " não existe em sistema");
                    return null;
                }
            }
            else
            {
                System.Console.WriteLine("DS: " + dataServerID + " - READ: está em modo failed. Ignora pedidos de clientes");
            }
            return null;
        }

        //overwrites the content of file, creates new version
        public void write(string fileName, byte[] array)
        {
            System.Console.WriteLine("DS: " + dataServerID + " - Client: enviou comando WRITE - ficheiro: " + fileName);
            if (!failed)
            {
                if (freezed)
                {
                    System.Console.WriteLine("DS: " + dataServerID + " - WRITE: encontra-se no modo freeze");
                    object remoteObject = new object();
                    lock (this)
                    {
                        priorityQueue.Enqueue(remoteObject);
                    }
                    lock (remoteObject)
                    {
                        Monitor.Wait(remoteObject);
                    }
                }
                else
                {
                    if (priorityQueue.Count > 0)
                    {
                        object remoteObject = new object();
                        lock (this)
                        {
                            priorityQueue.Enqueue(remoteObject);
                        }
                        lock (remoteObject)
                        {
                            Monitor.Wait(remoteObject);
                        }
                    }
                }

                System.Console.WriteLine("DS: " + dataServerID + " - WRITE: inicia escrita do ficheiro: " + fileName);
                if (files.ContainsKey(fileName))
                {
                    System.Console.WriteLine("DS: " + dataServerID + " - WRITE: o ficheiro: " + fileName + " existe em sistema. Inicia processo de overwrite do ficheiro");
                    FileStructure newFile = (FileStructure)files[fileName];
                    if (!newFile.getLockRead() & !newFile.getLockWrite() & !newFile.getLockDelete())
                    {
                        newFile.lockWrite();
                        newFile.incrementVersion();

                        //overwrites local file
                        string currentDirectory = Environment.CurrentDirectory;
                        string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
                        string strpathFiles = newDirectory[0] + "Disk\\" + dataServerID + "-" + fileName;
                        File.WriteAllBytes(strpathFiles, array);
                        lock (files)
                        {
                            files.Remove(fileName);
                            newFile.unlockWrite();
                            files.Add(fileName, newFile);
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("DS: " + dataServerID + " - WRITE: o ficheiro: " + fileName + " encontra-se em modo de escrita");
                        throw new NullReferenceException();
                    }

                }
                else
                {
                    System.Console.WriteLine("DS: " + dataServerID + " - WRITE: o ficheiro: " + fileName + " não existe em sistema. Criação de novo ficheiro");
                    //new file
                    FileStructure newFile = new FileStructure(fileName);
                    newFile.lockWrite();

                    //writes local file
                    string currentDirectory = Environment.CurrentDirectory;
                    string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
                    string strpathFiles = newDirectory[0] + "Disk\\" + fileName;
                    File.WriteAllBytes(strpathFiles, array);

                    newFile.unlockWrite();
                    lock (files)
                    {
                        files.Add(fileName, newFile);
                    }
                }


            }
            else
            {
                System.Console.WriteLine("DS: " + dataServerID + " - WRITE: está em modo failed. Ignora pedidos de clientes");
                throw new NullReferenceException();
            }

        }

        public bool delete(string fileName)
        {
            System.Console.WriteLine("DS: " + dataServerID + " - Client: enviou comando delete - ficheiro: " + fileName);
            if (!failed && !freezed)
            {
                if (files.ContainsKey(fileName))
                {
                    System.Console.WriteLine("DS: " + dataServerID + " - DELETE: inicia o processo de apagar o ficheiro: " + fileName);
                    FileStructure newFile = (FileStructure)files[fileName];
                    if (!newFile.getLockRead() && !newFile.getLockWrite())
                    {
                        newFile.lockDelete();
                        lock (files)
                        {
                            files.Remove(fileName);
                            files.Add(fileName, newFile);
                        }
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
                System.Console.WriteLine("DS: " + dataServerID + " - DELETE: apaga o ficheiro: " + fileName);
                if (files.ContainsKey(fileName))
                {
                    lock (files)
                    {
                        File.Delete(fileName);
                        files.Remove(fileName);
                    }
                }
            }
            else
            {
                System.Console.WriteLine("DS: " + dataServerID + " - DELETE: reverte o processo de apagar o ficheiro: " + fileName);
                if (files.ContainsKey(fileName))
                {
                    FileStructure newFile = (FileStructure)files[fileName];
                    newFile.unlockDelete();
                    lock (files)
                    {
                        files.Remove(fileName);
                        files.Add(fileName, newFile);
                    }
                }
            }

        }

        public void readFromDisk()
        {
            string currentDirectory = Environment.CurrentDirectory;
            string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
            string strpathDSFiles = newDirectory[0] + "Disk\\" + "DSFiles" + dataServerID + ".xml";
            string strpathDSPq = newDirectory[0] + "Disk\\" + "DSPq" + dataServerID + ".xml";

            StreamReader readMap = new StreamReader(@"" + strpathDSFiles);
            BinaryFormatter bf = new BinaryFormatter();
            files = (Hashtable)bf.Deserialize(readMap.BaseStream);

            StreamReader readMap2 = new StreamReader(@"" + strpathDSPq);
            BinaryFormatter bf2 = new BinaryFormatter();
            priorityQueue = (Queue<object>)bf2.Deserialize(readMap2.BaseStream);
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

        public DadosFicheiroDS readMS(string fileName)
        {
            return ctx.read(fileName, "");
        }

        public void writeMS(string fileName, byte[] array)
        {
            ctx.write(fileName, array);
        }

        public bool deleteMS(string fileName)
        {
            return ctx.delete(fileName);
        }

        public void confirmarDeleteMS(string fileName, bool confirmacao)
        {
            ctx.confirmarDelete(fileName, confirmacao);
        }

    }
}