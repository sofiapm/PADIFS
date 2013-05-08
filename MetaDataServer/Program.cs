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
        public static void writeToDisk(string nomeMeta, MetaServer meta)
        {
            while (true)
            {

                Hashtable dataServers = meta.get_dataServers2();
                Hashtable files = meta.get_files2();
                Hashtable nBDataS = meta.get_nBDataS2();
                SortedDictionary<string, int> dataServersNum = meta.get_DSnum2();
                Hashtable dsF = meta.get_dataServersFiles2();

                try
                {
                    //System.Console.WriteLine("O ms está a escrever para o disco!");

                    string currentDirectory = Environment.CurrentDirectory;
                    string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
                    string strpathDS = newDirectory[0] + "Disk\\" + "InfoDS" + nomeMeta + ".xml";
                    string strpathFile = newDirectory[0] + "Disk\\" + "InfoFiles" + nomeMeta + ".xml";
                    string strpathNBDS = newDirectory[0] + "Disk\\" + "NBDS" + nomeMeta + ".xml";
                    string strpathDSnum = newDirectory[0] + "Disk\\" + "DSnum" + nomeMeta + ".xml";
                    string strpathDSFiles = newDirectory[0] + "Disk\\" + "DSfiles" + nomeMeta + ".xml";

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

                    BinaryFormatter bfw5 = new BinaryFormatter();
                    StreamWriter ws5 = new StreamWriter(@"" + strpathDSFiles);
                    bfw5.Serialize(ws5.BaseStream, dsF);
                    ws5.Close();

                }
                catch //(Exception)
                {
                    //System.Console.WriteLine("[WRITETODISK] O MS está a ler do disco!");
                    //System.Console.WriteLine(e.ToString());
                }
            }
        }

        public static void isAlive(MetaServer meta, Hashtable metaDataServers)
        {

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

        public static void migracao(MetaServer meta, Hashtable metaDataServers)
        {
            while (true)
            {
                SortedDictionary<string, int> dict = meta.get_DSnum2();
                Hashtable dataServersFiles = meta.get_dataServersFiles2();
                Hashtable dataServers = meta.get_dataServers2();

                if (meta.get_migracao() > 0)
                {
                    System.Console.WriteLine("[MIGRACAO] MS entrou na migração de ficheiros...");

                    lock (dict)
                    {
                        foreach (DictionaryEntry entry2 in dataServers)
                        {
                            foreach (DictionaryEntry entry1 in dataServers)
                            {
                                while (dict[(string)entry2.Key] < meta.get_minFiles() &&
                                    dict[(string)entry1.Key] > meta.get_maxFiles())
                                {
                                    System.Console.WriteLine("[MIGRACAO] Fazer migração de " + (string)entry1.Key + " para " + (string)entry2.Key);

                                    IMSToDS ds1 = (IMSToDS)Activator.GetObject(
                                           typeof(IMSToDS),
                                           "tcp://localhost:809" + dataServers[(string)entry1.Key] + "/" + entry1.Key.ToString() + "DataServerMS");

                                    IMSToDS ds2 = (IMSToDS)Activator.GetObject(
                                           typeof(IMSToDS),
                                           "tcp://localhost:809" + dataServers[(string)entry2.Key] + "/" + entry2.Key.ToString() + "DataServerMS");


                                    //ficheiros de entry1
                                    ArrayList auxj = (ArrayList)dataServersFiles[(string)entry1.Key];
                                    ArrayList aux = (ArrayList) auxj.Clone();

                                    //ficheiros de entry2
                                    ArrayList auxi;
                                    try
                                    {
                                        auxi = (ArrayList)dataServersFiles[(string)entry2.Key];
                                        string s = (string)auxi[0];
                                    }
                                    catch
                                    {
                                        auxi = new ArrayList();
                                    }

                                    //para cada ficheiro de entry1
                                    foreach (string file in aux)
                                    {
                                        //se não existir no ds2 e se não estiver open
                                        if (!auxi.Contains(file) && !(((DadosFicheiro)meta.get_files2()[file]).getOpen()))
                                        {
                                            try
                                            {
                                                DadosFicheiroDS f = ds1.readMS(file);
                                                ds2.writeMS(file, f.getFile());
                                                ds1.confirmarDeleteMS(file, true);
                                            }
                                            catch
                                            {
                                                //o ficheiro ainda não foi escrito no DS
                                            }

                                            //retirar de entry1
                                            if (auxj.Contains(file))
                                                auxj.Remove(file);
                                            if (dataServersFiles.ContainsKey(entry1.Key))
                                                dataServersFiles.Remove(entry1.Key);
                                            dataServersFiles.Add(entry1.Key, auxj);
                                            --dict[(string)entry1.Key];

                                            //colocar em entry2
                                            auxi.Add(file);
                                            if (dataServersFiles.ContainsKey(entry2.Key))
                                                dataServersFiles.Remove(entry2.Key);
                                            dataServersFiles.Add(entry2.Key, auxi);
                                            ++dict[(string)entry2.Key];

                                            //propagar a migracao para as replicas
                                            foreach (DictionaryEntry c in metaDataServers)
                                            {
                                                IMSToMS ms = (IMSToMS)Activator.GetObject(
                                                       typeof(IMSToMS),
                                                       "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerMS");

                                                try
                                                {
                                                    ms.migrar((string)entry1.Key, (string)entry2.Key, file);
                                                }
                                                catch
                                                {
                                                }
                                            }

                                            System.Console.WriteLine("[MIGRACAO] MS migrou o ficheiro " + file
                                               + " de " + (string)entry1.Key
                                               + " para " + (string)entry2.Key);

                                            break;
                                        }
                                    }
                                }
                                //break;
                            }
                            //break;
                        }
                        meta.set_migracao(meta.get_migracao() - 1);
                        meta.balancing();

                        System.Console.WriteLine("[MIGRACAO] MS terminou migracao");
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

            MetaServer meta = new MetaServer(channel, args[0], metaDataServers);
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
            string strpathDSFiles = newDirectory[0] + "Disk\\" + "DSFiles" + args[0] + ".xml";
            File.Delete(strpathDS);
            File.Delete(strpathFile);
            File.Delete(strpathNBDS);
            File.Delete(strpathDSnum);
            File.Delete(strpathDSFiles);

            System.Console.WriteLine(args[0] + " MetaDataServer no port: " + args[1]);

            //thread de backup para disco
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            new Thread(delegate()
            {
                writeToDisk(args[0], meta);
            }).Start();

            ManualResetEvent resetEvent2 = new ManualResetEvent(false);
            new Thread(delegate()
            {
                isAlive(meta, metaDataServers);
            }).Start();

            ManualResetEvent resetEvent3 = new ManualResetEvent(false);
            new Thread(delegate()
            {
                migracao(meta, metaDataServers);

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

        //hashtable de dataserversfiles  <string dataserver, Array filename>
        public Hashtable dataServersFiles = new Hashtable();

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

        //flag que indica se é necessário haver migraçao de ficheiros
        public int migracao = 0;

        //numero maximo de ficheiros em cada DS
        public int maxFiles = 0;

        //numero minimo de ficheiros em cada DS
        public int minFiles = 0;

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

        public Hashtable get_dataServersFiles2()
        {
            return dataServersFiles;
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
            if (primary)
                return dict;
            else throw new NullReferenceException();
        }

        public Hashtable get_dataServersFiles()
        {
            if (primary)
                return dataServersFiles;
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

        public bool get_Primary()
        {
            return primary;
        }

        public int get_maxFiles()
        {
            return maxFiles;
        }

        public int get_minFiles()
        {
            return minFiles;
        }

        public int get_migracao()
        {
            return migracao;
        }

        public void set_migracao(int m)
        {
            migracao = m;
        }

        /********Puppet To MetaDataServer***********/
        //the MS stops processing requests from clients or others MS
        public void fail()
        {
            System.Console.WriteLine("[FAIL] MS vai falhar!");
            isFailed = true;
        }

        //MS starts receiving requests from clients and others MS
        public void recover()
        {
            System.Console.WriteLine("[RECOVER] MS vai recuperar!");
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
                    if (ms.get_Primary())
                    {
                        dataServers = ms.get_dataServers();
                        dict = ms.get_DSnum();
                        files = ms.get_files();
                        nBDataS = ms.get_nBDataS();
                        dataServersFiles = ms.get_dataServersFiles();

                        break;
                    }
                }
                catch //(Exception e)
                {
                    if (ms_falhados)
                        primary = true;

                    ms_falhados = true;

                    if (primary)
                        System.Console.WriteLine("[RECOVER] PRIMARY MS");

                }
            }
            System.Console.WriteLine("[RECOVER] MS recuperado");

        }

        //imprimir o estado do MS
        public string dump()
        {
            if (isFailed)
                throw new NullReferenceException();

            System.Console.WriteLine("[DUMP] MS vai fazer Dump");
            String result = "";

            result = result + "[DUMP]***************" + nomeMeta + "***************";
            try
            {
                result = result + "\n[DUMP]---------------DS Registados---------------";
                foreach (DictionaryEntry entry in dataServers)
                    result = result + "\n[DUMP]Nome: " + entry.Key +
                        " NumFiles: " + dict[(string)entry.Key];
                result = result + "\n[DUMP]---------------END DS---------------";

                result = result + "\n[DUMP]--------------Ficheiros--------------";

                foreach (DictionaryEntry entry in files)
                {
                    string aux = "";
                    foreach (DictionaryEntry ds in ((DadosFicheiro)entry.Value).getPorts())
                        aux = aux + "\n[DUMP]NomeDS: " + ds.Key;

                    result = result + "\n[DUMP]Nome: " + entry.Key
                        + " ReadQuórum: " + ((DadosFicheiro)entry.Value).getRQ()
                        + " WriteQuórum: " + ((DadosFicheiro)entry.Value).getWQ()
                        + aux + "\n--------------";
                }
                result = result + "\n[DUMP]------------END Ficheiros------------";
            }
            catch //(Exception e)
            {
                //System.Console.WriteLine("[DUMP] " + e.ToString());
            }

            System.Console.WriteLine(result);

            System.Console.WriteLine("[DUMP] MS fez Dump");
            return result;
        }

        /********Client To MetaDataServer***********/
        //returns to client the contents of the metadata stored for that file
        public DadosFicheiro open(string fileName)
        {
            if (isFailed || !primary)
                throw new NullReferenceException();

            System.Console.WriteLine("[OPEN] MS vai abrir ficheiro: " + fileName);

            try
            {
                DadosFicheiro ds = (DadosFicheiro)files[fileName];
                ds.setOpen(true);

                //envia mensagem para outras replicas
                foreach (DictionaryEntry c in metaDataServers)
                {
                    IMSToMS ms = (IMSToMS)Activator.GetObject(
                            typeof(IMSToMS),
                            "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerMS");
                    try
                    {
                        ms.open_replica(fileName);
                    }
                    catch //(Exception e)
                    {
                    }
                }
                System.Console.WriteLine("[OPEN] MS abriu ficheiro: " + fileName);
                return ds;
            }
            catch
            {
                System.Console.WriteLine("[OPEN] O Ficheiro " + fileName + " não existe.");
                return new DadosFicheiro(0, 0, null, "", 0);
            }
        }

        //informs MS that client is no longer using that file - client must discard all metadata for that file
        public void close(string fileName)
        {
            if (isFailed || !primary)
                throw new NullReferenceException();

            try
            {
                System.Console.WriteLine("[CLOSE] MS vai fechar ficheiro: " + fileName);
                ((DadosFicheiro)files[fileName]).setOpen(false);

                //envia mensagem para outras replicas
                foreach (DictionaryEntry c in metaDataServers)
                {
                    IMSToMS ms = (IMSToMS)Activator.GetObject(
                            typeof(IMSToMS),
                            "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerMS");
                    try
                    {
                        ms.close_replica(fileName);
                    }
                    catch //(Exception e)
                    {
                    }
                }
            }
            catch
            {
                System.Console.WriteLine("[CLOSE] O Ficheiro " + fileName + " não existe.");
            }

            System.Console.WriteLine("[CLOSE] MS fechou ficheiro: " + fileName);
        }

        //creates a new file (if it doesn t exist) - in case of sucesses, returns the same that open
        public DadosFicheiro create(string fileName, int numDS, int rQuorum, int wQuorum)
        {
            if (isFailed || !primary)
                throw new NullReferenceException();

            System.Console.WriteLine("[CREATE] MS vai criar ficheiro: " + fileName);

            Hashtable ports = new Hashtable();
            DadosFicheiro df = new DadosFicheiro(0, 0, null, "", 0);

            if (!files.ContainsKey(fileName))
            {
                System.Console.WriteLine("[CREATE] Numero de DS registados: " + dict.Count + ", Numero de replicas que o cliente quer: " + numDS);

                if (numDS >= dict.Count)
                    foreach (DictionaryEntry entry in dataServers)
                        ports.Add(entry.Key, entry.Value);
                else
                    ports = bestDS(numDS);

                //actualizar valores na dic dos ds
                df = new DadosFicheiro(rQuorum, wQuorum, ports, fileName, numDS);
                files.Add(fileName, df);
                nBDataS.Add(fileName, numDS);

                foreach (DictionaryEntry entry in ports)
                {
                    ++dict[(string)entry.Key];
                    ArrayList aux;
                    try
                    {
                        aux = (ArrayList)dataServersFiles[entry.Key];
                        dataServersFiles.Remove(entry.Key);
                        string f = (string)aux[0];
                    }
                    catch// (Exception)
                    {
                        aux = new ArrayList();
                    }
                    aux.Add(fileName);

                    try
                    {
                        dataServersFiles.Add(entry.Key, aux);
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine(e.ToString());
                    }

                }

                //envia mensagem para outras replicas
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
                        //System.Console.WriteLine(e.ToString());
                    }
                }

                this.balancing();

                String s = "[CREATE] " + fileName + " guardado no DS: ";
                foreach (DictionaryEntry c in df.getPorts())
                    s = s + " " + c.Key;

                System.Console.WriteLine(s);
            }
            else
                System.Console.WriteLine("[CREATE] O ficheiro " + fileName + " já existe!");

            System.Console.WriteLine("[CREATE] MS criou ficheiro: " + fileName);
            return df;
        }

        //deletes the file
        public DadosFicheiro delete(string fileName)
        {
            if (isFailed || !primary)
                throw new NullReferenceException();

            System.Console.WriteLine("[DELETE] MS vai apagar ficheiro: " + fileName);

            DadosFicheiro df = new DadosFicheiro(0, 0, null, "", 0);

            if (files.ContainsKey(fileName))
            {
                df = (DadosFicheiro)files[fileName];

                // Hashtable ports = (Hashtable)((DadosFicheiro)files[fileName]).getPorts();
                // df = new DadosFicheiro(((DadosFicheiro)files[fileName]).getRQ(),
                //    ((DadosFicheiro)files[fileName]).getWQ(),
                //    ports, fileName, ((DadosFicheiro)files[fileName]).getNumDS());
            }
            else System.Console.WriteLine("[DELETE] O ficheiro " + fileName + " não existe!");

            System.Console.WriteLine("[DELETE] MS apagou ficheiro: " + fileName);
            return df;
        }

        public void confirmarDelete(string fileName, bool confirmacao)
        {
            if (isFailed || !primary)
                throw new NullReferenceException();

            System.Console.WriteLine("[DELETE] MS vai confirmar apagar ficheiro: " + fileName);

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
                if (files.ContainsKey(fileName))
                {
                    Hashtable ports = (Hashtable)((DadosFicheiro)files[fileName]).getPorts();

                    files.Remove(fileName);
                    nBDataS.Remove(fileName);

                    //decrementar todos os ds onde o file estava
                    foreach (DictionaryEntry entry in ports)
                    {
                        --dict[(string)entry.Key];
                        ArrayList aux = (ArrayList)(dataServersFiles[entry.Key]);
                        if (aux.Contains(fileName))
                            aux.Remove(fileName);

                        if (dataServersFiles.ContainsKey(entry.Key))
                            dataServersFiles.Remove(entry.Key);
                        dataServersFiles.Add(entry.Key, aux);
                    }

                    this.balancing();
                }
                else System.Console.WriteLine("[DELETE] O ficheiro " + fileName + " não existe!");
            }

            System.Console.WriteLine("[DELETE] MS confirmou apagar ficheiro: " + fileName);
        }

        /********DS To MetadataServer***********/
        //registar DS no MS
        public void registarDS(string name, string id)
        {
            if (isFailed || !primary)
                throw new NullReferenceException();

            System.Console.WriteLine("[REGISTARDS] MS vai registar DS: " + name);

            //envia mensagem para outras replicas
            if (primary)
            {
                lock (dict)
                {

                    if (!dataServers.ContainsKey(name))
                    {
                        ArrayList filesDS = new ArrayList();
                        dataServers.Add(name, id);
                        dict.Add(name, 0);

                        foreach (DictionaryEntry entry in files)
                            if (((DadosFicheiro)entry.Value).getPorts().Count < (int)nBDataS[(string)entry.Key])
                            {
                                Hashtable p = ((DadosFicheiro)entry.Value).getPorts();
                                p.Add(name, id);
                                ((DadosFicheiro)entry.Value).setPorts(p);

                                filesDS.Add(entry.Key);
                                ++dict[name];

                                ArrayList aux;
                                try
                                {
                                    aux = (ArrayList)dataServersFiles[name];
                                    dataServersFiles.Remove(name);
                                    string f = (string)aux[0];

                                }
                                catch //(Exception)
                                {
                                    aux = new ArrayList();
                                }

                                aux.Add(entry.Key);
                                dataServersFiles.Add(name, aux);

                                System.Console.WriteLine("[REGISTARDS] O ficheiro " + entry.Key + " foi guardado no DS " + name);
                            }

                        foreach (DictionaryEntry c in metaDataServers)
                        {
                            IMSToMS ms = (IMSToMS)Activator.GetObject(
                                   typeof(IMSToMS),
                                   "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerMS");

                            try
                            {
                                ms.registarDS_replica(name, id, filesDS);
                                System.Console.WriteLine("[REGISTARDS] Primario contactou com sucesso o MS: " + c.Value.ToString() + " E " + c.Key.ToString());
                            }
                            catch //(Exception e)
                            {
                            }
                        }

                        this.balancing();

                        if (dict[name] < minFiles)
                        {
                            System.Console.WriteLine("[REGISTARDS] Fazer a migração...");
                            ++migracao;
                        }
                    }
                    else System.Console.WriteLine("[REGISTARDS] O DS " + name + " já está registado");
                }

                System.Console.WriteLine("[REGISTARDS] MS registou DS: " + name);
            }
        }

        /*************replicas***************/
        public void open_replica(string fileName)
        {
            if (isFailed || primary)
                throw new NullReferenceException();

            //System.Console.WriteLine("[OPEN] MS vai abrir ficheiro: " + fileName);

            try
            {
                ((DadosFicheiro)files[fileName]).setOpen(true);

            }
            catch
            {
                //System.Console.WriteLine("[OPEN] O Ficheiro " + fileName + " não existe.");
            }
            //System.Console.WriteLine("[OPEN] MS abriu ficheiro: " + fileName);
        }

        public void close_replica(string fileName)
        {
            if (isFailed || primary)
                throw new NullReferenceException();

            //System.Console.WriteLine("[OPEN] MS vai fechar o fcheiro " + fileName );

            try
            {
                ((DadosFicheiro)files[fileName]).setOpen(false);

            }
            catch
            {
                //System.Console.WriteLine("[OPEN] O Ficheiro " + fileName + " não existe.");
            }

            //System.Console.WriteLine("[OPEN] MS fechou o fcheiro " + fileName );
        }

        public void create_replica(DadosFicheiro file, int numDS)
        {
            if (isFailed || primary)
                throw new NullReferenceException();

            //System.Console.WriteLine("[CREATE] MS vai criar ficheiro: " + file.getName());

            if (!files.ContainsKey(file.getName()))
            {
                files.Add(file.getName(), file);
                nBDataS.Add(file.getName(), numDS);

                //actualizar valores na dic dos ds                
                foreach (DictionaryEntry entry in file.getPorts())
                {
                    ++dict[(string)entry.Key];
                    ArrayList aux;
                    try
                    {
                        aux = (ArrayList)dataServersFiles[entry.Key];
                        dataServersFiles.Remove(entry.Key);
                        string f = (string)aux[0];
                    }
                    catch
                    {
                        aux = new ArrayList();
                    }

                    aux.Add(file);
                    dataServersFiles.Add(entry.Key, aux);
                }

                this.balancing();
            }
            //else
            //System.Console.WriteLine("[CREATE] O ficheiro " + file.getName() + " já existe!");

            //System.Console.WriteLine("[CREATE] MS criou ficheiro: " + file.getName());
        }

        public void registarDS_replica(string name, string id, ArrayList filesDS)
        {
            if (isFailed || primary)
                throw new NullReferenceException();

            //System.Console.WriteLine("[REGISTARDS] MS vai registar o DS: " + name);

            if (!dataServers.ContainsKey(name))
            {
                dataServers.Add(name, id);
                dict.Add(name, 0);

                foreach (DictionaryEntry entry in filesDS)
                {
                    Hashtable p = ((DadosFicheiro)entry.Value).getPorts();
                    p.Add(name, id);
                    ((DadosFicheiro)entry.Value).setPorts(p);

                    ++dict[name];

                    ArrayList aux;
                    try
                    {
                        aux = (ArrayList)dataServersFiles[name];
                        dataServersFiles.Remove(name);
                        string f = (string)aux[0];
                    }
                    catch
                    {
                        aux = new ArrayList();
                    }

                    aux.Add(entry.Key);
                    dataServersFiles.Add(name, aux);

                    //System.Console.WriteLine("[REGISTARDS] O ficheiro " + entry.Key + " foi guardado no DS " + name);
                }
                this.balancing();
            }
            //else System.Console.WriteLine("[REGISTARDS] O DS " + name + " já está registado"); 

            //System.Console.WriteLine("[REGISTARDS] MS registou o DS: " + name);
        }

        public void confirmarDelete_replica(string fileName, bool confirmacao)
        {
            if (isFailed || primary)
                throw new NullReferenceException();

            //System.Console.WriteLine("[DELETE] MS vai confirmar apagar ficheiro: " + fileName);

            if (confirmacao)
            {
                if (files.ContainsKey(fileName))
                {
                    Hashtable ports = (Hashtable)((DadosFicheiro)files[fileName]).getPorts();

                    files.Remove(fileName);
                    nBDataS.Remove(fileName);

                    //decrementar todos os ds onde o file estava
                    foreach (DictionaryEntry entry in ports)
                    {
                        --dict[(string)entry.Key];
                        ArrayList aux = (ArrayList)(dataServersFiles[entry.Key]);
                        if (aux.Contains(fileName)) aux.Remove(fileName);
                        dataServersFiles.Remove(entry.Key);
                        dataServersFiles.Add(entry.Key, aux);
                    }

                    this.balancing();
                }
                // else System.Console.WriteLine("[DELETE] O ficheiro " + fileName + " não existe!");
            }

            //System.Console.WriteLine("[DELETE] MS confirmaou apagar ficheiro: " + fileName);
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
                    ports.Add(entry.Key, dataServers[entry.Key]);
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

        public bool areYouAlive()
        {
            if (!primary)
                throw new NullReferenceException();
            else
            {
                if (isFailed)
                    primary = false;
                return !isFailed;
            }
        }

        public void balancing()
        {
            try
            {
                int numFiles = 0;
                int numDS = 0;
                foreach (KeyValuePair<string, int> i in dict)
                {
                    numFiles = numFiles + i.Value;
                    numDS++;
                }

                maxFiles = (int)Math.Ceiling((double)numFiles / (double)numDS);
                minFiles = (int)Math.Floor((double)numFiles / (double)numDS);

                // System.Console.WriteLine("Balancing" + " MaxFiles: " + maxFiles + " MinFiles: " + minFiles);
            }
            catch { }
        }

        //migra o ficheiro filename do ds1 para o ds2
        public void migrar(string ds1, string ds2, string filename)
        {
            if (!primary && !isFailed)
            {
                ArrayList auxj = (ArrayList)dataServersFiles[ds1];

                //retirar de ds1
                if (auxj.Contains(filename))
                    auxj.Remove(filename);

                if (dataServersFiles.ContainsKey(ds1))
                    dataServersFiles.Remove(ds1);

                dataServersFiles.Add(ds1, auxj);
                --dict[ds1];

                //colocar em ds2
                ArrayList auxi;
                try
                {
                    auxi = (ArrayList)dataServersFiles[ds2];
                    auxi.Add(filename);
                }
                catch
                {
                    auxi = new ArrayList();
                    auxi.Add(filename);
                }

                if (dataServersFiles.ContainsKey(ds2))
                    dataServersFiles.Remove(ds2);

                dataServersFiles.Add(ds2, auxi);
                ++dict[ds2];
            }

        }
    }

    class MetaServerMS : MarshalByRefObject, IMSToMS
    {
        public static MetaServer ctx;

        //close a file
        public void close_replica(string file)
        {
            ctx.close_replica(file);
        }

        //open a file
        public void open_replica(string file)
        {
            ctx.open_replica(file);
        }

        //creates a new file (if it doesn t exist)
        public void create_replica(DadosFicheiro file, int numDS)
        {
            ctx.create_replica(file, numDS);
        }

        //regista o DS no MS
        public void registarDS_replica(string nome, string ID, ArrayList f)
        {
            ctx.registarDS_replica(nome, ID, f);
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

        public Hashtable get_dataServersFiles()
        {
            return ctx.get_dataServersFiles();
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

        public bool get_Primary()
        {
            return ctx.get_Primary();
        }

        public void fail()
        {
            ctx.fail();
        }

        public void migrar(string d1, string d2, string file)
        {
            ctx.migrar(d1, d2, file);
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
        public string dump()
        {
            return ctx.dump();
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
