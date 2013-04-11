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
using System.Threading;

namespace Client
{   
    class Program //: MarshalByRefObject, IDSToClient, IMSToClient
    {

        static void Main(string[] args)
        {
            TcpChannel channel;


            channel = new TcpChannel(Int32.Parse(args[1]));
            ChannelServices.RegisterChannel(channel, false);

            System.Console.WriteLine("************Cliente " + args[0] + " no port: " + args[1] + "************");


            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(PuppetClient),
            args[0] + "PuppetClient",
            WellKnownObjectMode.Singleton);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(DSClient),
            args[0] + "DSClient",
            WellKnownObjectMode.Singleton);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(MSClient),
            args[0] + "MSClient",
            WellKnownObjectMode.Singleton);

            Hashtable metaDataServers = new Hashtable();
            metaDataServers.Add("1", "m-0");
            metaDataServers.Add("2", "m-1");
            metaDataServers.Add("3", "m-2");
            
            Hashtable dataServers = new Hashtable();

            Cliente cliente = new Cliente(channel, metaDataServers, dataServers);
            PuppetClient.ctx = cliente;
            DSClient.ctx = cliente;
            MSClient.ctx = cliente;

            System.Console.ReadLine();
        }

    }


    class Cliente
    {
        private static TcpChannel channel;
        public Hashtable metaDataServers;
        //public Hashtable dataServers;
        //string nome, DadosFicheiro
        public Hashtable ficheiroInfo = new Hashtable();
        //string id, string nome
        public Hashtable fileRegister = new Hashtable();
        //string id, string conteudo
        public Hashtable arrayRegister = new Hashtable();
        //string nome, int versao
        public Hashtable versao = new Hashtable();

        int keyArrayRegister = 0;
        int keyFileRegister = 0;

        public Cliente (TcpChannel canal, Hashtable metaServers, Hashtable dataServer)
        {
            channel=canal;
            this.metaDataServers = metaServers;
            //this.dataServers = dataServer;
        }
        
        /********Puppet To Client***********/
        //puppet manda o cliente enviar pedidos ao MS
        public void open(string fileName)
        {
            DadosFicheiro fileData = null;
            foreach (DictionaryEntry c in metaDataServers)
            {
                IClientToMS ms = (IClientToMS)Activator.GetObject(
                       typeof(IClientToMS),
                       "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerClient");
                System.Console.WriteLine("[OPEN] Vou tentar falar com: " + c.Value.ToString());

                try
                {
                    fileData = ms.open(fileName);
                    System.Console.WriteLine("[OPEN]: Cliente contctou com sucesso o MS: " + c.Value.ToString() + " E " + c.Key.ToString());
                    break;
                }
                catch //(Exception e)
                {
                   // System.Console.WriteLine(e.ToString());
                    System.Console.WriteLine("[OPEN]: Não conseguiu aceder ao MS: " + c.Value.ToString() + " E " + c.Key.ToString());
                }
            }

            bool temDS = true;

            try{fileData.getPorts();}
            catch (NullReferenceException e){temDS = false;}

            if (!temDS)
            {
                System.Console.WriteLine("[OPEN]: Não Consegue abrir o ficheiro: " + fileName);
            }
            else
            {
                if (ficheiroInfo.Contains(fileName))
                {
                    ficheiroInfo.Remove(fileName);
                }

                ficheiroInfo.Add(fileName, fileData);

                if (fileRegister.Contains(keyFileRegister))
                {
                    fileRegister.Remove(keyFileRegister);
                }
                fileRegister.Add(keyFileRegister, fileName);
                keyFileRegister++;
                

            }
   
        }

        public void close(string fileName)
        {
            if (ficheiroInfo.Contains(fileName))
            {
                foreach (DictionaryEntry c in metaDataServers)
                {
                    IClientToMS ms = (IClientToMS)Activator.GetObject(
                           typeof(IClientToMS),
                           "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerClient");
                    try
                    {
                        ms.close(fileName);
                        break;
                    }
                    catch
                    {
                        System.Console.WriteLine("[CLOSE]: Não conseguiu aceder ao MS: " + c.Key.ToString() + " E " + c.Value.ToString());
                    }
                }


                ficheiroInfo.Remove(fileName);

                System.Console.WriteLine("[CLOSE]: Mandou Ms fechar file: " + fileName);
            }
            else
            {
                System.Console.WriteLine("[CLOSE]: O ficheiro nao esta aberto: " + fileName);
            }
        }

        public void create(string fileName, int numDS, int rQuorum, int wQuorum)
        {
            DadosFicheiro fileData = null;
            foreach (DictionaryEntry c in metaDataServers)
            {
                IClientToMS ms = (IClientToMS)Activator.GetObject(
                       typeof(IClientToMS),
                       "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerClient");
                try
                {
                    fileData = ms.create(fileName, numDS, rQuorum, wQuorum);
                    System.Console.WriteLine("[CREATE] Contactou com sucesso " + c.Value.ToString());
                    break;
                }
                catch //( Exception e)
                {
                    //System.Console.WriteLine(e.ToString());
                    System.Console.WriteLine("[CREATE]: Não conseguiu aceder ao MS: " + c.Key.ToString() + " - " + c.Value.ToString());
                }
            }

            bool temDS = true;

            try{ fileData.getPorts(); }
            catch (NullReferenceException e){ temDS = false; }

            if (!temDS)
            {
                System.Console.WriteLine("[CREATE]: Nao conseguiu criar o ficheiro!");
            }
            else
            {
                if (ficheiroInfo.Contains(fileName))
                {
                    ficheiroInfo.Remove(fileName);
                }

                ficheiroInfo.Add(fileName, fileData);
            }

            //System.Console.WriteLine("Mandou Ms criar file");
        }

        public void delete(string fileName)
        {
            DadosFicheiro dados=null;
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            foreach (DictionaryEntry c in metaDataServers)
            {
                 IClientToMS ms = (IClientToMS)Activator.GetObject(
                          typeof(IClientToMS),
                           "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerClient");
                 try
                 {
                     dados = ms.delete(fileName);
                     System.Console.WriteLine("[DELETE]:  Mandou Ms apagar file: " + fileName);
                     break;
                 }
                 catch
                 {
                     System.Console.WriteLine("[DELETE]: Não conseguiu aceder ao MS: " + c.Key.ToString() + " E " + c.Value.ToString());
                 }
             }

            bool temDS = true;

            try{ dados.getPorts(); }
            catch (NullReferenceException e){ temDS = false; }

            if (!temDS)
            {
                System.Console.WriteLine("[DELETE]: Nao tem DS para mandar o DELETE!");
            }
            else
            {
                bool consegueApagar = true;
                int idDados = 0;
                foreach (DictionaryEntry c in dados.getPorts())
                {
                    IClientToDS ds = (IClientToDS)Activator.GetObject(
                       typeof(IClientToDS),
                       "tcp://localhost:809" + c.Value.ToString() + "/" + c.Key.ToString() + "dataServerClient");
                    try
                    {
                        new Thread(delegate()
                        {
                            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

                            if (!ds.delete(fileName))
                                consegueApagar = false;
                            idDados++;
                           
                            if (idDados == dados.getPorts().Count)
                                resetEvent.Set();

                        }).Start();

                        //break;
                    }
                    catch
                    {
                        System.Console.WriteLine("[READthreads]: Não conseguiu aceder ao DS");
                    }
                }
                resetEvent.WaitOne();

                resetEvent = new ManualResetEvent(false);
                foreach (DictionaryEntry c in dados.getPorts())
                {
                    IClientToDS ds = (IClientToDS)Activator.GetObject(
                       typeof(IClientToDS),
                       "tcp://localhost:809" + c.Value.ToString() + "/" + c.Key.ToString() + "dataServerClient");
                    try
                    {
                        new Thread(delegate()
                        {
                            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

                            //ds.confirmarDelete(fileName, consegueApagar);

                            idDados++;

                            if (idDados == dados.getPorts().Count)
                                resetEvent.Set();

                        }).Start();

                        break;
                    }
                    catch
                    {
                        System.Console.WriteLine("[READthreads]: Não conseguiu aceder ao DS");
                    }
                }
                resetEvent.WaitOne();
            }
                           
            
        }

        public void runScript(List<string> operations)
        {
            //corre as instrucoes do script
            System.Console.WriteLine("[RUNSCRIPT]: Puppet mandou o Client correr script");

            foreach (string operation in operations)
            {
                string[] token = new string[] { " ", ", " };
                string[] arg = operation.Split(token, StringSplitOptions.None);

                //O cliente recebe mais??

                //arg[1] e sempre o processo, que e ignorado
                if (arg[0].Equals("OPEN")) open(arg[2]);
                else if (arg[0].Equals("CLOSE")) close(arg[2]);
                else if (arg[0].Equals("CREATE")) create(arg[2], Int32.Parse(arg[3]), Int32.Parse(arg[4]), Int32.Parse(arg[5]));
                else if (arg[0].Equals("DELETE")) delete(arg[2]);
                else if (arg[0].Equals("DUMP")) dump();

            }
        }

        public void dump()
        {
            System.Console.WriteLine("*******************************Client DUMP*******************************\n");

            System.Console.WriteLine("------------------------Informacao de Ficheiros------------------------");
            foreach (DictionaryEntry c in ficheiroInfo)
            {
                DadosFicheiro dados = (DadosFicheiro) c.Value;
                System.Console.WriteLine("Ficheiro: " + c.Key + " tem readQuorum=" + dados.getRQ() + " e writeQuorum=" + dados.getWQ() + "e esta guardado nos DS: ");
                try
                {
                    foreach (DictionaryEntry d in dados.getPorts())
                    {
                        System.Console.WriteLine(d.Key + "");
                    }
                }
                catch { }

                System.Console.WriteLine("");
            }
            System.Console.WriteLine("\n-----------------------------File Register-----------------------------");
            foreach (DictionaryEntry c in fileRegister)
            {
                System.Console.WriteLine("FileRegister: " + c.Key + " Nome Ficheiro: " + c.Value);
            }

            System.Console.WriteLine("\n-----------------------------Array Register-----------------------------");
            foreach (DictionaryEntry c in arrayRegister)
            {
                System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                byte[] b = (byte [])c.Value;
                String str = enc.GetString(b);
                System.Console.WriteLine("ArrayRegister: " + c.Key + " Nome Ficheiro: " + str);
            }
            System.Console.WriteLine("\n-----------------------------File Version-----------------------------");
            foreach (DictionaryEntry c in versao)
            {
                System.Console.WriteLine("FileRegister: " + c.Key + " Nome Ficheiro: " + c.Value);
            }

            System.Console.WriteLine("\n*************************************************************************\n\n");
            

        }

        public Hashtable readthreads(string fileName, string semantics)
        {
            DadosFicheiro dados = (DadosFicheiro)ficheiroInfo[fileName];
            Hashtable dataServers = dados.getPorts();

            ManualResetEvent resetEvent = new ManualResetEvent(false);
            int idDados = 0;
            Hashtable dadosDS = new Hashtable();

            foreach (DictionaryEntry c in dataServers)
            {
                IClientToDS ds = (IClientToDS)Activator.GetObject(
                       typeof(IClientToDS),
                       "tcp://localhost:809" + c.Value.ToString() + "/" + c.Key.ToString() + "dataServerClient");
                try
                {
                    new Thread(delegate()
                    {
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

                        dadosDS.Add(idDados, ds.read(fileName, semantics));
                        idDados++;
                        // If we're the last thread, signal
                        if (idDados == dados.getRQ())
                            resetEvent.Set();
                        while(true)
                            System.Console.WriteLine("[READ]: thread: " + idDados); 
                    }).Start();

                    //break;
                }
                catch
                {
                    System.Console.WriteLine("[READthreads]: Não conseguiu aceder ao DS");
                }
            }

            resetEvent.WaitOne();
            return dadosDS;
        }

        public byte[] read(int fileReg, string semantics) 
        {
            byte[] file = new byte[0];

            if (fileRegister.Contains(fileReg))
            {
                string fileName = (string)fileRegister[fileReg];
                DadosFicheiro dados = (DadosFicheiro)ficheiroInfo[fileName];

                Hashtable dataServers = dados.getPorts();
                if (dataServers.Count < dados.getRQ())
                {
                    open(fileName);
                    if (dataServers.Count < dados.getRQ())
                    {
                        System.Console.WriteLine("[READ]: Nao tem DataServers suficientes para o Quorum de Leitura"); 
                    }
                    else
                    {
                        Hashtable dadosDS = readthreads(fileName, semantics);

                        if (semantics.Equals("default"))
                        {
                            int v = 0;

                            foreach (DictionaryEntry e in dadosDS)
                            {
                                DadosFicheiroDS d = (DadosFicheiroDS)e.Value;
                                if (d.getVersion() >= v)
                                {
                                    v = d.getVersion();
                                    file = d.getFile();
                                    if (versao.Contains(fileName))
                                    {
                                        if (v > (int)versao[fileName])
                                        {
                                            versao.Remove(fileName);
                                            versao.Add(fileName, d.getVersion());
                                        }
                                    }
                                    else
                                        versao.Add(fileName, d.getVersion());
                                    break;
                                }
                            }
                        }
                        else //monotonic
                        {
                            //ultima versao que li
                            int v = 0;
                            if (versao.Contains(fileName))

                                v = (int)versao[fileName];
                            while (true)
                            {

                                DadosFicheiroDS d = new DadosFicheiroDS(-1, null);
                                //percorre todos os files que leu
                                foreach (DictionaryEntry e in dadosDS)
                                {
                                    d = (DadosFicheiroDS)e.Value;
                                    //se a versao deste file e maior ou igual a que tinha lido anteriormente
                                    //entao é este o file que vai ler e actualiza a versao
                                    if (d.getVersion() >= v)
                                    {
                                        v = d.getVersion();
                                        file = d.getFile();
                                        versao.Remove(fileName);
                                        versao.Add(fileName, d.getVersion());
                                        break;
                                    }
                                    else
                                    {
                                        //caso contrario continua a ler
                                        dadosDS = readthreads(fileName, semantics);
                                    }
                                }
                                if (d.getVersion() >= v) break;
                            }
                        }
                    }
                }
            }
            else
            {
                System.Console.WriteLine("[READ - DEForMON]: Nao existem dados do fileRegister");
            }
            return file;
        }
        //puppet mandou o cliente enviar pedidos ao DS
        public byte [] read(int fileReg, string semantics, int strinRegister)
        {
            
            byte[] file = read(fileReg, semantics);

            System.Console.WriteLine("[READ]: Mandou Read ao DS");

            if (arrayRegister.ContainsKey(strinRegister))
                arrayRegister.Remove(strinRegister);
            else
                keyArrayRegister++;

            arrayRegister.Add(strinRegister, file);
            
            return file;

        }

        public void writeR(int fileReg, int ByteArrayRegister)
        {
            string nameFile = (string)fileRegister[fileReg];
            byte[] conteudo = (byte[])arrayRegister[ByteArrayRegister];

            DadosFicheiro dados = (DadosFicheiro)ficheiroInfo[nameFile];
            Hashtable dataServers = dados.getPorts();

            if (dataServers.Count < dados.getWQ())
            {
                open(nameFile);
                if (dataServers.Count < dados.getWQ())
                {
                    System.Console.WriteLine("[READ]: Nao tem DataServers suficientes para o Quorum de Leitura");
                }
                else
                {
                    write(nameFile, conteudo);
                }
            }
            
        }

        public void writeS(int fileReg, string conteudo)
        {
            if (fileRegister.Contains(fileReg))
            {
                string nameFile = (string)fileRegister[fileReg];
                //string to byte[]
                byte[] bytes = new byte[conteudo.Length * sizeof(char)];
                System.Buffer.BlockCopy(conteudo.ToCharArray(), 0, bytes, 0, bytes.Length);


                //E suposto aqui tambem guardar??????????????
                if (arrayRegister.ContainsKey(keyArrayRegister))
                    arrayRegister.Remove(keyArrayRegister);
                else
                    keyArrayRegister++;

                arrayRegister.Add(keyArrayRegister, bytes);

                DadosFicheiro dados = (DadosFicheiro)ficheiroInfo[nameFile];
                Hashtable dataServers = dados.getPorts();

                if (dataServers.Count < dados.getWQ())
                {
                    open(nameFile);
                    if (dataServers.Count < dados.getWQ())
                    {
                        System.Console.WriteLine("[READ]: Nao tem DataServers suficientes para o Quorum de Leitura");
                    }
                    else
                    {
                        write(nameFile, bytes);
                    }
                }
            }
            else
            {
                System.Console.WriteLine("[WRITE-S]: Nao existem dados do FileRegister");
            }
        }

        public void write(string fileName, byte[] array)
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            DadosFicheiro dados = (DadosFicheiro)ficheiroInfo[fileName];
            Hashtable dataServers = dados.getPorts();
            int idWrite = 0;

            foreach (DictionaryEntry c in dataServers)
            {
                //System.Console.WriteLine("[WRITE]: DS-key: " + c.Key + " DS-value " + c.Value);
                IClientToDS ds = (IClientToDS)Activator.GetObject(
                       typeof(IClientToDS),
                       "tcp://localhost:809" + c.Value.ToString() + "/" + c.Key.ToString() + "dataServerClient");
                try
                {
                    
                    new Thread(delegate()
                    {
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

                        ds.write(fileName, array);
                        idWrite++;
                        // If we're the last thread, signal
                        if (idWrite == dados.getWQ())
                            resetEvent.Set();
                    }).Start();
                }
                catch
                {
                    System.Console.WriteLine("[WRITE]: Não conseguiu aceder ao DS");
                }
            }

            resetEvent.WaitOne();

            System.Console.WriteLine("[WRITE]: DS escrever file");
        }

        public void copy(int fileRegister1, string semantics, int fileRegister2, string salt)
        {
            try
            {
                byte[] file1 = read(fileRegister1, semantics);

                //string to byte[]
                byte[] file2 = new byte[salt.Length * sizeof(char)];
                System.Buffer.BlockCopy(salt.ToCharArray(), 0, file2, 0, file2.Length);

                byte[] resultado = new byte[file1.Length + file2.Length];
                System.Buffer.BlockCopy(file1, 0, resultado, 0, file1.Length);
                System.Buffer.BlockCopy(file2, 0, resultado, file1.Length, file2.Length);

                string nameFile2 = (string)fileRegister[fileRegister1];
                write(nameFile2, resultado);
                System.Console.WriteLine("[COPY]: Cliente fez copy");
            }
            catch
            {
                System.Console.WriteLine("[COPY]: Cliente FALHOU a fazer copy");
            }
            
        }

        /********DS To Client***********/
        public void respostaDS(string resposta)
        {
            System.Console.WriteLine(resposta);
        }


        /********MS To Client***********/
        public void respostaMS(string resposta)
        {
            System.Console.WriteLine(resposta);
        }
    }

    class PuppetClient : MarshalByRefObject, IPuppetToClient
    {
        public static Cliente ctx;

        public void writeR(int fileReg, int ByteArrayRegister)
        {
            ctx.writeR(fileReg, ByteArrayRegister);
        }
        
        public void writeS(int fileRegister, string conteudo)
        {
            ctx.writeS(fileRegister, conteudo);
        }

        //puppet manda o cliente enviar pedidos ao MS
        public void open(string fileName)
        {
            ctx.open(fileName);

        }

        public void close(string fileName)
        {
            ctx.close(fileName);
        }

        public void create(string fileName, int numDS, int rQuorum, int wQuorum)
        {
            ctx.create(fileName, numDS, rQuorum, wQuorum);
        }

        public void delete(string fileName)
        {
            ctx.delete(fileName);
        }

        public void runScript(List<string> operations)
        {
            ctx.runScript(operations);
        }

        public void copy(int fileRegister1, string semantics, int fileRegister2, string salt)
        {
            ctx.copy(fileRegister1, semantics, fileRegister2, salt);
        }

        public void dump()
        {
            ctx.dump();

        }


        //puppet mandou o cliente enviar pedidos ao DS
        public void read(int fileRegister, string semantics, int stringRegister)
        {
            ctx.read(fileRegister, semantics, stringRegister);


        }
    }

    class DSClient : MarshalByRefObject, IDSToClient
    {
        public static Cliente ctx;

        public void respostaDS(string resposta)
        {
            ctx.respostaDS(resposta);
        }
    }

    class MSClient : MarshalByRefObject, IMSToClient
    {
        public static Cliente ctx;

        public void respostaMS(string resposta)
        {
            ctx.respostaMS(resposta);
        }
    }
}
