using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace DataServer
{
    class Program : MarshalByRefObject, IClientToDS, IPuppetToDS, IMSToDS
    {
        private static TcpChannel channel;

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

            System.Console.WriteLine(args[0] + ": <enter> para sair...");

            System.Console.ReadLine();
        }

        /********Puppet To DataServer***********/

        //starts buffering read and write requests, without answering
        public void freeze()
        {
            System.Console.WriteLine("Puppet mandou o DS freeze");

            //Nao se pode fazer ciclo
            /*IDSToPuppet puppet = (IDSToPuppet)Activator.GetObject(
            typeof(IDSToPuppet),
            "tcp://localhost:8060/PuppetMaster");
            
            puppet.respostaDS("Puppet mandou fazer freeze");*/
        }

        //responds to all buffered requests from clients and restarts replying new requests
        public void unfreeze()
        {
            System.Console.WriteLine("Puppet mandou o DS unfreeze");
        }

        //DS ignores requests from Clients or messages from MS
        public void fail()
        {
            System.Console.WriteLine("Puppet mandou o DS falhar");
        }

        //DS starts receiving requests from Clients and MS
        public void recover()
        {
            System.Console.WriteLine("Puppet mandou o DS recuperar");
        }

        /********Client To DataServer***********/

        //returns the version and content os local file
        public void read(string fileName, string semantics)
        {
            System.Console.WriteLine("Cliente está a ler ficheiro do DS");

            //Nao se pode fazer ciclo
            /*IDSToClient client = (IDSToClient)Activator.GetObject(
            typeof(IDSToClient),
            "tcp://localhost:8070/Client");
            
            client.respostaDS("DS envia a Cliente: Já li ficheiro: " + fileName);*/

        }

        //overwrites the content of file, creates new version
        public void write(string fileName, byte[] array)
        {
            System.Console.WriteLine("Cliente está a escrever ficheiro do DS");
        }

        /********MS To DataServer***********/
        public void areYouAlive()
        {
            System.Console.WriteLine("MS pergunta se DS esta vivo.");

            //Nao se pode fazer ciclo
            /*IDSToMS ms = (IDSToMS)Activator.GetObject(
            typeof(IDSToMS),
            "tcp://localhost:8081/MetaDataServer1");

            ms.respostaDS("DS diz: I'm Alive!");*/
        }

        public void respostaMS(string resp)
        {
            System.Console.WriteLine("MS diz: " + resp);
        }
    }
}
