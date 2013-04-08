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
    class Program 
    {

        Program()
        {
        }

        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(Int32.Parse(args[1]));

            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(DataServerPuppet),
            args[0] + "DataServerPuppet",
            WellKnownObjectMode.Singleton);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(DataServerClient),
            args[0] + "DataServerClient",
            WellKnownObjectMode.Singleton);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(DataServerMS),
            args[0] + "DataServerMS",
            WellKnownObjectMode.Singleton);

            DataServer ds = new DataServer(channel);
            DataServerClient.ctx = ds;
            DataServerMS.ctx = ds;
            DataServerPuppet.ctx = ds;

            System.Console.WriteLine(args[0] + ": <enter> para sair...");

            System.Console.ReadLine();
        }

    }

    class DataServer
    {
        TcpChannel channel;

        public DataServer(TcpChannel channel)
        {
            this.channel = channel;
        }

        /********Puppet To DataServer***********/

        //starts buffering read and write requests, without answering
        public void freeze()
        {
            System.Console.WriteLine("Puppet mandou o DS freeze");
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

        public void dump()
        {
            System.Console.WriteLine("Puppet mandou o DS fazer Dump");
        }

        /********Client To DataServer***********/

        //returns the version and content os local file
        public void read(string fileName, string semantics)
        {
            System.Console.WriteLine("Cliente está a ler ficheiro do DS");

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
        }

        public void respostaMS(string resp)
        {
            System.Console.WriteLine("MS diz: " + resp);
        }
    }

    class DataServerPuppet : MarshalByRefObject, IPuppetToDS
    {
        public static DataServer ctx;

        //starts buffering read and write requests, without answering
        public void freeze()
        {
            ctx.freeze();
        }

        //responds to all buffered requests from clients and restarts replying new requests
        public void unfreeze()
        {
            ctx.unfreeze();
        }

        //DS ignores requests from Clients or messages from MS
        public void fail()
        {
            ctx.fail();
        }

        //DS starts receiving requests from Clients and MS
        public void recover()
        {
            ctx.recover();
        }

        public void dump()
        {
            ctx.dump();
        }

    }

    class DataServerClient : MarshalByRefObject, IClientToDS
    {
        public static DataServer ctx;

        //returns the version and content os local file
        public void read(string fileName, string semantics)
        {
            ctx.read(fileName, semantics);

        }

        //overwrites the content of file, creates new version
        public void write(string fileName, byte[] array)
        {
            ctx.write(fileName, array);
        }

    }

    class DataServerMS : MarshalByRefObject, IMSToDS
    {
        public static DataServer ctx;

        public void areYouAlive()
        {
            ctx.areYouAlive();
        }

        public void respostaMS(string resp)
        {
            ctx.respostaMS(resp);
        }
    }
}
