﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Collections;

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
                System.Console.WriteLine("Vou tentar falar com: " + c.Value.ToString());

                try
                {
                    
                    fileData = ms.open(fileName);
                    break;
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.ToString());
                    System.Console.WriteLine("[OPEN]: Não conseguiu aceder ao MS: " + c.Value.ToString() + " E " + c.Key.ToString());
                }
            }

            bool temDS = true;

            try{fileData.getPorts();}
            catch (NullReferenceException e){temDS = false;}

            if (!temDS)
            {
                System.Console.WriteLine("Não Consegue abrir o ficheiro!");
            }
            else
            {
                if (ficheiroInfo.Contains(fileName))
                {
                    ficheiroInfo.Remove(fileName);
                }

                ficheiroInfo.Add(fileName, fileData);
            }
   
        }

        public void close(string fileName)
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
                    System.Console.WriteLine("[CREATE]: Não conseguiu aceder ao MS: " + c.Key.ToString() + " E " + c.Value.ToString());
                }
            }



            System.Console.WriteLine("Mandou Ms fechar file");
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
                    break;
                }
                catch //( Exception e)
                {
                    System.Console.WriteLine("[CREATE]: Não conseguiu aceder ao MS: " + c.Key.ToString() + " E " + c.Value.ToString());
                }
            }

            bool temDS = true;

            try{ fileData.getPorts(); }
            catch (NullReferenceException e){ temDS = false; }

            if (!temDS)
            {
                System.Console.WriteLine("Nao conseguiu criar o ficheiro!");
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
            foreach (DictionaryEntry c in metaDataServers)
            {
                IClientToMS ms = (IClientToMS)Activator.GetObject(
                       typeof(IClientToMS),
                       "tcp://localhost:808" + c.Key.ToString() + "/" + c.Value.ToString() + "MetaServerClient");
                try
                {
                    ms.delete(fileName);
                    break;
                }
                catch
                {
                    System.Console.WriteLine("[DELETE]: Não conseguiu aceder ao MS: " + c.Key.ToString() + " E " + c.Value.ToString());
                }
            }

            System.Console.WriteLine("Mandou Ms apagar file");
        }

        public void runScript(List<string> operations)
        {
            //corre as instrucoes do script
            System.Console.WriteLine("Puppet mandou o Client correr script");

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
            
            foreach (DictionaryEntry c in ficheiroInfo)
            {
                DadosFicheiro dados = (DadosFicheiro) c.Value;
                System.Console.WriteLine("Ficheiro: " + c.Key + " tem readQuorum=" + dados.getRQ() + " e writeQuorum=" + dados.getWQ() + "e esta guardado nos DS: ");
                foreach (DictionaryEntry d in dados.getPorts())
                {
                    System.Console.WriteLine(d.Key + "\n\n");
                }
            }

            System.Console.WriteLine("*************************************************************************\n\n");
            

        }


        //puppet mandou o cliente enviar pedidos ao DS
        public void read(string fileName, string semantics, int strinRegister)
        {
            DadosFicheiro dados = (DadosFicheiro)ficheiroInfo[fileName];
            Hashtable dataServers = dados.getPorts();

            //guarda a resposta do DS no stringRegister
            foreach (DictionaryEntry c in dataServers)
            {
                IClientToDS ds = (IClientToDS)Activator.GetObject(
                       typeof(IClientToDS),
                       "tcp://localhost:809" + c.Value.ToString() + "/" + c.Key.ToString() + "dataServerClient");
                try
                {
                    ds.read(fileName, semantics);
                    break;
                }
                catch
                {
                    System.Console.WriteLine("[READ]: Não conseguiu aceder ao DS");
                }
            }

            System.Console.WriteLine("Mandou Read ao DS");


        }

        public void writeR(int fileReg, int ByteArrayRegister)
        {
            string nameFile = (string)fileRegister[fileReg];
            string conteudo = (string)arrayRegister[ByteArrayRegister];

            //string to byte[]
            byte[] bytes = new byte[conteudo.Length * sizeof(char)];
            System.Buffer.BlockCopy(conteudo.ToCharArray(), 0, bytes, 0, bytes.Length);

            write(nameFile, bytes);
        }

        public void writeS(int fileReg, string conteudo)
        {
            string nameFile = (string)fileRegister[fileReg];
            
            //string to byte[]
            byte[] bytes = new byte[conteudo.Length * sizeof(char)];
            System.Buffer.BlockCopy(conteudo.ToCharArray(), 0, bytes, 0, bytes.Length);

            write(nameFile, bytes);
        }

        public void write(string fileName, byte[] array)
        {   
            DadosFicheiro dados = (DadosFicheiro)ficheiroInfo[fileName];
            Hashtable dataServers = dados.getPorts();

            foreach (DictionaryEntry c in dataServers)
            {
                IClientToDS ds = (IClientToDS)Activator.GetObject(
                       typeof(IClientToDS),
                       "tcp://localhost:809" + c.Value.ToString() + "/" + c.Key.ToString() + "dataServerClient");
                try
                {
                    ds.write(fileName, array);
                    break;
                }
                catch
                {
                    System.Console.WriteLine("[WRITE]: Não conseguiu aceder ao DS");
                }
            }

            System.Console.WriteLine("Mandou DS escrever file");
        }

        public void copy(int fileRegister1, string semantics, int fileRegister2, string salt)
        {
            System.Console.WriteLine("Cliente fez copy");
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
        public void read(string fileName, string semantics, int stringRegister)
        {
            ctx.read(fileName, semantics, stringRegister);


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
