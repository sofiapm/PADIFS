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

namespace MetaDataServer
{
    class Program
    {
        
        Program()
        {
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

            MetaServer meta = new MetaServer (channel);
            MetaServerPuppet.ctx = meta;
            MetaServerClient.ctx = meta;
            MetaServerDS.ctx = meta;

            System.Console.WriteLine(args[0] + ": <enter> para sair..." + args[1]);

            System.Console.ReadLine();
        }

    }

    class MetaServer
    {
        TcpChannel channel;

        public MetaServer(TcpChannel channel)
        {
            this.channel = channel;
        }

        /********Puppet To MetaDataServer***********/

        //the MS stops processing requests from clients or others MS
        public void fail()
        {
            System.Console.WriteLine("puppet mandou MS falhar!");

        }

        //MS starts receiving requests from clients and others MS
        public void recover()
        {
            System.Console.WriteLine("puppet mandou MS recuperar!");
        }

        public void dump()
        {
            System.Console.WriteLine("Puppet mandou o MS fazer Dump");
        }


        /********Client To MetaDataServer***********/

        //returns to client the contents of the metadata stored for that file
        public DadosFicheiro open(string fileName)
        {
            System.Console.WriteLine("cliente mandou MS abrir ficheiro: " + fileName);

            System.Console.WriteLine("olaa fiiii, estou a alterar aqui");

            //igualar a hashtable de DS a retornar ao cliente
            Hashtable n = new Hashtable ();

            n.Add(1, 2);

            ArrayList y = new ArrayList();
            y.Add(1);
            return new DadosFicheiro(1, 2, y);

        }

        //informs MS that client is no longer using that file - client must discard all metadata for that file
        public void close(string fileName)
        {
            System.Console.WriteLine("cliente mandou MS fechar ficheiro: " + fileName);
        }

        //creates a new file (if it doesn t exist) - in case of sucesses, returns the same that open
        public void create(string fileName, int numDS, int rQuorum, int wQuorum)
        {
            System.Console.WriteLine("cliente mandou MS criar ficheiro: " + fileName);
        }

        //deletes the file
        public void delete(string fileName)
        {
            System.Console.WriteLine("cliente mandou MS apagar ficheiro: " + fileName);
        }

        /********DS To MetadataServer***********/
        public void respostaDS(string resposta)
        {
            System.Console.WriteLine(resposta);

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
        public void create(string fileName, int numDS, int rQuorum, int wQuorum)
        {
            ctx.create(fileName, numDS, rQuorum, wQuorum);
        }

        //deletes the file
        public void delete(string fileName)
        {
            ctx.delete(fileName);
        }
   
    }

    class MetaServerDS : MarshalByRefObject, IDSToMS
    {
        public static MetaServer ctx;

        public void respostaDS(string resposta)
        {
            ctx.respostaDS(resposta);
        }
    }
}
