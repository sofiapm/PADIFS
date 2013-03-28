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
    class Program : MarshalByRefObject, IPuppetToClient, IDSToClient, IMSToClient
    {
        private static TcpChannel channel;
        public IClientToMS objMS;
        public IClientToDS objDS;
        public Hashtable metaDataServers;
        public Hashtable dataServers;


        Program(Hashtable metaServers, Hashtable dataServer)
        {
            metaDataServers = metaServers;
            this.dataServers = dataServer;
        }

        Program()
        {
        }

        static void Main(string[] args)
        {
            channel = new TcpChannel(Int32.Parse(args[1]));
            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(Program),
            args[0],
            WellKnownObjectMode.Singleton);
            
            Hashtable metaDataServers = new Hashtable();
            Hashtable dataServers = new Hashtable();

            System.Console.WriteLine(args[0] + ": <enter> para sair...");

            System.Console.ReadLine();
        }

        
        
        /********Puppet To Client***********/
        //puppet envia informações ao cliente
        public void guardaMS(Hashtable metadataservers)
        {
            metaDataServers = metadataservers;
            System.Console.WriteLine("Recebeu MS para guardar");

            /*this.objMS = (IClientToMS)metaDataServers[1];

            objMS.open("guardei ms");*/
        }


        //puppet manda o cliente enviar pedidos ao MS
        public void open(string fileName)
        {
            /*this.objMS = (IClientToMS)metaDataServers[1];

            objMS.open(fileName);*/

            System.Console.WriteLine("Mandou Ms abrir file");

        }

        public void close(string fileName)
        {
            /*this.objMS = (IClientToMS)metaDataServers[1];

            objMS.close(fileName);*/

            System.Console.WriteLine("Mandou Ms fechar file");
        }

        public void create(string fileName, int numDS, int rQuorum, int wQuorum)
        {
            this.objMS = (IClientToMS)metaDataServers[1];

            objMS.create(fileName, numDS, rQuorum, wQuorum);

            System.Console.WriteLine("Mandou Ms criar file");
        }

        public void delete(string fileName)
        {
            this.objMS = (IClientToMS)metaDataServers[1];

            objMS.delete(fileName);

            System.Console.WriteLine("Mandou Ms apagar file");
        }

        //puppet mandou o cliente enviar pedidos ao DS
        public void read(string fileName, string semantics)
        {
            this.objDS = (IClientToDS)dataServers[1];

            this.objDS.read(fileName, semantics);

            System.Console.WriteLine("Mandou Ms ler file");

            IClientToPuppet puppet = (IClientToPuppet)Activator.GetObject(
            typeof(IClientToPuppet),
            "tcp://localhost:8060/PuppetMaster");

            puppet.respostaClient("Ja pedi open");

            System.Console.WriteLine("Mandou mensagem ao puppet");


        }

        public void write(string fileName, byte[] array)
        {
            this.objDS = (IClientToDS)dataServers[1];

            objDS.write(fileName, array);

            System.Console.WriteLine("Mandou Ms ler file");
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
}
