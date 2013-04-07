using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;

namespace MetaDataServer
{
    class Program : MarshalByRefObject, IClientToMS, IPuppetToMS, IDSToMS 
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

        /********Puppet To MetaDataServer***********/

        //the MS stops processing requests from clients or others MS
        public void  fail()
        {
            System.Console.WriteLine("puppet mandou MS falhar!");

            //Nao se pode fazer ciclo
           /*IMSToPuppet puppet = (IMSToPuppet)Activator.GetObject(
           typeof(IMSToPuppet),
           "tcp://localhost:8060/PuppetMaster");
            
            puppet.respostaMS("Puppet mandou fazer fail");*/
        }

        //MS starts receiving requests from clients and others MS
        public void recover()
        {
            System.Console.WriteLine("puppet mandou MS recuperar!");
        }


        /********Client To MetaDataServer***********/

        //returns to client the contents of the metadata stored for that file
        public void open(string fileName)
        {
            System.Console.WriteLine("cliente mandou MS abrir ficheiro: " + fileName);

            //Nao se pode fazer ciclo
            /*IMSToClient client = (IMSToClient)Activator.GetObject(
            typeof(IMSToClient),
            "tcp://localhost:8070/Client");

            client.respostaMS("MS envia info sobre ficheiro a abrir");*/
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

            //Nao se pode fazer ciclo
            /*IMSToDS ds = (IMSToDS)Activator.GetObject(
            typeof(IMSToDS),
            "tcp://localhost:8090/DataServer");

            ds.respostaMS("Recebi o teu i'm alive.");*/
        }
    }
}
