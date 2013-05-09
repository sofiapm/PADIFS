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

            metaDataServers = new Hashtable();
            dataServers = new Hashtable();
            clients = new Hashtable();

            idClient = 0;
            idDS = 0;

            RespostaClientHostDelegate = new RespostaClientHost(updateCliente);
            RespostaDSHostDelegate = new RespostaDSHost(updateDS);
            RespostaMSHostDelegate = new RespostaMSHost(updateMS);


        }

        public void dumpClient(String d)
        {
            //listBox_dump_client.Items.Clear();

            string[] words = d.Split('\n');
            foreach (string word in words)
            {
                listBox_dump_client.Items.Add(word);
            }
        }

        public void dumpDS(String d)
        {
            //listBox_dump_client.Items.Clear();

            string[] words = d.Split('\n');
            foreach (string word in words)
            {
                listBox_dump_data.Items.Add(word);
            }
        }

        public void dumpMS(String d)
        {
            //listBox_dump_client.Items.Clear();

            string[] words = d.Split('\n');
            foreach (string word in words)
            {
                listBox_dump_meta.Items.Add(word);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void listBox_clients_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

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

            //textBox1.Clear();

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
            runningProcesses.Remove(clientName);
            listBox_clients.Items.Remove(clientName);
        }

        //termina o processo dataServer
        private void stopDataServer(string dataName)
        {
            runningProcesses[dataName].Kill();
            runningProcesses.Remove(dataName);
            listBox_data.Items.Remove(dataName);
        }

        //termina o processo metaData
        private void stopMetaDataServer(string metaDataName)
        {
            runningProcesses[metaDataName].Kill();
            runningProcesses.Remove(metaDataName);
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

            //textBox2.Clear();
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

        //Botao RunAll
        private void button_run_all_Click(object sender, EventArgs e)
        {
            RunScript();
        }

        public void RunScript()
        {
            KillAll();

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
            try
            {
                string[] token = new string[] { " ", "\t", ", ", "," };
                string[] arg = operation.Split(token, StringSplitOptions.None);


                if (operation.StartsWith("FAIL"))
                {

                    if (arg[1].StartsWith("d"))
                    {
                        if (dataServers[arg[1]] != null)
                        {
                            IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
                          typeof(IPuppetToDS),
                          "tcp://localhost:809" + dataServers[arg[1]] + "/" + arg[1] + "DataServerPuppet");

                            ds.fail();

                        }
                        else
                        {
                            //lança popup, nao cria o DS
                            //System.Windows.Forms.MessageBox.Show("O DataServer " + arg[1] + " nao existe!-" + arg[0]);
                        }


                    }
                    else if (arg[1].StartsWith("m"))
                    {
                        try
                        {
                            IPuppetToMS ms = (IPuppetToMS)Activator.GetObject(
                           typeof(IPuppetToMS),
                           "tcp://localhost:808" + metaDataServers[arg[1]] + "/" + arg[1] + "MetaServerPuppet");

                            ms.fail();
                        }
                        catch
                        {
                            //lança popup
                            //System.Windows.Forms.MessageBox.Show("O MetaDataServer " + arg[1] + " nao existe!-" + arg[0]);
                        }


                    }
                }
                else if (operation.StartsWith("RECOVER"))
                {
                    if (arg[1].StartsWith("d"))
                    {
                        if (dataServers[arg[1]] != null)
                        {
                            IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
                           typeof(IPuppetToDS),
                           "tcp://localhost:809" + dataServers[arg[1]] + "/" + arg[1] + "DataServerPuppet");

                            ds.recover();
                        }
                        else
                        {
                            //lança popup
                            //System.Windows.Forms.MessageBox.Show("O DataServer " + arg[1] + " nao existe!-" + arg[0]);
                        }


                    }
                    else if (arg[1].StartsWith("m"))
                    {
                        try
                        {
                            IPuppetToMS ms = (IPuppetToMS)Activator.GetObject(
                          typeof(IPuppetToMS),
                          "tcp://localhost:808" + metaDataServers[arg[1]] + "/" + arg[1] + "MetaServerPuppet");

                            ms.recover();
                        }
                        catch
                        {
                            //lança popup - nao existe o server
                            //System.Windows.Forms.MessageBox.Show("O MetadataServer " + arg[1] + " nao existe!-" + arg[0]);

                            int num = Int32.Parse(arg[1].Last().ToString());
                            num = num + 1;
                            startMS(arg[1], num + "");
                            metaDataServers.Add(arg[1], num + "");
                            listBox_metadata.Items.Add(arg[1]);

                            IPuppetToMS ms = (IPuppetToMS)Activator.GetObject(
                               typeof(IPuppetToMS),
                               "tcp://localhost:808" + metaDataServers[arg[1]] + "/" + arg[1] + "MetaServerPuppet");

                            ms.recover();
                        }

                    }
                }
                else if (operation.StartsWith("FREEZE"))
                {
                    if (arg[1].StartsWith("d"))
                    {
                        if (dataServers[arg[1]] != null)
                        {
                            IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
                           typeof(IPuppetToDS),
                           "tcp://localhost:809" + dataServers[arg[1]] + "/" + arg[1] + "DataServerPuppet");

                            ds.freeze();
                        }
                        else
                        {
                            //lança popup
                            //System.Windows.Forms.MessageBox.Show("O DataServer " + arg[1] + " nao existe!-" + arg[0]);
                        }

                    }

                }
                else if (operation.StartsWith("UNFREEZE"))
                {
                    if (arg[1].StartsWith("d"))
                    {
                        if (dataServers[arg[1]] == null)
                        {
                            //lança popup - nao existe o server
                            //System.Windows.Forms.MessageBox.Show("O DataServer " + arg[1] + " nao existe!-" + arg[0]);
                            startDS(arg[1]);
                            dataServers.Add(arg[1], idDS);
                            //listBox_data.Items.Add(arg[1]);
                            idDS++;
                        }

                        IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
                           typeof(IPuppetToDS),
                           "tcp://localhost:809" + dataServers[arg[1]] + "/" + arg[1] + "DataServerPuppet");

                        ds.unfreeze();
                    }

                }
                else if (operation.StartsWith("CREATE"))
                {
                    if (clients[arg[1]] != null)
                    {
                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                      typeof(IPuppetToClient),
                      "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        client.create(arg[2], Int32.Parse(arg[3]), Int32.Parse(arg[4]), Int32.Parse(arg[5]));
                    }
                    else
                    {
                        //lança popup - nao existe o cliente
                        //System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                        startClient(arg[1]);
                        clients.Add(arg[1], idClient);
                        listBox_clients.Items.Add(arg[1]);
                        idClient++;

                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                       typeof(IPuppetToClient),
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        //client.guardaMS(metaDataServers);


                        client.create(arg[2], Int32.Parse(arg[3]), Int32.Parse(arg[4]), Int32.Parse(arg[5]));

                    }
                }
                else if (operation.StartsWith("DELETE"))
                {
                    if (clients[arg[1]] != null)
                    {
                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                       typeof(IPuppetToClient),
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");


                        client.delete(arg[2]);
                    }
                    else
                    {
                        //lança popup - nao existe o cliente
                        //System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                        startClient(arg[1]);
                        clients.Add(arg[1], idClient);
                        listBox_clients.Items.Add(arg[1]);
                        idClient++;

                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                       typeof(IPuppetToClient),
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        //client.guardaMS(metaDataServers);

                        client.delete(arg[2]);

                    }
                }
                else if (operation.StartsWith("OPEN"))
                {
                    if (clients[arg[1]] != null)
                    {
                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                       typeof(IPuppetToClient),
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        client.open(arg[2]);
                    }
                    else
                    {
                        //lança popup - nao existe o cliente
                        //System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                        startClient(arg[1]);
                        clients.Add(arg[1], idClient);
                        listBox_clients.Items.Add(arg[1]);
                        idClient++;

                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                      typeof(IPuppetToClient),
                      "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        //client.guardaMS(metaDataServers);
                        client.open(arg[2]);
                    }
                }
                else if (operation.StartsWith("CLOSE"))
                {
                    if (clients[arg[1]] != null)
                    {
                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                       typeof(IPuppetToClient),
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        client.close(arg[2]);
                    }
                    else
                    {
                        //lança popup - nao existe o cliente
                        //System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                        startClient(arg[1]);
                        clients.Add(arg[1], idClient);
                        listBox_clients.Items.Add(arg[1]);
                        idClient++;

                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                       typeof(IPuppetToClient),
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        //client.guardaMS(metaDataServers);
                        client.close(arg[2]);
                    }
                }
                else if (operation.StartsWith("READ"))
                {
                    if (clients[arg[1]] != null)
                    {
                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                       typeof(IPuppetToClient),
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        //arg[4] e o string register
                        //reads the contents of the file idetified bye  file-register (arg[2])
                        //and stores it in a string register (arg[4]) int the puppet
                        client.read(Int32.Parse(arg[2]), arg[3], Int32.Parse(arg[4]));
                    }
                    else
                    {
                        //lança popup - nao existe o cliente
                        //System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                        startClient(arg[1]);
                        clients.Add(arg[1], idClient);
                        listBox_clients.Items.Add(arg[1]);
                        idClient++;

                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                       typeof(IPuppetToClient),
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        //client.guardaMS(metaDataServers);

                        //arg[4] e o string register
                        //reads the contents of the file idetified bye  file-register (arg[2])
                        //and stores it in a string register (arg[4]) int the puppet
                        client.read(Int32.Parse(arg[2]), arg[3], Int32.Parse(arg[4]));
                    }
                }
                else if (operation.StartsWith("WRITE")) //EXISTEM 2 TIPOS DE WRITE
                {
                    if (clients[arg[1]] != null)
                    {
                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                       typeof(IPuppetToClient),
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        if (arg[3].Length > 1)
                        {
                            //ex: WRITE c-1, 0, "Text contents of the file. Contents are a string delimited by double quotes as this one"
                            string s = arg[3];
                            int i = 4;
                            while (true)
                            {
                                try
                                {
                                    s = s + " " + arg[i];
                                    i++;
                                }
                                catch
                                {
                                    break;
                                }

                            }
                            client.writeS(Int32.Parse(arg[2]), s);
                        }
                        else
                        {
                            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

                            //ex: WRITE c-1, 0, 0
                            client.writeR(Int32.Parse(arg[2]), Int32.Parse(arg[3]));
                        }

                    }
                    else
                    {
                        //lança popup - nao existe o cliente
                        //System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                        startClient(arg[1]);
                        clients.Add(arg[1], idClient);
                        listBox_clients.Items.Add(arg[1]);
                        idClient++;

                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                       typeof(IPuppetToClient),
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        //client.guardaMS(metaDataServers);

                        if (arg[3].Length > 1)
                        {
                            //ex: WRITE c-1, 0, "Text contents of the file. Contents are a string delimited by double quotes as this one"
                            client.writeS(Int32.Parse(arg[2]), arg[3]);
                        }
                        else
                        {
                            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();

                            //ex: WRITE c-1, 0, 0
                            client.writeR(Int32.Parse(arg[2]), Int32.Parse(arg[3]));
                        }
                    }
                }
                else if (operation.StartsWith("COPY"))
                {
                    if (clients[arg[1]] != null)
                    {
                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                       typeof(IPuppetToClient),
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        string s = arg[5];
                        int i = 6;
                        while (true)
                        {
                            try
                            {
                                s = s + " " + arg[i];
                                i++;
                            }
                            catch
                            {
                                break;
                            }

                        }

                        client.copy(Int32.Parse(arg[2]), arg[3], Int32.Parse(arg[4]), s);
                    }
                    else
                    {
                        //lança popup - nao existe o cliente
                        //System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                        startClient(arg[1]);
                        clients.Add(arg[1], idClient);
                        listBox_clients.Items.Add(arg[1]);
                        idClient++;

                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                       typeof(IPuppetToClient),
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        //client.guardaMS(metaDataServers);


                        client.copy(Int32.Parse(arg[2]), arg[3], Int32.Parse(arg[4]), arg[4]);

                    }
                }
                else if (operation.StartsWith("DUMP"))
                {
                    if (arg[1].StartsWith("c"))
                    {
                        if (clients[arg[1]] != null)
                        {
                            IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                           typeof(IPuppetToClient),
                           "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                            dumpClient(client.dump());
                        }
                        else
                        {
                            //lança popup - nao existe o cliente
                            //System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                            startClient(arg[1]);
                            clients.Add(arg[1], idClient);
                            listBox_clients.Items.Add(arg[1]);
                            idClient++;

                            IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                           typeof(IPuppetToClient),
                           "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                            //client.guardaMS(metaDataServers);
                            dumpClient(client.dump());

                        }
                    }
                    else if (arg[1].StartsWith("d"))
                    {
                        if (dataServers[arg[1]] != null)
                        {
                            IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
                           typeof(IPuppetToDS),
                           "tcp://localhost:809" + dataServers[arg[1]] + "/" + arg[1] + "DataServerPuppet");

                            dumpDS(ds.dump());

                        }


                    }
                    else if (arg[1].StartsWith("m"))
                    {
                        try
                        {
                            IPuppetToMS ms = (IPuppetToMS)Activator.GetObject(
                          typeof(IPuppetToMS),
                          "tcp://localhost:808" + metaDataServers[arg[1]] + "/" + arg[1] + "MetaServerPuppet");

                            dumpMS(ms.dump());
                        }
                        catch
                        {
                            //lança popup
                            //System.Windows.Forms.MessageBox.Show("O MetaDataServer " + arg[1] + " nao existe!-" + arg[0]);
                        }
                    }

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
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");
                        client.runScript(operations);
                    }
                    else
                    {
                        //lança popup - nao existe o cliente
                        //System.Windows.Forms.MessageBox.Show("O Cliente " + arg[1] + " nao existe!-" + arg[0]);
                        startClient(arg[1]);
                        clients.Add(arg[1], idClient);
                        listBox_clients.Items.Add(arg[1]);
                        idClient++;

                        IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                       typeof(IPuppetToClient),
                       "tcp://localhost:807" + clients[arg[1]] + "/" + arg[1] + "PuppetClient");

                        //client.guardaMS(metaDataServers);
                        client.runScript(operations);
                    }

                }
            }
            catch
            {

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
            listBox_dump_client.Items.Clear();

            //Kill All dataServers            
            foreach (DictionaryEntry d in dataServers)
            {
                stopDataServer(d.Key.ToString());
            }
            dataServers.Clear();
            listBox_dump_data.Items.Clear();

            //Kill All MetadataServers            
            foreach (DictionaryEntry m in metaDataServers)
            {
                stopMetaDataServer(m.Key.ToString());
            }
            metaDataServers.Clear();
            listBox_dump_meta.Items.Clear();

        }

        //Botao DumpALL
        private void button1_Click_2(object sender, EventArgs e)
        {
            foreach (DictionaryEntry en in metaDataServers)
            {
                IPuppetToMS ms = (IPuppetToMS)Activator.GetObject(
                       typeof(IPuppetToMS),
                       "tcp://localhost:808" + en.Value + "/" + en.Key + "MetaServerPuppet");

                dumpMS(ms.dump());
            }

            foreach (DictionaryEntry en in dataServers)
            {
                IPuppetToDS ds = (IPuppetToDS)Activator.GetObject(
                       typeof(IPuppetToDS),
                       "tcp://localhost:809" + en.Value + "/" + en.Key + "DataServerPuppet");

                dumpDS(ds.dump());
            }

            foreach (DictionaryEntry en in clients)
            {
                IPuppetToClient client = (IPuppetToClient)Activator.GetObject(
                    typeof(IPuppetToClient),
                    "tcp://localhost:807" + en.Value + "/" + en.Key + "PuppetClient");
                dumpClient(client.dump());
            }

        }

        //Butao KILLALL
        private void button2_Click(object sender, EventArgs e)
        {
            KillAll();
        }

        private void button_runStep_Click(object sender, EventArgs e)
        {
            string operation = (string)listBox_script_steps.SelectedItem;
            RunInstruction(operation);
            if (listBox_script_steps.SelectedIndex + 1 != listBox_script_steps.Items.Count) listBox_script_steps.SelectedIndex++;
        }

    }

    public class PuppetMaster : MarshalByRefObject, IMSToPuppet, IClientToPuppet, IDSToPuppet
    {
        public static Form1 ctx;

        public void respostaClient(String resp)
        {
            ctx.Invoke(ctx.RespostaClientHostDelegate, new Object[] { resp });
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
