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
using System.Threading;

namespace MetaDataServer
{
    class Program
    {
        //escrever a informação para disco
        public static void  writeToDisc(string nomeMeta, Hashtable dataServers, Hashtable files, Hashtable nBDataS, Hashtable dataServersNum)
        {
            while (true)
                try
                {
                    string currentDirectory = Environment.CurrentDirectory;
                    string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
                    string strpathDS = newDirectory[0] + "Disk\\" + "InfoDS" + nomeMeta + ".xml";
                    string strpathFile = newDirectory[0] + "Disk\\" + "InfoFiles" + nomeMeta + ".xml";
                    string strpathNBDS = newDirectory[0] + "Disk\\" + "NBDS" + nomeMeta + ".xml";
                    string strpathDSnum = newDirectory[0] + "Disk\\" + "DSnum" + nomeMeta + ".xml";

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

                    BinaryFormatter bfw4 = new BinaryFormatter();
                    StreamWriter ws4 = new StreamWriter(@"" + strpathDSnum);
                    bfw4.Serialize(ws4.BaseStream, dataServersNum);
                    ws4.Close();

                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.ToString());
                }
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

            //apagar os ficheiros que existam 
            string currentDirectory = Environment.CurrentDirectory;
            string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
            string strpathDS = newDirectory[0] + "Disk\\" + "InfoDS" + args[0] + ".xml";
            string strpathFile = newDirectory[0] + "Disk\\" + "InfoFiles" + args[0] + ".xml";
            string strpathNBDS = newDirectory[0] + "Disk\\" + "NBDS" + args[0] + ".xml";
            string strpathDSnum = newDirectory[0] + "Disk\\" + "DSnum" + args[0] + ".xml";
            File.Delete(strpathDS);
            File.Delete(strpathFile);
            File.Delete(strpathNBDS);
            File.Delete(strpathDSnum);

            System.Console.WriteLine(args[0] + ": <enter> para sair..." + args[1]);

            //thread de backup para disco --> tem que ser com monitores para aceder ao ficheiro
            //ManualResetEvent resetEvent = new ManualResetEvent(false);
            //new Thread(delegate()
            //{
            //    String m = args[0];
            //    Hashtable ds = meta.get_dataServers();
            //    Hashtable f = meta.get_files();
            //    Hashtable nDS = meta.get_nBDataS();
            //    Hashtable DSn = meta.get_DSnum();
            //    writeToDisc(m, ds, f, nDS);
            //}).Start();

            System.Console.ReadLine();
        }

    }

    class MetaServer
    {
        TcpChannel channel;
        String nomeMeta;

        //hashtable de dataserververs <string nome, string ID>
        Hashtable dataServers = new Hashtable();

        //hashtable com o numero de ficheiros que cada ds tem <string nome, int num>
        Hashtable dataServersNum = new Hashtable();

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

        public Hashtable get_dataServers()
        {
            return dataServers;
        }

        public Hashtable get_files()
        {
            return files;
        }

        public Hashtable get_nBDataS()
        {
            return nBDataS;
        }

        public Hashtable get_DSnum()
        {
            return dataServersNum;
        }

        /********Puppet To MetaDataServer***********/

        //the MS stops processing requests from clients or others MS
        public void fail()
        {
            System.Console.WriteLine("[FAIL] Puppet mandou MS falhar!");
            isFailed = true;

            writeToDisc();
        }

        //MS starts receiving requests from clients and others MS
        public void recover()
        {
            System.Console.WriteLine("[RECOVER] Puppet mandou MS recuperar!");
            isFailed = false;

            try
            {
                readFromDisk();
            }
            catch
            {
                System.Console.WriteLine("[RECOVER] Não existe nenhum ficheiro em disco.");
            }
        }

        public void dump()
        {
            if (isFailed)
                throw new NullReferenceException();

            System.Console.WriteLine("[DUMP] Puppet mandou o MS fazer Dump");
            String result = "";

            try
            {
                result = result + "\n***************DS Registados***************";
                foreach (DictionaryEntry entry in dataServers)
                    result = result + "\nNome: " + entry.Key + 
                        " ID: " + entry.Value + 
                        " NumFiles: " + dataServersNum[entry.Key];
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
                System.Console.WriteLine("[DUMP] " + e.ToString());
            }

            System.Console.WriteLine(result);
        }


        /********Client To MetaDataServer***********/

        //returns to client the contents of the metadata stored for that file
        public DadosFicheiro open(string fileName)
        {
            if (isFailed)
                throw new NullReferenceException();

            System.Console.WriteLine("[OPEN] Cliente mandou MS abrir ficheiro: " + fileName);

            try
            {
                return (DadosFicheiro)files[fileName];
            }
            catch
            {
                System.Console.WriteLine("[OPEN] O Ficheiro " + fileName + " não existe.");
                return new DadosFicheiro(0, 0, null);
            }
        }


        //informs MS that client is no longer using that file - client must discard all metadata for that file
        public void close(string fileName)
        {
            if (isFailed)
                throw new NullReferenceException();

            System.Console.WriteLine("[CLOSE] Cliente mandou MS fechar ficheiro: " + fileName);
        }

        //creates a new file (if it doesn t exist) - in case of sucesses, returns the same that open
        public DadosFicheiro create(string fileName, int numDS, int rQuorum, int wQuorum)
        {
            if (isFailed)
                throw new NullReferenceException();

            System.Console.WriteLine("[CREATE] Cliente mandou MS criar ficheiro: " + fileName);

            Hashtable ports = new Hashtable();
            DadosFicheiro df = new DadosFicheiro(0, 0, null);
            if (!files.ContainsKey(fileName))
            {
                System.Console.WriteLine("[CREATE] Numero de DS: " + dataServers.Count + ", Numero de replicas: " + numDS);
                
                if (numDS >= dataServers.Count)
                    foreach (DictionaryEntry entry in dataServers)
                        ports.Add(entry.Key, entry.Value);
                else
                    ports = bestDS(numDS);

                //actualizar valores na hash dos ds
                foreach (DictionaryEntry entry in ports)
                {
                    int i = (int)dataServersNum[entry.Key] + 1;
                    dataServersNum.Remove(entry.Key);
                    dataServersNum.Add(entry.Key, i);
                }
                System.Console.WriteLine("saiu");

                df = new DadosFicheiro(rQuorum, wQuorum, ports);
                files.Add(fileName, df);
                nBDataS.Add(fileName, numDS);

                System.Console.WriteLine("********Stored in DS********");
                foreach (DictionaryEntry c in df.getPorts())
                    System.Console.WriteLine("Nome: " + c.Key + " ID: " + c.Value);
                System.Console.WriteLine("***********DS-END***********");
            }
            else
                System.Console.WriteLine("[CREATE] O ficheiro " + fileName + " já existe!");

             return df;
        }

        //deletes the file
        public DadosFicheiro delete(string fileName)
        {
            if (isFailed)
                throw new NullReferenceException();

            System.Console.WriteLine("[DELETE] Cliente mandou MS apagar ficheiro: " + fileName);

            DadosFicheiro df = new DadosFicheiro(0, 0, null);

            if (files.ContainsKey(fileName))
            {
                Hashtable ports = (Hashtable)((DadosFicheiro)files[fileName]).getPorts().Clone();
                df = new DadosFicheiro (((DadosFicheiro)files[fileName]).getRQ(),
                    ((DadosFicheiro)files[fileName]).getWQ(),
                    ports) ;
                files.Remove(fileName);
                nBDataS.Remove(fileName);
                //decrementar todos os ds onde o file estava
                foreach (DictionaryEntry entry in ports){
                    int i = (int)dataServersNum[entry.Key] - 1 ;
                    dataServersNum.Remove(entry.Key);
                    dataServersNum.Add(entry.Key, i);
                }
            }
            else System.Console.WriteLine("[DELETE] O ficheiro " + fileName + " não existe!");

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

            System.Console.WriteLine("[REGISTARDS] MS registou DS: " + name);
            
            if (!dataServers.ContainsKey(name))
            {
                dataServers.Add(name,id);
                dataServersNum.Add(name, 0);

                foreach (DictionaryEntry entry in files)
                    if (((DadosFicheiro)entry.Value).getPorts().Count < (int)nBDataS[(string)entry.Key])
                    {
                        ((DadosFicheiro)entry.Value).getPorts().Add(name, id);
                        int i = (int)dataServersNum[name] + 1;
                        dataServersNum.Remove(name);
                        dataServersNum.Add(name, i);
                        System.Console.WriteLine("[REGISTARDS] O ficheiro " + entry.Key + " foi guardado no DS " + name);
                    }

            }
            else System.Console.WriteLine("[REGISTARDS] O DS " + name + " já está registado");
        }

        public Hashtable bestDS(int num)
        {
            Hashtable ports = new Hashtable(); 

            //escolher num DSs
            
            foreach (DictionaryEntry entry in dataServers)
            {
                 for (int i = 0; i <= num; i++)
                    if (!ports.ContainsKey(entry.Key))
                      ports.Add(entry.Key, entry.Value);
            }
           
            return ports;
        }

        public void writeToDisc()
        {
            try
            {                
                string currentDirectory = Environment.CurrentDirectory;
                string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
                string strpathDS = newDirectory[0] + "Disk\\" + "InfoDS" + nomeMeta + ".xml";
                string strpathFile = newDirectory[0] + "Disk\\" + "InfoFiles" + nomeMeta + ".xml";
                string strpathNBDS = newDirectory[0] + "Disk\\" + "NBDS" + nomeMeta + ".xml";
                string strpathDSnum = newDirectory[0] + "Disk\\" + "DSnum" + nomeMeta + ".xml";

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

                BinaryFormatter bfw4 = new BinaryFormatter();
                StreamWriter ws4 = new StreamWriter(@"" + strpathDSnum);
                bfw4.Serialize(ws4.BaseStream, dataServersNum);
                ws4.Close();

            }
            catch (Exception e)
            {
                System.Console.WriteLine("[WRITETODISC] " + e.ToString());
            }
        }

        public void readFromDisk()
        {
            string currentDirectory = Environment.CurrentDirectory;
            string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
            string strpathDS = newDirectory[0] + "Disk\\" + "InfoDS" + nomeMeta + ".xml";
            string strpathFile = newDirectory[0] + "Disk\\" + "InfoFiles" + nomeMeta + ".xml";
            string strpathNBDS = newDirectory[0] + "Disk\\" + "NBDS" + nomeMeta + ".xml";
            string strpathDSnum = newDirectory[0] + "Disk\\" + "DSnum" + nomeMeta + ".xml";

            StreamReader readMap = new StreamReader(@"" + strpathDS);
            BinaryFormatter bf = new BinaryFormatter();
            dataServers = (Hashtable)bf.Deserialize(readMap.BaseStream);

            StreamReader readMap2 = new StreamReader(@"" + strpathFile);
            BinaryFormatter bf2 = new BinaryFormatter();
            files = (Hashtable)bf2.Deserialize(readMap2.BaseStream);

            StreamReader readMap3 = new StreamReader(@"" + strpathNBDS);
            BinaryFormatter bf3 = new BinaryFormatter();
            nBDataS = (Hashtable)bf3.Deserialize(readMap3.BaseStream);

            StreamReader readMap4 = new StreamReader(@"" + strpathDSnum);
            BinaryFormatter bf4 = new BinaryFormatter();
            dataServersNum = (Hashtable)bf4.Deserialize(readMap4.BaseStream);
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
