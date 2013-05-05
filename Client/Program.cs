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
    class Program
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

            Cliente cliente = new Cliente(channel, metaDataServers, args[0]);
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
        //string nomeFicheiro, DadosFicheiro
        public Hashtable ficheiroInfo = new Hashtable();
        //int id-keyFileRegister, string nome
        public Hashtable fileRegister = new Hashtable();
        //int id-keyArrayRegister, string conteudo
        public Hashtable arrayRegister = new Hashtable();
        //string nomeFicheiro, int versao
        public Hashtable versao = new Hashtable();
        String idCliente;

        int keyArrayRegister = 0;
        int keyFileRegister = 0;

        public Cliente (TcpChannel canal, Hashtable metaServers, String id)
        {
            channel=canal;
            this.metaDataServers = metaServers;
            idCliente = id;
        }
        
        /********Puppet To Client***********/
        //puppet manda o cliente fazer open de um ficheiro ao MS
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
                    System.Console.WriteLine("[OPEN]: Cliente contactou com sucesso o MS: " + c.Value.ToString() + " E " + c.Key.ToString());
                    System.Console.WriteLine("[OPEN]: Cliente abriu o ficheiro: " + fileName);
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
                //se o ficheiro já estava aberto, actualiza os seus metadados
                if (ficheiroInfo.Contains(fileName))
                {
                    ficheiroInfo.Remove(fileName);
                }

                ficheiroInfo.Add(fileName, fileData);

                int key=-1;
                bool existia=false;
                //verifica se ja existe um fileRegister para aquele ficheiro
                foreach (DictionaryEntry en in fileRegister)
                {
                    if (en.Value.ToString().Equals(fileName))
                    {
                        key = (int)en.Key;
                        existia = true;
                        break;
                    }
                }

                if (existia)//se ja existia, actualiza
                {
                    fileRegister.Remove(key);
                    fileRegister.Add(key, fileName);
                }
                else//se nao existia, cria novo fileRegister
                {
                    if (keyFileRegister >= 10)
                    {
                        keyFileRegister = 0;
                        fileRegister.Remove(keyFileRegister);
                    }
                    fileRegister.Add(keyFileRegister, fileName);
                    keyFileRegister++;
                }               

            }
   
        }

        //Puppet mando Cliente fazer close do ficheiro
        public void close(string fileName)
        {
            //verifica se o ficheiro está aberto
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

                //int key = -1;
                //bool existia = false;
                ////verifica se ja existe um fileRegister para aquele ficheiro
                //foreach (DictionaryEntry en in fileRegister)
                //{
                //    if (en.Value.ToString().Equals(fileName))
                //    {
                //        key = (int)en.Key;
                //        existia = true;
                //        break;
                //    }
                //}

                //if (existia)//se ja existia, actualiza
                //{
                //    fileRegister.Remove(key);
                //    keyFileRegister--;
                    
                //}

                System.Console.WriteLine("[CLOSE]: Mandou Ms fechar file: " + fileName);
            }
            else
            {
                System.Console.WriteLine("[CLOSE]: O ficheiro nao esta aberto: " + fileName);
            }
        }

        //puppet manda o cliente criar um novo ficheiro no MS
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
                    System.Console.WriteLine("[CREATE] Cliente criou o ficheiro: " + fileName);
                    break;
                }
                catch //( Exception e)
                {
                    //System.Console.WriteLine(e.ToString());
                    System.Console.WriteLine("[CREATE] Não conseguiu aceder ao MS: " + c.Key.ToString() + " - " + c.Value.ToString());
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
                
                //verifica se ja tinha metadados do ficheiro
                if (ficheiroInfo.Contains(fileName))
                {
                    ficheiroInfo.Remove(fileName);
                }

                //actualiza os metadados do ficheiro
                ficheiroInfo.Add(fileName, fileData);

                //if (fileRegister.Contains(keyFileRegister))
                //{
                //    fileRegister.Remove(keyFileRegister);
                //}

                int key = -1;
                bool existia = false;
                //verifica se ja existia um fileRegister para este ficheiro
                foreach (DictionaryEntry en in fileRegister)
                {
                    if (en.Value.ToString().Equals(fileName))
                    {
                        key = (int)en.Key;
                        existia = true;
                    }
                }

                if (existia)//se existia, actuliza
                {
                    fileRegister.Remove(key);
                    fileRegister.Add(key, fileName);
                }
                else//se nao existia, cria novo
                {
                    if (keyFileRegister >= 10)
                    {
                        keyFileRegister = 0;
                        fileRegister.Remove(keyFileRegister);
                    }
                    fileRegister.Add(keyFileRegister, fileName);
                    keyFileRegister++;
                }
            }

            
            //System.Console.WriteLine("Mandou Ms criar file");
        }

        //puppet manda Cliente apagar o ficheiro
        public void delete(string fileName)
        {
            DadosFicheiro dados=null;
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            List<IClientToDS> listDS = new List<IClientToDS>();
            List<IClientToMS> listMS = new List<IClientToMS>();
            
            //Cliente pede ao MS para apagar o ficheiro
            //recebe metadados do ficheiro, para comonicar com DS
            foreach (DictionaryEntry c in metaDataServers)
            {
                 IClientToMS ms = (IClientToMS)Activator.GetObject(
                          typeof(IClientToMS),
                           "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerClient");
                 listMS.Add(ms);

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

            bool consegueApagar = false;

            if (!temDS)
            {
                System.Console.WriteLine("[DELETE]: Nao tem DS para mandar o DELETE!");
            }
            else
            {
                consegueApagar = true;
                int idDados = 0;

                //Cliente vai a todos os DS onde o ficheiro esta replicado e pede para apagar
                //se todos os Ds conseguirem apagar, a flag consegueApagar mantem-se a true
                foreach (DictionaryEntry c in dados.getPorts())
                {
                    IClientToDS ds = (IClientToDS)Activator.GetObject(
                       typeof(IClientToDS),
                       "tcp://localhost:809" + c.Value.ToString() + "/" + c.Key.ToString() + "dataServerClient");
                    listDS.Add(ds);
                    try
                    {
                        new Thread(delegate()
                        {
                            bool b = ds.delete(fileName);
                            if (b.Equals(false))
                                consegueApagar = false;
                            idDados++;
                           
                            if (idDados >= dados.getPorts().Count)
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
                idDados = 0;

                //como todos os DS podem apagar a sua replica do ficheiro, o Cliente envia
                //a confirmação aos DS, para que apaguem efectivamente o ficheiro
                foreach (IClientToDS c in listDS)
                {
                    //IClientToDS ds = (IClientToDS)Activator.GetObject(
                    //   typeof(IClientToDS),
                    //   "tcp://localhost:809" + c.Value.ToString() + "/" + c.Key.ToString() + "dataServerClient");
                    try
                    {
                        new Thread(delegate()
                        {
                            c.confirmarDelete(fileName, consegueApagar);

                            idDados++;

                            if (idDados >= dados.getPorts().Count)
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
            }

            //como o delete foi efectuado por todos os DS, o cliente envia confirmaçao ao MS
            //para que este possa também apagar o ficehiro dos seus metadados
            foreach (DictionaryEntry c in metaDataServers)
            {
                IClientToMS ms = (IClientToMS)Activator.GetObject(
                         typeof(IClientToMS),
                          "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerClient");
                listMS.Add(ms);

                try
                {
                    ms.confirmarDelete(fileName, consegueApagar);
                    System.Console.WriteLine("[DELETE]:  Confirmacao para Ms apagar file: " + fileName);
                    break;
                }
                catch
                {
                    System.Console.WriteLine("[DELETE]: Não conseguiu aceder ao MS: " + c.Key.ToString() + " E " + c.Value.ToString());
                }
            }
            
            //Cliente apaga os metadados do ficheiro
            if (consegueApagar)
            {
                ficheiroInfo.Remove(fileName);
                //key = -1;
                //existia = false;
                ////verifica se ja existe um fileRegister para aquele ficheiro
                //foreach (DictionaryEntry en in fileRegister)
                //{
                //    if (en.Value.ToString().Equals(fileName))
                //    {
                //        key = (int)en.Key;
                //        existia = true;
                //        break;
                //    }
                //}

                //if (existia)//se ja existia, actualiza
                //{
                //    fileRegister.Remove(key);
                //    keyFileRegister--;
                //}
            }
                           
            
        }

        //corre um novo script assincrono
        public void runScript(List<string> operations)
        {
            //corre as instrucoes do script
            System.Console.WriteLine("[RUNSCRIPT]: Puppet mandou o Client correr script");

            foreach (string operation in operations)
            {
                string[] token = new string[] { " ", ", " };
                string[] arg = operation.Split(token, StringSplitOptions.None);

                //arg[1] e sempre o processo, que e ignorado
                if (arg[0].Equals("OPEN")) open(arg[2]);
                else if (arg[0].Equals("CLOSE")) close(arg[2]);
                else if (arg[0].Equals("READ")) read(Int32.Parse(arg[2]), arg[3], Int32.Parse(arg[4]));
                else if (arg[0].Equals("WRITE"))
                {
                    if (arg[3].Length > 1)
                    {
                        //ex: WRITE c-1, 0, "Text contents of the file. Contents are a string delimited by double quotes as this one"
                        writeS(Int32.Parse(arg[2]), arg[3]);
                    }
                    else
                    {
                        //ex: WRITE c-1, 0, 0
                        writeR(Int32.Parse(arg[2]), Int32.Parse(arg[3]));
                    }
                }
                else if (arg[0].Equals("COPY")) copy(Int32.Parse(arg[2]), arg[3], Int32.Parse(arg[4]), arg[4]);
                else if (arg[0].Equals("CREATE")) create(arg[2], Int32.Parse(arg[3]), Int32.Parse(arg[4]), Int32.Parse(arg[5]));
                else if (arg[0].Equals("DELETE")) delete(arg[2]);
                else if (arg[0].Equals("DUMP")) dump();

            }
        }

        public string dump()
        {
            String st = "*******************************Client" + idCliente + "DUMP*******************************\n";


            st +="------------------------Informacao de Ficheiros------------------------\n";
            foreach (DictionaryEntry c in ficheiroInfo)
            {
                DadosFicheiro dados = (DadosFicheiro) c.Value;
                st += "Ficheiro: " + c.Key + " tem readQuorum=" + dados.getRQ() + " e writeQuorum=" + dados.getWQ() + "e esta guardado nos DS: \n";
                try
                {
                    foreach (DictionaryEntry d in dados.getPorts())
                    {
                        st += d.Key + "\n";
                    }
                }
                catch { }

                st += "\n";
            }
            st += "\n-----------------------------File Register-----------------------------\n";
            foreach (DictionaryEntry c in fileRegister)
            {
                st += "FileRegister: " + c.Key + " Nome Ficheiro: " + c.Value + "\n";
            }

            st += "\n-----------------------------Array Register-----------------------------\n";
            foreach (DictionaryEntry c in arrayRegister)
            {
               
                byte[] b = (byte [])c.Value;
              
                string str = Encoding.UTF8.GetString(b, 0, b.Length);
                st += "ArrayRegister: " + c.Key + " Ficheiro: " + str +"\n";
            }
            st +="\n-----------------------------File Version-----------------------------\n";
            foreach (DictionaryEntry c in versao)
            {
                st += "Nome Ficheiro: " + c.Key + " Versao: " + c.Value + "\n";
            }

            st += "****************************END -- Client" + idCliente + "DUMP***************************\n\n";

           System.Console.WriteLine(st);

           return st;
            

        }

        //funcao que lança uma thread para cada leitura num DS diferente
        //devolve uma hashtable com todos os retornos de cada DS
        //porque na funcao read tanto o default como o monotonic espera pelo quorum
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
                        
                        try
                        {
                            DadosFicheiroDS d = ds.read(fileName, semantics);
                            dadosDS.Add(idDados++, d);
                        }
                        catch
                        {
                            System.Console.WriteLine("[READthreads]: Nao conseguiu aceder ao DS!");
                        }
                        

                        //idDados++;
                        // If we're the last thread, signal
                        if (idDados >= dados.getRQ())
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

            int numNaoNull = 0;
            foreach (DictionaryEntry c in dadosDS) {
                if (c.Value != null)
                {
                    numNaoNull++;   
                }
            }

            if (numNaoNull < dados.getRQ())
                dadosDS = null;

            return dadosDS;
        }

        //funcao Read onde e verificada a semantica - versao
        public byte[] read(int fileReg, string semantics) 
        {
            byte[] file = new byte[0];

            if (fileRegister.Contains(fileReg))
            {
                string fileName = (string)fileRegister[fileReg];
                DadosFicheiro dados = (DadosFicheiro)ficheiroInfo[fileName];

                Hashtable dataServers = dados.getPorts();
                
                //verifica se tem DS para o quorum ou replicas
                //se nao tiver, actualiza metadados
                if (dataServers.Count < dados.getRQ() || dataServers.Count < dados.getNumDS())
                {
                    open(fileName);
                    dados = (DadosFicheiro)ficheiroInfo[fileName];
                    dataServers = dados.getPorts();
                }

                //se ainda nao tiver DS para o quorum, nao consegue LER
                if (dataServers.Count < dados.getRQ())
                {
                    System.Console.WriteLine("[READ]: Nao tem DataServers suficientes para o Quorum de Leitura"); 
                }
                else
                {
                        //faz read, recebe a hashtable com todas as respostas dos DS
                        Hashtable dadosDS = readthreads(fileName, semantics);
                        
                        if (dadosDS != null)//consegue fazer read
                        {
                            if (semantics.Equals("default"))
                            {
                                int v = 0;

                                //percorre todas as respostas dos DS
                                
                                foreach (DictionaryEntry e in dadosDS)
                                {
                                    if (!e.Equals(null))
                                    {
                                        DadosFicheiroDS d = (DadosFicheiroDS)e.Value;
                                        //versao lida por este DS é superior à anterior
                                        if (d.getVersion() >= v)
                                        {
                                            v = d.getVersion();
                                            file = d.getFile();
                                        }
                                    }
                                }

                                //le-guarda a versao mais recente dessas respostas
                                if (versao.Contains(fileName))
                                {
                                    versao.Remove(fileName);
                                }
                                
                                versao.Add(fileName, v);
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
                                       
                                    }
                                    
                                    if (d.getVersion() >= v) break;
                                    else
                                    {
                                        //se nao havia nenhuma versao superior a anterior lida
                                        //volta a ler
                                        dadosDS = readthreads(fileName, semantics);
                                    }
                                }
                            }
                        }
                        else
                        {
                            System.Console.WriteLine("[READ]: Nao existe o file no DS.");
                        }
                    }
            }
            else
            {
                System.Console.WriteLine("[READ - DEForMON]: Nao existem dados do fileRegister");
            }
            return file;
        }

        //puppet mandou o cliente fazer READ
        //actualiza os registers
        public byte [] read(int fileReg, string semantics, int strinRegister)
        {
            
            byte[] file = read(fileReg, semantics);

            System.Console.WriteLine("[READ]: Mandou Read ao DS");

            if (arrayRegister.ContainsKey(strinRegister))
                arrayRegister.Remove(strinRegister);
                
            arrayRegister.Add(strinRegister, file);

            return file;

        }

        //Funcao de Write que recebe o byteArrayRegister
        public void writeR(int fileReg, int ByteArrayRegister)
        {
            string nameFile = (string)fileRegister[fileReg];
            byte[] conteudo = (byte[])arrayRegister[ByteArrayRegister];

            DadosFicheiro dados = (DadosFicheiro)ficheiroInfo[nameFile];
            Hashtable dataServers = dados.getPorts();

            //se os metadados do ficheiro nao tiverem DS suficientes para o 
            //quorum ou com o numero suficiente de replicas, volta a pedir os metadados
            //if (dataServers.Count < dados.getWQ() || dataServers.Count < dados.getNumDS())
            //{
            //    open(nameFile);
            //    dados = (DadosFicheiro)ficheiroInfo[nameFile];
            //    dataServers = dados.getPorts();
            //}
             
            ////se ainda nao existirem DS suficientes para o quorum, nao consegue fazer a escrita
            //if (dataServers.Count < dados.getWQ())
            //{
            //   System.Console.WriteLine("[READ]: Nao tem DataServers suficientes para o Quorum de Leitura");
            //}
            //else
            //{
               write(nameFile, conteudo);
            //}
            
        }

        //Funcao de Write que recebe a string
        public void writeS(int fileReg, string conteudo)
        {
            if (fileRegister.Contains(fileReg))
            {
                string nameFile = (string)fileRegister[fileReg];
                //string to byte[]
                byte[] bytes = Encoding.ASCII.GetBytes(conteudo);
                
                //actualiza arrayRegister
                if (arrayRegister.ContainsKey(keyArrayRegister))
                {
                    arrayRegister.Remove(keyArrayRegister);
                    arrayRegister.Add(keyArrayRegister, bytes);
                }
                else
                {
                    if (keyArrayRegister >= 10)
                    {
                        keyArrayRegister = 0;
                        arrayRegister.Remove(keyArrayRegister);
                    }
                    arrayRegister.Add(keyArrayRegister, bytes);
                    keyArrayRegister++;
                }

                

                DadosFicheiro dados = (DadosFicheiro)ficheiroInfo[nameFile];
                Hashtable dataServers = dados.getPorts();

                //verifica se tem o numero de Ds para o quorum ou para o num de replicas
                //se nao tiver, actualiza metadados
                //if (dataServers.Count < dados.getWQ() || dataServers.Count < dados.getNumDS())
                //{
                //    open(nameFile);
                //    dados = (DadosFicheiro)ficheiroInfo[nameFile];
                //    dataServers = dados.getPorts();
                //}
                 
                ////se ainda nao tiver o quorum, nao consegue fazer a escrita
                //if (dataServers.Count < dados.getWQ())
                //{
                //    System.Console.WriteLine("[WRITE]: Nao tem DataServers suficientes para o Quorum de Escrita");
                //}
                //else
                //{
                    write(nameFile, bytes);
                //}
                
            }
            else
            {
                System.Console.WriteLine("[WRITE-S]: Nao existem dados do FileRegister");
            }
        }

        //funcao de escrita que comunica com os DS
        public void write(string fileName, byte[] array)
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            int idWrite = 0;

            DadosFicheiro dados = (DadosFicheiro)ficheiroInfo[fileName];
            Hashtable dataServers = dados.getPorts();
           
            if (dataServers.Count < dados.getWQ() || dataServers.Count < dados.getNumDS())
            {
                open(fileName);
                dados = (DadosFicheiro)ficheiroInfo[fileName];
                dataServers = dados.getPorts();
            }

            if (dataServers.Count < dados.getWQ())
            {
                System.Console.WriteLine("[WRITE]: Nao tem DataServers suficientes para o Quorum de Escrita");
            }
            else
            {


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
                            try
                            {
                                ds.write(fileName, array);
                                idWrite++;
                                // If we're the last thread, signal
                                if (idWrite >= dados.getWQ())
                                    resetEvent.Set();
                            }
                            catch (Exception e)
                            {
                                System.Console.WriteLine("[WRITE]: Não conseguiu aceder ao DS");
                            }

                        }).Start();
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine(e.ToString());
                        System.Console.WriteLine("[WRITE]: Não conseguiu aceder ao DS");
                    }
                }
                resetEvent.WaitOne();

                System.Console.WriteLine("[WRITE]: DS escrever file");
            }
        }

        //puppet manda o Cliente fazer copy  um ficheiro
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

                string nameFile2 = (string)fileRegister[fileRegister2];

                string nameFile = (string)fileRegister[fileRegister1];
                DadosFicheiro d = (DadosFicheiro)ficheiroInfo[nameFile];

                System.Console.WriteLine("[COPY]: Nome Novo File: " + nameFile2);

                //create (nameFile + nameFile2, 1, 1, 1);
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

        public string dump()
        {
            return ctx.dump();

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
