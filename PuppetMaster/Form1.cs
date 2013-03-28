using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTypes;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Collections;
using System.Diagnostics;

namespace PuppetMaster
{
    public delegate void RespostaClientHost(string resposta);
    public delegate void RespostaDSHost(string resposta);
    public delegate void RespostaMSHost(string resposta);

    public partial class Form1 : Form
    {
        //PuppetMaster puppet;
        public RespostaClientHost RespostaClientHostDelegate;
        public RespostaDSHost RespostaDSHostDelegate;
        public RespostaMSHost RespostaMSHostDelegate;

        public Dictionary<string, Process> runningProcesses = new Dictionary<string, Process>();

        //Lista de MetadataServers, DataServer e Clientes
        public Hashtable metaDataServers;
        public Hashtable dataServers;
        public Hashtable clients;

        public int idClient;
        public int idDS;


        public Form1()
        {
            InitializeComponent();
            PuppetMaster.ctx = this;

            TcpChannel channel = new TcpChannel(8060);
            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
            typeof(PuppetMaster),
            "PuppetMaster",
            WellKnownObjectMode.Singleton);

            metaDataServers = new Hashtable ();
            dataServers = new Hashtable ();
            clients = new Hashtable ();

            idClient = 0;
            idDS = 0;

            RespostaClientHostDelegate = new RespostaClientHost(updateCliente);
            RespostaDSHostDelegate = new RespostaDSHost(updateDS);
            RespostaMSHostDelegate = new RespostaMSHost(updateMS);


        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        //Cria Cliente
        private void button1_Click(object sender, EventArgs e)
        {
            string clientName = textBox1.Text;
            
            startClient(clientName);
            clients.Add(clientName, idClient);
                 

            listBox_clients.Items.Add(clientName);
            idClient++;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void listBox_clients_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        //mata cliente
         private void button_kill_client_Click(object sender, EventArgs e)
         {
             string nomeClient = listBox_clients.SelectedItem.ToString();
             stopClient(nomeClient);
         }

        //cria metadata 1
         private void button_start_meta1_Click(object sender, EventArgs e)
         {
             string currentDirectory = Environment.CurrentDirectory;
             string path = currentDirectory.Replace("PuppetMaster", "MetaDataServer");
             path += "/MetaDataServer.exe";

             String name = "MetadataServer1";

             metaDataServers.Add(name, "1");

             runningProcesses.Add(name, new Process());
             runningProcesses[name].StartInfo.Arguments = name + " " + "8081";
             runningProcesses[name].StartInfo.FileName = path;
             runningProcesses[name].Start();

             textBox1.Clear();
             listBox_metadata.Items.Add(name);
         }

        //cria metadata 2
         private void button_start_meta2_Click(object sender, EventArgs e)
         {
             string currentDirectory = Environment.CurrentDirectory;
             string path = currentDirectory.Replace("PuppetMaster", "MetaDataServer");
             path += "/MetaDataServer.exe";

             String name = "MetadataServer2";

             metaDataServers.Add(name, "2");

             runningProcesses.Add(name, new Process());
             runningProcesses[name].StartInfo.Arguments = name + " " + "8082";
             runningProcesses[name].StartInfo.FileName = path;
             runningProcesses[name].Start();

             textBox1.Clear();
             listBox_metadata.Items.Add(name);
         }

        //cria metadata 3
         private void button_start_meta3_Click(object sender, EventArgs e)
         {
             string currentDirectory = Environment.CurrentDirectory;
             string path = currentDirectory.Replace("PuppetMaster", "MetaDataServer");
             path += "/MetaDataServer.exe";

             String name = "MetadataServer3";

             metaDataServers.Add(name, "3");

             runningProcesses.Add(name, new Process());
             runningProcesses[name].StartInfo.Arguments = name + " " + "8083";
             runningProcesses[name].StartInfo.FileName = path;
             runningProcesses[name].Start();

             textBox1.Clear();
             listBox_metadata.Items.Add(name);
         }

        //cria dataServer
         private void button_start_data_Click(object sender, EventArgs e)
         {
             string dsName = textBox2.Text;

             startDS(dsName);
             dataServers.Add(dsName, idDS);
             idDS++;
         }

        
         private void button1_Click_1(object sender, EventArgs e)
         {
             
         }

         private void textBox3_TextChanged(object sender, EventArgs e)
         {

         }

        //Diz ao cliente para abrir ficheiro X
        //no cliente, este pede ao MS para abrir o ficheiro
         private void button_openFile_Click(object sender, EventArgs e)
         {   
             string nomeFile = textBox3.Text;
             string nomeClientSeleccionado = listBox_clients.SelectedItem.ToString();
             
             IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
            typeof(IPuppetToClient),
            "tcp://localhost:807" + clients[nomeClientSeleccionado] + "/" + nomeClientSeleccionado);



             if (client != null)
             {
                 //client.guardaMS(metaDataServers);
                 client.open(nomeFile);
             }
           
                 
                           
         }

         //Diz ao cliente para fechar ficheiro X
         //no cliente, este pede ao MS para fechar o ficheiro
         private void button_closeFile_Click(object sender, EventArgs e)
         {
             string nomeFile = textBox3.Text;
             string nomeClientSeleccionado = listBox_clients.SelectedItem.ToString();

             IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
            typeof(IPuppetToClient),
            "tcp://localhost:807" + clients[nomeClientSeleccionado] + "/" + nomeClientSeleccionado);

             if (client != null)
                 client.close(nomeFile);
         }

        //Diz ao MS para falhar
         private void button_fail_meta_Click(object sender, EventArgs e)
         {
             string nomeMS = listBox_metadata.SelectedItem.ToString();

             IPuppetToMS ms = (IPuppetToMS)Activator.GetObject(
            typeof(IPuppetToMS),
            "tcp://localhost:808" + metaDataServers[nomeMS] + "/" + nomeMS);

             if (ms != null)
                 ms.fail();
         }

        //Diz ao DS para freeze
         private void button_freeze_Click(object sender, EventArgs e)
         {
             string nomeDS = listBox_data.SelectedItem.ToString();

             IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
            typeof(IPuppetToDS),
            "tcp://localhost:809" + dataServers[nomeDS] + "/" + nomeDS);

             if (ds != null)
                 ds.freeze();
         }

        //Diz ao DS para falhar
         private void button_fail_data_Click(object sender, EventArgs e)
         {
             string nomeDS = listBox_data.SelectedItem.ToString();

             IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
            typeof(IPuppetToDS),
            "tcp://localhost:809" + dataServers[nomeDS] + "/" + nomeDS);

             if (ds != null)
                 ds.fail();
         }

        //Diz ao DS para unfreeze
         private void button_Unfreeze_Click(object sender, EventArgs e)
         {
             string nomeDS = listBox_data.SelectedItem.ToString();

             IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
            typeof(IPuppetToDS),
            "tcp://localhost:809" + dataServers[nomeDS] + "/" + nomeDS);

             if (ds != null)
                 ds.unfreeze();
         }

        //Diz ao DS par recuperar
         private void button_recover_data_Click(object sender, EventArgs e)
         {
             string nomeDS = listBox_data.SelectedItem.ToString();

             IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
            typeof(IPuppetToDS),
            "tcp://localhost:809" + dataServers[nomeDS] + "/" + nomeDS);

             if (ds != null)
                 ds.recover();
         }

         //Diz ao MS par recuperar
         private void button_recover_meta_Click(object sender, EventArgs e)
         {
             string nomeMS = listBox_metadata.SelectedItem.ToString();

             IPuppetToMS ms = (IPuppetToMS)Activator.GetObject(
            typeof(IPuppetToMS),
            "tcp://localhost:808" + metaDataServers[nomeMS] + "/" + nomeMS);

             if (ms != null)
                 ms.recover();
         }

         //faz update da lista Box dos clientes
         private void updateCliente(string resposta)
         {
             listBox_clients.Items.Add(resposta); //alterar para outra accao
         }

         //faz update da lista Box dos DS
         private void updateDS(string resposta)
         {
             listBox_data.Items.Add(resposta); //alterar para outra accao
         }

         //faz update da lista Box dos MS
         private void updateMS(string resposta)
         {
             listBox_metadata.Items.Add(resposta); //alterar para outra accao
         }

         //inicia o processo cliente
         public void startClient(string clientName)
         {
             string currentDirectory = Environment.CurrentDirectory;
             string path = currentDirectory.Replace("PuppetMaster", "Client");
             path += "/Client.exe";

             runningProcesses.Add(clientName, new Process());
             string port = "807" + idClient.ToString();
             runningProcesses[clientName].StartInfo.Arguments = clientName + " " + port;
             runningProcesses[clientName].StartInfo.FileName = path;
             runningProcesses[clientName].Start();

             textBox1.Clear();
             
         }

         //termina o processo cliente
         private void stopClient(string clientName)
         {
             runningProcesses[clientName].Kill();
             listBox_clients.Items.Remove(clientName);
         }

        //começa o processo do DS
         public void startDS(string dsName)
         {
             string currentDirectory = Environment.CurrentDirectory;
             string path = currentDirectory.Replace("PuppetMaster", "DataServer");
             path += "/DataServer.exe";

             runningProcesses.Add(dsName, new Process());
             string port = "809" + idDS.ToString();
             runningProcesses[dsName].StartInfo.Arguments = dsName + " " + port;
             runningProcesses[dsName].StartInfo.FileName = path;
             runningProcesses[dsName].Start();

             textBox2.Clear();
             listBox_data.Items.Add(dsName);
         }


    }

    public class PuppetMaster : MarshalByRefObject, IMSToPuppet, IClientToPuppet, IDSToPuppet
    {
        public static Form1 ctx;

       public void respostaClient(String resp)
        {
            ctx.Invoke(ctx.RespostaClientHostDelegate, new Object[] {resp});
        }

       public void respostaDS(String resp)
       {
           ctx.Invoke(ctx.RespostaDSHostDelegate, new Object[] { resp });
       }
       public void respostaMS(String resp)
       {
           ctx.Invoke(ctx.RespostaMSHostDelegate, new Object[] { resp });
       }

    }
}
