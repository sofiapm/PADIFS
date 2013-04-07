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

namespace Client
{
    class Program //: MarshalByRefObject, IDSToClient, IMSToClient
    {

        static void Main(string[] args)
        {
            TcpChannel channel;

            channel = new TcpChannel(Int32.Parse(args[1]));
            ChannelServices.RegisterChannel(channel, false);

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
            Hashtable dataServers = new Hashtable();

            Cliente cliente = new Cliente(channel, metaDataServers, dataServers);
            PuppetClient.ctx = cliente;
            DSClient.ctx = cliente;
            MSClient.ctx = cliente;

            System.Console.WriteLine(args[0] + ": <enter> para sair...");

            System.Console.ReadLine();
        }

    }


    class Cliente
    {
        private static TcpChannel channel;
        public Hashtable metaDataServers;
        public Hashtable dataServers;

        public Cliente (TcpChannel canal, Hashtable metaServers, Hashtable dataServer)
        {
            channel=canal;
            this.metaDataServers = metaServers;
            this.dataServers = dataServer;
        }
        
        /********Puppet To Client***********/
        //puppet envia informações ao cliente
        public void guardaMS(Hashtable metadataservers)
        {
            metaDataServers = metadataservers;
            System.Console.WriteLine("Recebeu MS para guardar");

        }


        //puppet manda o cliente enviar pedidos ao MS
        public void open(string fileName)
        {
            Hashtable n = null;
            foreach (DictionaryEntry c in metaDataServers)
            {
                IClientToMS ms = (IClientToMS)Activator.GetObject(
                       typeof(IClientToMS),
                       "tcp://localhost:808" + c.Value.ToString() + "/" + c.Key.ToString() + "MetaServerClient");
                try
                {
                    n = ms.open(fileName);
                    break;
                }
                catch
                {
                    System.Console.WriteLine("[OPEN]: Não conseguiu aceder ao MS");
                }
            }

            foreach (DictionaryEntry c in n)
            {
                System.Console.WriteLine("[TESTE]");
                System.Console.WriteLine("c.key: " + c.Key);
                System.Console.WriteLine("c.value: " + c.Value);
            }
            

            System.Console.WriteLine("Mandou Ms abrir file");

        }

        public void close(string fileName)
        {
            foreach (DictionaryEntry c in metaDataServers)
            {
                IClientToMS ms = (IClientToMS)Activator.GetObject(
                       typeof(IClientToMS),
                       "tcp://localhost:808" + c.Value.ToString() + "/" + c.Key.ToString() + "MetaServerClient");
                try
                {
                    ms.close(fileName);
                    break;
                }
                catch
                {
                    System.Console.WriteLine("[CREATE]: Não conseguiu aceder ao MS");
                }
            }

            System.Console.WriteLine("Mandou Ms fechar file");
        }

        public void create(string fileName, int numDS, int rQuorum, int wQuorum)
        {
            foreach (DictionaryEntry c in metaDataServers)
            {
                IClientToMS ms = (IClientToMS)Activator.GetObject(
                       typeof(IClientToMS),
                       "tcp://localhost:808" + c.Value.ToString() + "/" + c.Key.ToString() + "MetaServerClient");
                try
                {
                    ms.create(fileName, numDS, rQuorum, wQuorum);
                    break;
                }
                catch ( Exception e)
                {
                    System.Console.WriteLine("[CREATE]: Não conseguiu aceder ao MS - " + e.Message);
                }
            }

            System.Console.WriteLine("Mandou Ms criar file");
        }

        public void delete(string fileName)
        {
            foreach (DictionaryEntry c in metaDataServers)
            {
                IClientToMS ms = (IClientToMS)Activator.GetObject(
                       typeof(IClientToMS),
                       "tcp://localhost:808" + c.Value.ToString() + "/" + c.Key.ToString() + "MetaServerClient");
                try
                {
                    ms.delete(fileName);
                    break;
                }
                catch
                {
                    System.Console.WriteLine("[DELETE]: Não conseguiu aceder ao MS");
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

        public void fail()
        {
            System.Console.WriteLine("Puppet mandou o Client falhar");
        }

        public void recover()
        {
            System.Console.WriteLine("Puppet mandou o Client recover");
        }

        public void freeze()
        {
            System.Console.WriteLine("Puppet mandou o Client freeze");
        }

        public void unfreeze()
        {
            System.Console.WriteLine("Puppet mandou o Client unfreeze");
        }

        public void dump()
        {
            System.Console.WriteLine("Puppet mandou o Client fazer Dump");

        }


        //puppet mandou o cliente enviar pedidos ao DS
        public void read(string fileName, string semantics)
        {

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

        public void write(string fileName, byte[] array)
        {
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

            System.Console.WriteLine("Mandou Ms escrever file");
        }

        /********DS To Client***********/
        public void respostaDS(string resposta)
        {
            System.Console.WriteLine(resposta);
        }


        /********MS To Client***********/
        public void guardaDS(Hashtable dataservers)
        {
            dataServers = dataservers;
        }

        public void respostaMS(string resposta)
        {
            System.Console.WriteLine(resposta);
        }
    }

    class PuppetClient : MarshalByRefObject, IPuppetToClient
    {
        public static Cliente ctx;

        //puppet envia informações ao cliente
        public void guardaMS(Hashtable metadataservers)
        {
           ctx.guardaMS(metadataservers);
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

        public void fail()
        {
            ctx.fail();
        }

        public void recover()
        {
            ctx.recover();
        }

        public void freeze()
        {
            ctx.freeze();
        }

        public void unfreeze()
        {
            ctx.unfreeze();
        }

        public void dump()
        {
            ctx.dump();

        }


        //puppet mandou o cliente enviar pedidos ao DS
        public void read(string fileName, string semantics)
        {
            ctx.read(fileName, semantics);


        }

        public void write(string fileName, byte[] array)
        {
            ctx.write(fileName, array);
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

        public void guardaDS(Hashtable dataservers)
        {
            ctx.guardaDS(dataservers);
        }

        public void respostaMS(string resposta)
        {
            ctx.respostaMS(resposta);
        }
    }
}
