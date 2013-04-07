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
using System.IO;
using System.Text.RegularExpressions;

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

        public string[] scriptList;

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
             clients.Remove(nomeClient);
         }

        //cria metadata 1
         private void button_start_meta1_Click(object sender, EventArgs e)
         {
             String name = "m-1";
             startMS(name, "1");

             metaDataServers.Add(name, "1");
             textBox1.Clear();
             listBox_metadata.Items.Add(name);
         }

        //cria metadata 2
         private void button_start_meta2_Click(object sender, EventArgs e)
         {
             String name = "m-2";
             startMS(name, "2");

             metaDataServers.Add(name, "2");

             textBox1.Clear();
             listBox_metadata.Items.Add(name);
         }

        //cria metadata 3
         private void button_start_meta3_Click(object sender, EventArgs e)
         {
             String name = "m-3";
             startMS(name, "3");

             metaDataServers.Add(name, "3");

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
                 client.guardaMS(metaDataServers);
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

         public void startMS(string name, string id)
         {
             string currentDirectory = Environment.CurrentDirectory;
             string path = currentDirectory.Replace("PuppetMaster", "MetaDataServer");
             path += "/MetaDataServer.exe";

             runningProcesses.Add(name, new Process());
             runningProcesses[name].StartInfo.Arguments = name + " " + "808" + id;
             runningProcesses[name].StartInfo.FileName = path;
             runningProcesses[name].Start();

         }

         //termina o processo cliente
         private void stopClient(string clientName)
         {
             runningProcesses[clientName].Kill();
             listBox_clients.Items.Remove(clientName);
         }

         //termina o processo dataServer
         private void stopDataServer(string dataName)
         {
             runningProcesses[dataName].Kill();
             listBox_data.Items.Remove(dataName);
         }

         //termina o processo metaData
         private void stopMetaDataServer(string metaDataName)
         {
             runningProcesses[metaDataName].Kill();
             listBox_metadata.Items.Remove(metaDataName);
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

         private void listBox_scripts_SelectedIndexChanged(object sender, EventArgs e)
         {
             string scriptLine;

             if (listBox_scripts.SelectedItem != null)
             {
                 string scriptPath = (string)listBox_scripts.SelectedItem;
                 StreamReader userscript = new StreamReader(scriptPath);

                 listBox_script_steps.Items.Clear();
                 while ((scriptLine = userscript.ReadLine()) != null)
                 {
                     listBox_script_steps.Items.Add(scriptLine);
                 }
             }
         }

         private void button_script_Click(object sender, EventArgs e)
         {
             // Displays an OpenFileDialog so the user can select a Cursor.
             OpenFileDialog openFileDialog1 = new OpenFileDialog();
             openFileDialog1.Filter = "Script|*.txt";
             openFileDialog1.Title = "Escolha o Script";


             if (openFileDialog1.ShowDialog() == DialogResult.OK)
             {
                 if (listBox_scripts.FindString(openFileDialog1.FileName, 0) != 0)
                 {
                     listBox_scripts.Items.Add(openFileDialog1.FileName);
                 }

             }
         }

         private void button_get_script_Click(object sender, EventArgs e)
         {

         }

         private void listBox_script_steps_SelectedIndexChanged(object sender, EventArgs e)
         {

         }

         private void button_run_all_Click(object sender, EventArgs e)
         {
             RunScript();
         }

         public void RunScript()
         {
             //KillAll();

             if (listBox_script_steps.Items.Count != 0)
             {
                 foreach (string operation in listBox_script_steps.Items)
                 {
                     RunInstruction(operation);
                 }
             }
         }

         private void RunInstruction(string operation)
         {
             string[] token = new string [] { " ", "\t" , ", "};
             string[] arg = operation.Split(token, StringSplitOptions.None);

             if (operation.StartsWith("FAIL"))
             {

                 if (arg[1].StartsWith("c"))
                 {
                     if (clients[arg[1]] != null)
                     {
                         IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                        typeof(IPuppetToClient),
                        "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1]);

                         client.fail();
                     }
                     else
                     {
                         //lança popup - nao existe o cliente
                         System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                         startClient(arg[1]);
                         clients.Add(arg[1], idClient);
                         listBox_clients.Items.Add(arg[1]);
                         idClient++;
                     }
                 }
                 else if (arg[1].StartsWith("d"))
                 {
                     if (dataServers[arg[1]] != null)
                     {
                         IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
                        typeof(IPuppetToDS),
                        "tcp://localhost:809" + dataServers[arg[1]] + "/" + arg[1]);

                         ds.fail();
                     }
                     else
                     {
                         //lança popup - nao existe o server
                         System.Windows.Forms.MessageBox.Show("O DataServer " + arg[1] + " nao existe!-" + arg[0]);
                         startDS(arg[1]);
                         dataServers.Add(arg[1], idDS);
                         listBox_data.Items.Add(arg[1]);
                         idDS++;
                     }
                 }
                 else if (arg[1].StartsWith("m"))
                 {
                     if (metaDataServers[arg[1]] != null)
                     {
                         IPuppetToMS ms = (IPuppetToMS)Activator.GetObject(
                        typeof(IPuppetToMS),
                        "tcp://localhost:808" + metaDataServers[arg[1]] + "/" + arg[1]);

                         ms.fail();
                     }
                     else
                     {
                         //lança popup - nao existe o server
                         System.Windows.Forms.MessageBox.Show("O MetadataServer " + arg[1] + " nao existe!-" + arg[0]);
                         startMS(arg[1], arg[1].Last().ToString());
                         metaDataServers.Add(arg[1], arg[1].Last().ToString());
                         listBox_metadata.Items.Add(arg[1]);
                     }
                 }
             }
             else if (operation.StartsWith("RECOVER"))
             {
                 if (arg[1].StartsWith("c"))
                 {
                     if (clients[arg[1]] != null)
                     {
                         IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                        typeof(IPuppetToClient),
                        "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1]);

                         client.recover();
                     }
                     else
                     {
                         //lança popup - nao existe o cliente
                         System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                         startClient(arg[1]);
                         clients.Add(arg[1], idClient);
                         listBox_clients.Items.Add(arg[1]);
                         idClient++;
                     }
                 }
                 else if (arg[1].StartsWith("d"))
                 {
                     if (dataServers[arg[1]] != null)
                     {
                         IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
                        typeof(IPuppetToDS),
                        "tcp://localhost:809" + dataServers[arg[1]] + "/" + arg[1]);

                         ds.recover();
                     }
                     else
                     {
                         //lança popup - nao existe o server
                         System.Windows.Forms.MessageBox.Show("O DataServer " + arg[1] + " nao existe!-" + arg[0]);
                         startDS(arg[1]);
                         dataServers.Add(arg[1], idDS);
                         listBox_data.Items.Add(arg[1]);
                         idDS++;
                     }
                 }
                 else if (arg[1].StartsWith("m"))
                 {
                     if (metaDataServers[arg[1]] != null)
                     {
                         IPuppetToMS ms = (IPuppetToMS)Activator.GetObject(
                        typeof(IPuppetToMS),
                        "tcp://localhost:808" + metaDataServers[arg[1]] + "/" + arg[1]);

                         ms.recover();
                     }
                     else
                     {
                         //lança popup - nao existe o server
                         System.Windows.Forms.MessageBox.Show("O MetadataServer " + arg[1] + " nao existe!-" + arg[0]);
                         startMS(arg[1], arg[1].Last().ToString());
                         metaDataServers.Add(arg[1], arg[1].Last().ToString());
                         listBox_metadata.Items.Add(arg[1]);
                     }
                 }
             }
             else if (operation.StartsWith("FREEZE"))
             {
                 if (arg[1].StartsWith("c"))
                 {
                     if (clients[arg[1]] != null)
                     {
                         IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                        typeof(IPuppetToClient),
                        "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1]);

                         client.freeze();
                     }
                     else
                     {
                         //lança popup - nao existe o cliente
                         System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                         startClient(arg[1]);
                         clients.Add(arg[1], idClient);
                         listBox_clients.Items.Add(arg[1]);
                         idClient++;
                     }
                 }
                 else if (arg[1].StartsWith("d"))
                 {
                     if (dataServers[arg[1]] != null)
                     {
                         IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
                        typeof(IPuppetToDS),
                        "tcp://localhost:809" + dataServers[arg[1]] + "/" + arg[1]);

                         ds.freeze();
                     }
                     else
                     {
                         //lança popup - nao existe o server
                         System.Windows.Forms.MessageBox.Show("O DataServer " + arg[1] + " nao existe!-" + arg[0]);
                         startDS(arg[1]);
                         dataServers.Add(arg[1], idDS);
                         listBox_data.Items.Add(arg[1]);
                         idDS++;
                     }
                 }
                 else if (arg[1].StartsWith("m"))
                 {
                     if (metaDataServers[arg[1]] != null)
                     {
                         IPuppetToMS ms = (IPuppetToMS)Activator.GetObject(
                        typeof(IPuppetToMS),
                        "tcp://localhost:808" + metaDataServers[arg[1]] + "/" + arg[1]);

                         ms.freeze();
                     }
                     else
                     {
                         //lança popup - nao existe o server
                         System.Windows.Forms.MessageBox.Show("O MetadataServer " + arg[1] + " nao existe!-" + arg[0]);
                         startMS(arg[1], arg[1].Last().ToString());
                         metaDataServers.Add(arg[1], arg[1].Last().ToString());
                         listBox_metadata.Items.Add(arg[1]);
                     }
                 }
             }
             else if (operation.StartsWith("UNFREEZE"))
             {
                 if (arg[1].StartsWith("c"))
                 {
                     if (clients[arg[1]] != null)
                     {
                         IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                        typeof(IPuppetToClient),
                        "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1]);

                         client.unfreeze();
                     }
                     else
                     {
                         //lança popup - nao existe o cliente
                         System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                         startClient(arg[1]);
                         clients.Add(arg[1], idClient);
                         listBox_clients.Items.Add(arg[1]);
                         idClient++;
                     }
                 }
                 else if (arg[1].StartsWith("d"))
                 {
                     if (dataServers[arg[1]] != null)
                     {
                         IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
                        typeof(IPuppetToDS),
                        "tcp://localhost:809" + dataServers[arg[1]] + "/" + arg[1]);

                         ds.unfreeze();
                     }
                     else
                     {
                         //lança popup - nao existe o server
                         System.Windows.Forms.MessageBox.Show("O DataServer " + arg[1] + " nao existe!-" + arg[0]);
                         startDS(arg[1]);
                         dataServers.Add(arg[1], idDS);
                         listBox_data.Items.Add(arg[1]);
                         idDS++;
                     }
                 }
                 else if (arg[1].StartsWith("m"))
                 {
                     if (metaDataServers[arg[1]] != null)
                     {
                         IPuppetToMS ms = (IPuppetToMS)Activator.GetObject(
                        typeof(IPuppetToMS),
                        "tcp://localhost:808" + metaDataServers[arg[1]] + "/" + arg[1]);

                         ms.unfreeze();
                     }
                     else
                     {
                         //lança popup - nao existe o server
                         System.Windows.Forms.MessageBox.Show("O MetadataServer " + arg[1] + " nao existe!-" + arg[0]);
                         startMS(arg[1], arg[1].Last().ToString());
                         metaDataServers.Add(arg[1], arg[1].Last().ToString());
                         listBox_metadata.Items.Add(arg[1]);
                     }
                 }
             }
             else if (operation.StartsWith("CREATE"))
             {
                 if (clients[arg[1]] != null)
                 {
                     IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                    typeof(IPuppetToClient),
                    "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1]);

                     client.create(arg[2], Int32.Parse(arg[3]), Int32.Parse(arg[4]), Int32.Parse(arg[5]));
                 }
                 else
                 {
                     //lança popup - nao existe o cliente
                     System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                     startClient(arg[1]);
                     clients.Add(arg[1], idClient);
                     listBox_clients.Items.Add(arg[1]);
                     idClient++;
                 }
             }
             else if (operation.StartsWith("OPEN"))
             {
                 if (clients[arg[1]] != null)
                 {
                     IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                    typeof(IPuppetToClient),
                    "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1]);

                     client.open(arg[2]);
                 }
                 else
                 {
                     //lança popup - nao existe o cliente
                     System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                     startClient(arg[1]);
                     clients.Add(arg[1], idClient);
                     listBox_clients.Items.Add(arg[1]);
                     idClient++;
                 }
             }
             else if (operation.StartsWith("CLOSE"))
             {
                 if (clients[arg[1]] != null)
                 {
                     IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                    typeof(IPuppetToClient),
                    "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1]);

                     client.close(arg[2]);
                 }
                 else
                 {
                     //lança popup - nao existe o cliente
                     System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                     startClient(arg[1]);
                     clients.Add(arg[1], idClient);
                     listBox_clients.Items.Add(arg[1]);
                     idClient++;
                 }
             }
             else if (operation.StartsWith("READ"))
             {
                 if (clients[arg[1]] != null)
                 {
                     IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                    typeof(IPuppetToClient),
                    "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1]);

                     //arg[4] e o string register
                     //reads the contents of the file idetified bye  file-register (arg[2])
                     //and stores it in a string register (arg[4]) int the puppet
                     client.read(arg[2], arg[3]);
                 }
                 else
                 {
                     //lança popup - nao existe o cliente
                     System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                     startClient(arg[1]);
                     clients.Add(arg[1], idClient);
                     listBox_clients.Items.Add(arg[1]);
                     idClient++;
                 }
             }
             else if (operation.StartsWith("WRITE")) //EXISTEM 2 TIPOS DE WRITE
             {
                 if (clients[arg[1]] != null)
                 {
                     IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                    typeof(IPuppetToClient),
                    "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1]);

                     if (arg[3].Length > 1)
                     {
                         //ex: WRITE c-1, 0, "Text contents of the file. Contents are a string delimited by double quotes as this one"
                         //client.write();
                     }
                     else
                     {
                         System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                         Byte[] bytes = encoding.GetBytes(arg[3]);
                         //ex: WRITE c-1, 0, 0
                         client.write(arg[2], bytes);
                     }
                     
                 }
                 else
                 {
                     //lança popup - nao existe o cliente
                     System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                     startClient(arg[1]);
                     clients.Add(arg[1], idClient);
                     listBox_clients.Items.Add(arg[1]);
                     idClient++;
                 }
             }
             else if (operation.StartsWith("COPY"))
             {
                 //quem faz o copy???
             }
             else if (operation.StartsWith("DUMP"))
             {
                 //quem faz dump?? e onde??
             }
             else if (operation.StartsWith("EXESCRIPT"))
             {
                 List<string> operations = new List<string>();
                 string scriptLine = null;
                 string scriptPath = "";
                 string currentDirectory = Environment.CurrentDirectory;
                 string[] newDirectory = Regex.Split(currentDirectory, "PuppetMaster");
                 string strpath = newDirectory[0] + "Scripts\\";
                 for (int i = 2; i < arg.Length; i++)
                 {
                     scriptPath += arg[i] + " ";
                 }
                 scriptPath = strpath + scriptPath;
                 StreamReader userscript = new StreamReader(scriptPath);

                 while ((scriptLine = userscript.ReadLine()) != null)
                 {
                     operations.Add(scriptLine);
                 }

                 if (clients[arg[1]] != null)
                 {
                     IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                    typeof(IPuppetToClient),
                    "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1]);
                     client.runScript(operations);
                 }
                 else
                 {
                     //lança popup - nao existe o cliente
                     System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                     startClient(arg[1]);
                     clients.Add(arg[1], idClient);
                     listBox_clients.Items.Add(arg[1]);
                     idClient++;
                 }
                 
             }

         }

         private void KillAll()
         {

             //Kill All Clients            
             foreach (DictionaryEntry c in clients)
             {
                 stopClient(c.Key.ToString());
             }
             clients.Clear();

             //Kill All dataServers            
             foreach (DictionaryEntry d in dataServers)
             {
                 stopDataServer(d.Key.ToString());
             }
             dataServers.Clear();

             //Kill All MetadataServers            
             foreach (DictionaryEntry m in metaDataServers)
             {
                 stopMetaDataServer(m.Key.ToString());
             }
             metaDataServers.Clear();

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
