using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace MetaDataServer
{
    class Program
    {
        
        Program()
        {
        }

        static void Main(string[] args)
        {
            TcpChannel channel;
            channel = new TcpChannel(Int32.Parse(args[1]));

            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(MetaServerPuppet),
            args[0] + "MetaServerPuppet", WellKnownObjectMode.Singleton);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(MetaServerClient),
            args[0] + "MetaServerClient", WellKnownObjectMode.Singleton);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(MetaServerDS),
            args[0] + "MetaServerDS", WellKnownObjectMode.Singleton);

            MetaServer meta = new MetaServer (channel, args[0]);
            MetaServerPuppet.ctx = meta;
            MetaServerClient.ctx = meta;
            MetaServerDS.ctx = meta;

            string currentDirectory = Environment.CurrentDirectory;
            string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
            string strpathDS = newDirectory[0] + "Disk\\" + "InfoDS" + args[0] + ".xml";
            string strpathFile = newDirectory[0] + "Disk\\" + "InfoFiles" + args[0] + ".xml";
            string strpathNBDS = newDirectory[0] + "Disk\\" + "NBDS" + args[0] + ".xml";

            File.Delete(strpathDS);
            File.Delete(strpathFile);
            File.Delete(strpathNBDS);

            System.Console.WriteLine(args[0] + ": <enter> para sair..." + args[1]);

            System.Console.ReadLine();
        }

    }

    class MetaServer
    {
        TcpChannel channel;
        String nomeMeta;

        //hashtable de dataserververs <string nome, string ID>
        Hashtable dataServers = new Hashtable(); 

        //hashtable de ficheiros <string filename, DadosFicheiro dados>
        Hashtable files = new Hashtable();
 
        //hashtable de NBDataS <string filename, int ds>
        Hashtable nBDataS = new Hashtable();

        //flag activa quando o metadata server esta em fail
        Boolean isFailed = false;

        public MetaServer(TcpChannel channel, String nome)
        {
            this.channel = channel;
            this.nomeMeta = nome;
        }

        /********Puppet To MetaDataServer***********/

        //the MS stops processing requests from clients or others MS
        public void fail()
        {
            System.Console.WriteLine("Puppet mandou MS falhar!");
            isFailed = true;

            writeToDisc();
        }

        //MS starts receiving requests from clients and others MS
        public void recover()
        {
            System.Console.WriteLine("Puppet mandou MS recuperar!");
            isFailed = false;

            try
            {
                readFromDisk();
            }
            catch
            {
                System.Console.WriteLine("Não existe nenhum ficheiro em disco.");
            }
        }

        public void dump()
        {
            if (isFailed)
                throw new NullReferenceException();

            System.Console.WriteLine("Puppet mandou o MS fazer Dump");
            String result = "";

            try
            {
                result = result + "\n***************DS Registados***************";
                foreach (DictionaryEntry entry in dataServers)
                    result = result + "\nNome: " + entry.Key + " ID: " + entry.Value;
                result = result + "\n******************END DS*******************";

                result = result + "\n*****************Ficheiros*****************";

                foreach (DictionaryEntry entry in files)
                {
                    string aux = "";
                    foreach (DictionaryEntry ds in ((DadosFicheiro)entry.Value).getPorts())
                        aux = aux + "\nNome: " + ds.Key + " ID: " + ds.Value;

                    result = result + "\nNome: " + entry.Key
                        + " ReadQuórum: " + ((DadosFicheiro)entry.Value).getRQ()
                        + " WriteQuórum: " + ((DadosFicheiro)entry.Value).getWQ()
                        + "\nDS: " + aux + "\n--------------";
                }
                result = result + "\n***************END Ficheiros***************\n";
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }

            System.Console.WriteLine(result);
        }


        /********Client To MetaDataServer***********/

        //returns to client the contents of the metadata stored for that file
        public DadosFicheiro open(string fileName)
        {
            if (isFailed)
                throw new NullReferenceException();

            System.Console.WriteLine("Cliente mandou MS abrir ficheiro: " + fileName);

            try
            {
                return (DadosFicheiro)files[fileName];
            }
            catch
            {
                System.Console.WriteLine("O Ficheiro " + fileName + " não existe.");
                return new DadosFicheiro(0, 0, null);
            }
        }


        //informs MS that client is no longer using that file - client must discard all metadata for that file
        public void close(string fileName)
        {
            if (isFailed)
                throw new NullReferenceException();

            System.Console.WriteLine("Cliente mandou MS fechar ficheiro: " + fileName);
        }

        //creates a new file (if it doesn t exist) - in case of sucesses, returns the same that open
        public DadosFicheiro create(string fileName, int numDS, int rQuorum, int wQuorum)
        {
            if (isFailed)
                throw new NullReferenceException();

            System.Console.WriteLine("Cliente mandou MS criar ficheiro: " + fileName);

            Hashtable ports = new Hashtable();
            DadosFicheiro df = new DadosFicheiro(0, 0, null);

            if (!files.ContainsKey(fileName))
            {
                System.Console.WriteLine("Numero de DS: " + dataServers.Count + ", Numero de replicas: " + numDS);
                if (numDS >= dataServers.Count)
                    ports = (Hashtable)dataServers.Clone();
                else
                    ports = bestDS(numDS);

                df = new DadosFicheiro(rQuorum, wQuorum, ports);
                files.Add(fileName, df);
                nBDataS.Add(fileName, numDS);

                System.Console.WriteLine("********Stored in DS********");
                foreach (DictionaryEntry c in df.getPorts())
                    System.Console.WriteLine("Nome: " + c.Key + " ID: " + c.Value);
                System.Console.WriteLine("***********DS-END***********");
            }
            else
                System.Console.WriteLine("O ficheiro " + fileName + " já existe!");
                     
            return df;
        }

        //deletes the file
        public DadosFicheiro delete(string fileName)
        {
            if (isFailed)
                throw new NullReferenceException();

            System.Console.WriteLine("Cliente mandou MS apagar ficheiro: " + fileName);

            DadosFicheiro df = new DadosFicheiro(0, 0, null);

            if (files.ContainsKey(fileName))
            {
                Hashtable ports = (Hashtable)((DadosFicheiro)files[fileName]).getPorts().Clone();
                df = new DadosFicheiro (((DadosFicheiro)files[fileName]).getRQ(),
                    ((DadosFicheiro)files[fileName]).getWQ(),
                    ports) ;
                files.Remove(fileName);
                nBDataS.Remove(fileName);
            }
            else System.Console.WriteLine("O ficheiro " + fileName + " não existe!");

            return df;
        }

        /********DS To MetadataServer***********/
        public void respostaDS(string resposta)
        {
            if (isFailed)
                throw new NullReferenceException();
            
            System.Console.WriteLine(resposta);
        }

        public void registarDS(string name, string id)
        {
            if (isFailed)
                throw new NullReferenceException();

            System.Console.WriteLine("MS registou DS: " + name);

            if (!dataServers.Contains(name))
            {
                dataServers.Add(name, id);

                foreach (DictionaryEntry entry in files)
                    if (((DadosFicheiro)entry.Value).getPorts().Count < (int)nBDataS[(string)entry.Key])
                    {
                        ((DadosFicheiro)entry.Value).getPorts().Add(name, id);
                        System.Console.WriteLine("O ficheiro " + entry.Key + " foi guardado no DS " + name);
                    }

            }
            else System.Console.WriteLine("O DS " + name + " já está registado");
        }

        public void writeToDisc()
        {
            try
            {
                string currentDirectory = Environment.CurrentDirectory;
                string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
                string strpathDS = newDirectory[0] + "Disk\\" + "InfoDS" + nomeMeta + ".xml";
                string strpathFile = newDirectory[0] + "Disk\\" + "InfoFiles" + nomeMeta + ".xml";
                string strpathNBDS = newDirectory[0] + "Disk\\" + "NBDS" + ".xml";

                BinaryFormatter bfw = new BinaryFormatter();
                StreamWriter ws = new StreamWriter(@"" + strpathDS);
                bfw.Serialize(ws.BaseStream, dataServers);
                ws.Close();

                BinaryFormatter bfw2 = new BinaryFormatter();
                StreamWriter ws2 = new StreamWriter(@"" + strpathFile);
                bfw2.Serialize(ws2.BaseStream, files);
                ws2.Close();

                BinaryFormatter bfw3 = new BinaryFormatter();
                StreamWriter ws3 = new StreamWriter(@"" + strpathNBDS);
                bfw3.Serialize(ws3.BaseStream, nBDataS);
                ws3.Close();

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }            
        }

        public void readFromDisk()
        {
            string currentDirectory = Environment.CurrentDirectory;
            string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
            string strpathDS = newDirectory[0] + "Disk\\" + "InfoDS" + nomeMeta + ".xml";
            string strpathFile = newDirectory[0] + "Disk\\" + "InfoFiles" + nomeMeta + ".xml";
            string strpathNBDS= newDirectory[0] + "Disk\\" + "NBDS" + nomeMeta + ".xml";

            StreamReader readMap = new StreamReader(@"" + strpathDS);
            BinaryFormatter bf = new BinaryFormatter();
            dataServers = (Hashtable)bf.Deserialize(readMap.BaseStream);

            StreamReader readMap2 = new StreamReader(@"" + strpathFile);
            BinaryFormatter bf2 = new BinaryFormatter();
            files = (Hashtable)bf2.Deserialize(readMap2.BaseStream);

            StreamReader readMap3 = new StreamReader(@"" + strpathNBDS);
            BinaryFormatter bf3 = new BinaryFormatter();
            nBDataS = (Hashtable)bf3.Deserialize(readMap3.BaseStream);
        }

        public Hashtable bestDS(int num)
        {
            Hashtable ports = new Hashtable(); 

            //escolher num DSs
            for (int i = 0; i <= num; i++) 
                foreach (DictionaryEntry entry in dataServers)
                    ports.Add(entry.Key, entry.Value);

            return ports;
        }

    }

    class MetaServerPuppet : MarshalByRefObject, IPuppetToMS
    {
        public static MetaServer ctx;

        //the MS stops processing requests from clients or others MS
        public void fail()
        {
            ctx.fail();
        }

        //MS starts receiving requests from clients and others MS
        public void recover()
        {
            ctx.recover();
        }

        public void dump()
        {
            ctx.dump();
        }
    }

    class MetaServerClient : MarshalByRefObject, IClientToMS
    {
        public static MetaServer ctx;

        //returns to client the contents of the metadata stored for that file
        public DadosFicheiro open(string fileName)
        {
            return ctx.open(fileName);
        }

        //informs MS that client is no longer using that file - client must discard all metadata for that file
        public void close(string fileName)
        {
            ctx.close(fileName);
        }

        //creates a new file (if it doesn t exist) - in case of sucesses, returns the same that open
        public DadosFicheiro create(string fileName, int numDS, int rQuorum, int wQuorum)
        {
            return ctx.create(fileName, numDS, rQuorum, wQuorum);
        }

        //deletes the file
        public DadosFicheiro delete(string fileName)
        {
           return ctx.delete(fileName);
        }
   
    }

    class MetaServerDS : MarshalByRefObject, IDSToMS
    {
        public static MetaServer ctx;

        public void respostaDS(string resposta)
        {
            ctx.respostaDS(resposta);
        }

        public void registarDS(string nome, string ID)
        {
            ctx.registarDS(nome, ID);
        }

        public void delete(string fileName)
        {
            ctx.delete(fileName);
        }
    }
}
