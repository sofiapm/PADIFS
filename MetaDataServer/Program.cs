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
        public static void  writeToDisk(string nomeMeta, Hashtable dataServers, Hashtable files, Hashtable nBDataS, SortedDictionary<string, int> dataServersNum)
        {
            while (true)
                try
                {
                    //System.Console.WriteLine("O ms está a escrever para o disco!");

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
                    //System.Console.WriteLine("[WRITETODISK] O MS está a ler do disco!");
                    //System.Console.WriteLine(e.ToString());
                }
        }
       
        public static void isAlive(MetaServer meta, Hashtable metaDataServers){

            while (true)
            {
                foreach (DictionaryEntry c in metaDataServers)
                {
                    IMSToMS ms = (IMSToMS)Activator.GetObject(
                           typeof(IMSToMS),
                           "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerMS");

                    try
                    {

                        if (!meta.get_failed() && !ms.areYouAlive())
                        {
                            meta.setPrimary();
                            //ms.fail();
                        }
                    }
                    catch
                    {
                    }
                }

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

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(MetaServerMS),
            args[0] + "MetaServerMS", WellKnownObjectMode.Singleton);

            //hash dos outros metadatas
            Hashtable metaDataServers = new Hashtable();
            if (!args[0].Equals("m-0"))
                metaDataServers.Add("1", "m-0");
            if (!args[0].Equals("m-1"))
                metaDataServers.Add("2", "m-1");
            if (!args[0].Equals("m-2"))
                metaDataServers.Add("3", "m-2");

            MetaServer meta = new MetaServer (channel, args[0], metaDataServers);
            MetaServerPuppet.ctx = meta;
            MetaServerClient.ctx = meta;
            MetaServerDS.ctx = meta;
            MetaServerMS.ctx = meta;

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

            System.Console.WriteLine(args[0] + " MetaDataServer no porto: " + args[1]);

            //thread de backup para disco
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            new Thread(delegate()
            {               
                String m = args[0];
                Hashtable ds = meta.get_dataServers2();
                Hashtable f = meta.get_files2();
                Hashtable nDS = meta.get_nBDataS2();
                SortedDictionary<string, int> DSn = meta.get_DSnum2();
                writeToDisk(m, ds, f, nDS, DSn);
            }).Start();

            ManualResetEvent resetEvent2 = new ManualResetEvent(false);
            new Thread(delegate()
            {
                isAlive(meta, metaDataServers);
            }).Start();

            System.Console.ReadLine();
        }

    }

    class MetaServer
    {
        //canal de comunicação
        TcpChannel channel;

        //nome do meta
        public string nomeMeta;

        //hashtable dos outros metaDatas
        public Hashtable metaDataServers = new Hashtable();

        //hashtable de dataserververs <string nome, string ID>
        public Hashtable dataServers = new Hashtable();

        //hashtable com o numero de ficheiros que cada ds tem <string nome, int num>
        public SortedDictionary<string, int> dict = new SortedDictionary<string, int>();

        //hashtable de ficheiros <string filename, DadosFicheiro dados>
        public Hashtable files = new Hashtable();
 
        //hashtable de NBDataS <string filename, int ds>
        public Hashtable nBDataS = new Hashtable();

        //flag activa quando o metadata server esta em fail
        public bool isFailed = false;

        //flag que indica se é ou não o primario
        public bool primary = false;

        //construtor, recebe o canal e o nome
        public MetaServer(TcpChannel channel, String nome, Hashtable mdservers)
        {
            this.channel = channel;
            this.nomeMeta = nome;
            this.metaDataServers = mdservers;
        }

        /****************Gets******************/
        public Hashtable get_dataServers2()
        {
            return dataServers;
        }

        public Hashtable get_files2()
        {
            return files;
        }

        public Hashtable get_nBDataS2()
        {
            return nBDataS;
        }

        public SortedDictionary<string, int> get_DSnum2()
        {
            return dict;
        }

        public Hashtable get_dataServers()
        {
            if (primary)
            return dataServers;
            else throw new NullReferenceException();
        }

        public Hashtable get_files()
        {
            if (primary)
            return files;
            else throw new NullReferenceException();
        }

        public Hashtable get_nBDataS()
        {
            if (primary)
            return nBDataS;
            else throw new NullReferenceException();
        }

        public SortedDictionary<string, int> get_DSnum()
        {
            if(primary)
            return dict;
            else throw new NullReferenceException();
        }

        public bool get_failed()
        {
            return isFailed;
        }

        public void setPrimary()
        {
            if (!primary)
            {
                primary = true;
                System.Console.WriteLine("Passou a ser o PRIMARY");
            }
        }

        public bool areYouAlive()
        {
            if (!primary)
                throw new NullReferenceException();
            else {
                if (isFailed)
                    primary = false;
                return !isFailed;
            }
        }

        /********Puppet To MetaDataServer***********/
        //the MS stops processing requests from clients or others MS
        public void fail()
        {
            System.Console.WriteLine("[FAIL] Puppet mandou MS falhar!");
            isFailed = true;
            //primary = false;
        }

        //MS starts receiving requests from clients and others MS
        public void recover()
        {
            System.Console.WriteLine("[RECOVER] Puppet mandou MS recuperar!");
            isFailed = false;
            bool ms_falhados = false;
            primary = false;

            try
            {
                readFromDisk();
            }
            catch
            {
                //System.Console.WriteLine("[RECOVER] Não existe nenhum ficheiro em disco.");
            }

            //envia mensagem para outras replicas
            foreach (DictionaryEntry c in metaDataServers)
                {
                    IMSToMS ms = (IMSToMS)Activator.GetObject(
                           typeof(IMSToMS),
                           "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerMS");

                    try
                    {
                        dataServers = ms.get_dataServers();
                        dict = ms.get_DSnum();
                        files = ms.get_files();
                        nBDataS = ms.get_nBDataS();

                        break;
                    }
                    catch (Exception e)
                    {
                        if (ms_falhados)
                            primary = true;
                      
                        ms_falhados = true;

                        if (primary)
                            System.Console.WriteLine("[RECOVER] PRIMARY MS");
                       
                    }
                }

        }

        //imprimir o estado do MS
        public void dump()
        {
            if (isFailed)
                throw new NullReferenceException();

            System.Console.WriteLine("[DUMP] Puppet mandou o MS fazer Dump");
            String result = "";

            try
            {
                result = result + "[DUMP]***************DS Registados***************";
                foreach (DictionaryEntry entry in dataServers)
                    result = result + "\n[DUMP]Nome: " + entry.Key +
                        " ID: " + entry.Value +
                        " NumFiles: " + dict[(string)entry.Key]; 
                result = result + "\n[DUMP]******************END DS*******************";

                result = result + "\n[DUMP]*****************Ficheiros*****************";

                foreach (DictionaryEntry entry in files)
                {
                    string aux = "";
                    foreach (DictionaryEntry ds in ((DadosFicheiro)entry.Value).getPorts())
                        aux = aux + "\n[DUMP]NomeDS: " + ds.Key + " IDDS: " + ds.Value;

                    result = result + "\n[DUMP]Nome: " + entry.Key
                        + " ReadQuórum: " + ((DadosFicheiro)entry.Value).getRQ()
                        + " WriteQuórum: " + ((DadosFicheiro)entry.Value).getWQ()
                        + aux + "\n--------------";
                }
                result = result + "\n[DUMP]***************END Ficheiros***************";
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
            if (isFailed || !primary)
                throw new NullReferenceException();

            System.Console.WriteLine("[OPEN] Cliente mandou MS abrir ficheiro: " + fileName);

            try
                {
                    return (DadosFicheiro)files[fileName];
                }
                catch
                {
                    System.Console.WriteLine("[OPEN] O Ficheiro " + fileName + " não existe.");
                    return new DadosFicheiro(0, 0, null, "",0);
                }
        }

        //informs MS that client is no longer using that file - client must discard all metadata for that file
        public void close(string fileName)
        {
            if (isFailed || !primary)
                throw new NullReferenceException();
                      
            if (files.ContainsKey(fileName))
                System.Console.WriteLine("[CLOSE] Cliente mandou MS fechar ficheiro: " + fileName);
            else
                System.Console.WriteLine("[CLOSE] O ficheiro: " + fileName + " não existe!");
        }

        //creates a new file (if it doesn t exist) - in case of sucesses, returns the same that open
        public DadosFicheiro create(string fileName, int numDS, int rQuorum, int wQuorum)
        {
            if (isFailed || !primary)
                throw new NullReferenceException();

            System.Console.WriteLine("[CREATE] Cliente mandou MS criar ficheiro: " + fileName);

            Hashtable ports = new Hashtable();
            DadosFicheiro df = new DadosFicheiro(0, 0, null, "",0);
            if (!files.ContainsKey(fileName))
            {
                System.Console.WriteLine("[CREATE] Numero de DS registados: " + dataServers.Count + ", Numero de replicas que o cliente quer: " + numDS);
                
                if (numDS >= dataServers.Count)
                    foreach (DictionaryEntry entry in dataServers)
                        ports.Add(entry.Key, entry.Value);
                else
                    ports = bestDS(numDS);

                //actualizar valores na dic dos ds
                foreach (DictionaryEntry entry in ports)
                    ++dict[(string)entry.Key];
                                
                df = new DadosFicheiro(rQuorum, wQuorum, ports, fileName, numDS);
                files.Add(fileName, df);
                nBDataS.Add(fileName, numDS);


                //envia mensagem para outras replicas
                if (primary)
                    foreach (DictionaryEntry c in metaDataServers)
                    {
                        IMSToMS ms = (IMSToMS)Activator.GetObject(
                               typeof(IMSToMS),
                               "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerMS");
                     
                        try
                        {
                            ms.create_replica(df, numDS);
                        }
                        catch //(Exception e)
                        {
                        }
                    }

                System.Console.WriteLine("[CREATE]********Stored in DS********");
                foreach (DictionaryEntry c in df.getPorts())
                    System.Console.WriteLine("[CREATE] Nome: " + c.Key + " ID: " + c.Value);
                System.Console.WriteLine("[CREATE]***********DS-END***********");
            }
            else
                System.Console.WriteLine("[CREATE] O ficheiro " + fileName + " já existe!");

            return df;
        }

        //deletes the file
        public DadosFicheiro delete(string fileName)
        {
            if (isFailed || !primary)
                throw new NullReferenceException();

            System.Console.WriteLine("[DELETE] Cliente mandou MS apagar ficheiro: " + fileName);

            DadosFicheiro df = new DadosFicheiro(0, 0, null,"",0);

            if (files.ContainsKey(fileName))
            {
                Hashtable ports = (Hashtable)((DadosFicheiro)files[fileName]).getPorts().Clone();
                df = new DadosFicheiro (((DadosFicheiro)files[fileName]).getRQ(),
                    ((DadosFicheiro)files[fileName]).getWQ(),
                    ports, fileName, ((DadosFicheiro)files[fileName]).getNumDS());
            }
            else System.Console.WriteLine("[DELETE] O ficheiro " + fileName + " não existe!");

            return df;
        }

        public void confirmarDelete(string fileName, bool confirmacao)
        {
            if (isFailed || !primary)
                throw new NullReferenceException();

            System.Console.WriteLine("[DELETE] Cliente confirmou apagar ficheiro: " + fileName);

            //envia mensagem para outras replicas
            foreach (DictionaryEntry c in metaDataServers)
            {
                IMSToMS ms = (IMSToMS)Activator.GetObject(
                        typeof(IMSToMS),
                        "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerMS");
                try
                {
                    ms.confirmarDelete_replica(fileName, confirmacao);
                }
                catch //(Exception e)
                {
                }
            }

            if (confirmacao)
            {
                DadosFicheiro df = new DadosFicheiro(0, 0, null, "",0);

                if (files.ContainsKey(fileName))
                {
                    Hashtable ports = (Hashtable)((DadosFicheiro)files[fileName]).getPorts().Clone();
                    df = new DadosFicheiro(((DadosFicheiro)files[fileName]).getRQ(),
                        ((DadosFicheiro)files[fileName]).getWQ(),
                        ports, fileName, ((DadosFicheiro)files[fileName]).getNumDS());

                    files.Remove(fileName);
                    nBDataS.Remove(fileName);

                    //decrementar todos os ds onde o file estava
                    foreach (DictionaryEntry entry in ports)
                        --dict[(string)entry.Key];
                }
                else System.Console.WriteLine("[DELETE] O ficheiro " + fileName + " não existe!");   
            }
        }

        /********DS To MetadataServer***********/
        //resposta de confirmação do DS
        public void respostaDS(string resposta)
        {
            if (isFailed)
                throw new NullReferenceException();
            
            System.Console.WriteLine(resposta);
        }

        //registar DS no MS
        public void registarDS(string name, string id)
        {
            if (isFailed || !primary)
                throw new NullReferenceException();

            System.Console.WriteLine("[REGISTARDS] MS registou DS: " + name);

            //envia mensagem para outras replicas
            if (primary)
                foreach (DictionaryEntry c in metaDataServers)
                {
                    IMSToMS ms = (IMSToMS)Activator.GetObject(
                           typeof(IMSToMS),
                           "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerMS");
                    
                    try
                    {
                        ms.registarDS_replica(name,id);
                        System.Console.WriteLine("[REGISTARDS] Primario contactou com sucesso o MS: " + c.Value.ToString() + " E " + c.Key.ToString());
                    }
                    catch //(Exception e)
                    {
                    }
                }

            if (!dataServers.ContainsKey(name))
            {
                dataServers.Add(name,id);
                dict.Add(name, 0);

                foreach (DictionaryEntry entry in files)
                    if (((DadosFicheiro)entry.Value).getPorts().Count < (int)nBDataS[(string)entry.Key])
                    {
                        ((DadosFicheiro)entry.Value).getPorts().Add(name, id);
                        ++dict[name];
                        System.Console.WriteLine("[REGISTARDS] O ficheiro " + entry.Key + " foi guardado no DS " + name);
                    }

            }
            else System.Console.WriteLine("[REGISTARDS] O DS " + name + " já está registado");               
        }

        /*************replicas***************/
        public void create_replica(DadosFicheiro file, int numDS)
        { 
            if (isFailed || primary)
                throw new NullReferenceException();

            System.Console.WriteLine("[CREATE] Cliente mandou MS criar ficheiro: " + file.getName());

            if (!files.ContainsKey(file.getName()))
            {
                files.Add(file.getName(), file);
                nBDataS.Add(file.getName(), numDS);

                //actualizar valores na dic dos ds
                foreach (DictionaryEntry entry in file.getPorts())
                    ++dict[(string)entry.Key];
                           
                System.Console.WriteLine("[CREATE]********Stored in DS********");
                foreach (DictionaryEntry c in file.getPorts())
                    System.Console.WriteLine("[CREATE] Nome: " + c.Key + " ID: " + c.Value);
                System.Console.WriteLine("[CREATE]***********DS-END***********");
            }
            else
                System.Console.WriteLine("[CREATE] O ficheiro " + file.getName() + " já existe!");
        }

        public void registarDS_replica(string name, string id)
        {
            if (isFailed || primary)
                throw new NullReferenceException();

            System.Console.WriteLine("[REGISTARDS] MS registou DS: " + name);

            if (!dataServers.ContainsKey(name))
            {
                dataServers.Add(name, id);
                dict.Add(name, 0);

                foreach (DictionaryEntry entry in files)
                    if (((DadosFicheiro)entry.Value).getPorts().Count < (int)nBDataS[(string)entry.Key])
                    {
                        ((DadosFicheiro)entry.Value).getPorts().Add(name, id);
                        ++dict[name];
                        System.Console.WriteLine("[REGISTARDS] O ficheiro " + entry.Key + " foi guardado no DS " + name);
                    }

            }
            else System.Console.WriteLine("[REGISTARDS] O DS " + name + " já está registado");  
        }

        public void confirmarDelete_replica(string fileName, bool confirmacao)
        {
            if (isFailed || primary)
                throw new NullReferenceException();

            System.Console.WriteLine("[DELETE] Cliente confirmou apagar ficheiro: " + fileName);

            if (confirmacao)
            {
                DadosFicheiro df = new DadosFicheiro(0, 0, null, "", 0);

                if (files.ContainsKey(fileName))
                {
                    Hashtable ports = (Hashtable)((DadosFicheiro)files[fileName]).getPorts().Clone();
                    df = new DadosFicheiro(((DadosFicheiro)files[fileName]).getRQ(),
                        ((DadosFicheiro)files[fileName]).getWQ(),
                        ports, fileName, ((DadosFicheiro)files[fileName]).getNumDS());

                    files.Remove(fileName);
                    nBDataS.Remove(fileName);

                    //decrementar todos os ds onde o file estava
                    foreach (DictionaryEntry entry in ports)
                        --dict[(string)entry.Key];
                }
                else System.Console.WriteLine("[DELETE] O ficheiro " + fileName + " não existe!");
            }
        }

        /*************Funçoes auxiliares**************/
        //retorna os num DS com menos files guardados
        public Hashtable bestDS(int num)
        {
            Hashtable ports = new Hashtable(); 

            //escolher num DSs
            Dictionary<string, int> nha = dict.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
           
                foreach (KeyValuePair<string, int> entry in nha)
                {
                    if (num == ports.Count)
                        break;
                    else if (!ports.ContainsKey(entry.Key))
                        ports.Add(entry.Key,dataServers[entry.Key]);
                }
            
             return ports;
        }

        //ler informação do disco
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
            dict = (SortedDictionary<string, int>)bf4.Deserialize(readMap4.BaseStream);
        }

    }

    class MetaServerMS : MarshalByRefObject, IMSToMS
    {
        public static MetaServer ctx;

        //creates a new file (if it doesn t exist) - in case of sucesses, returns the same that open
        public void create_replica(DadosFicheiro file, int numDS)
        {
            ctx.create_replica(file, numDS);
        }

        //regista o DS no MS
        public void registarDS_replica(string nome, string ID)
        {
            ctx.registarDS_replica(nome, ID);
        }

        //confirmar o delete
        public void confirmarDelete_replica(string filename, bool confirmacao)
        {
            ctx.confirmarDelete_replica(filename, confirmacao);
        }

        //gets
        public Hashtable get_dataServers()
        {
            return ctx.get_dataServers();
        }

        public SortedDictionary<string, int> get_DSnum()
        {
            return ctx.get_DSnum();
        }

        public Hashtable get_files()
        {
            return ctx.get_files();
        }

        public Hashtable get_nBDataS()
        {
            return ctx.get_nBDataS();
        }

        //ack de confirmacao
        public bool areYouAlive()
        {
            return ctx.areYouAlive();
        }

        public void fail()
        {
            ctx.fail();
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

        //imprime o estado do MS
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

        //confirmar delete
        public void confirmarDelete(string filename, bool confirmacao)
        {
            ctx.confirmarDelete(filename, confirmacao);
        }
    }

    class MetaServerDS : MarshalByRefObject, IDSToMS
    {
        public static MetaServer ctx;

        public void respostaDS(string resposta)
        {
            ctx.respostaDS(resposta);
        }

        //regista o DS no MS
        public void registarDS(string nome, string ID)
        {
            ctx.registarDS(nome, ID);
        }

        //confirmação de que o delete foi executado
        public void delete(string fileName)
        {
            ctx.delete(fileName);
        }
    }
}
