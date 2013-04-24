namespace PuppetMaster
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.listBox_dump_client = new System.Windows.Forms.ListBox();
            this.listBox_clients = new System.Windows.Forms.ListBox();
            this.label_clients = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.listBox_metadata = new System.Windows.Forms.ListBox();
            this.label_metadata = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.listBox_data = new System.Windows.Forms.ListBox();
            this.label_data = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.button_runStep = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button_script = new System.Windows.Forms.Button();
            this.button_run_all = new System.Windows.Forms.Button();
            this.listBox_script_steps = new System.Windows.Forms.ListBox();
            this.listBox_scripts = new System.Windows.Forms.ListBox();
            this.label_scripts = new System.Windows.Forms.Label();
            this.listBox_dump_meta = new System.Windows.Forms.ListBox();
            this.listBox_dump_data = new System.Windows.Forms.ListBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.listBox_dump_client);
            this.panel1.Controls.Add(this.listBox_clients);
            this.panel1.Controls.Add(this.label_clients);
            this.panel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.panel1.Location = new System.Drawing.Point(12, 245);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(364, 429);
            this.panel1.TabIndex = 0;
            // 
            // listBox_dump_client
            // 
            this.listBox_dump_client.FormattingEnabled = true;
            this.listBox_dump_client.Location = new System.Drawing.Point(3, 109);
            this.listBox_dump_client.Name = "listBox_dump_client";
            this.listBox_dump_client.Size = new System.Drawing.Size(356, 303);
            this.listBox_dump_client.TabIndex = 2;
            // 
            // listBox_clients
            // 
            this.listBox_clients.FormattingEnabled = true;
            this.listBox_clients.Location = new System.Drawing.Point(3, 34);
            this.listBox_clients.Name = "listBox_clients";
            this.listBox_clients.Size = new System.Drawing.Size(356, 69);
            this.listBox_clients.TabIndex = 1;
            this.listBox_clients.SelectedIndexChanged += new System.EventHandler(this.listBox_clients_SelectedIndexChanged);
            // 
            // label_clients
            // 
            this.label_clients.AutoSize = true;
            this.label_clients.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_clients.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.label_clients.Location = new System.Drawing.Point(-1, 0);
            this.label_clients.Name = "label_clients";
            this.label_clients.Size = new System.Drawing.Size(98, 31);
            this.label_clients.TabIndex = 0;
            this.label_clients.Text = "Clients";
            this.label_clients.Click += new System.EventHandler(this.label1_Click);
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.listBox_dump_meta);
            this.panel2.Controls.Add(this.listBox_metadata);
            this.panel2.Controls.Add(this.label_metadata);
            this.panel2.Location = new System.Drawing.Point(382, 245);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(358, 429);
            this.panel2.TabIndex = 1;
            // 
            // listBox_metadata
            // 
            this.listBox_metadata.FormattingEnabled = true;
            this.listBox_metadata.Location = new System.Drawing.Point(7, 34);
            this.listBox_metadata.Name = "listBox_metadata";
            this.listBox_metadata.Size = new System.Drawing.Size(346, 69);
            this.listBox_metadata.TabIndex = 2;
            // 
            // label_metadata
            // 
            this.label_metadata.AutoSize = true;
            this.label_metadata.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_metadata.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.label_metadata.Location = new System.Drawing.Point(3, 0);
            this.label_metadata.Name = "label_metadata";
            this.label_metadata.Size = new System.Drawing.Size(221, 31);
            this.label_metadata.TabIndex = 2;
            this.label_metadata.Text = "MetadataServers";
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.listBox_dump_data);
            this.panel3.Controls.Add(this.listBox_data);
            this.panel3.Controls.Add(this.label_data);
            this.panel3.Location = new System.Drawing.Point(746, 245);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(354, 429);
            this.panel3.TabIndex = 2;
            // 
            // listBox_data
            // 
            this.listBox_data.FormattingEnabled = true;
            this.listBox_data.Location = new System.Drawing.Point(9, 34);
            this.listBox_data.Name = "listBox_data";
            this.listBox_data.Size = new System.Drawing.Size(340, 69);
            this.listBox_data.TabIndex = 4;
            // 
            // label_data
            // 
            this.label_data.AutoSize = true;
            this.label_data.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_data.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.label_data.Location = new System.Drawing.Point(3, 0);
            this.label_data.Name = "label_data";
            this.label_data.Size = new System.Drawing.Size(166, 31);
            this.label_data.TabIndex = 3;
            this.label_data.Text = "DataServers";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.Control;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(9, 187);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(121, 31);
            this.button1.TabIndex = 11;
            this.button1.Text = "Dump Geral";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click_2);
            // 
            // panel4
            // 
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Controls.Add(this.button_runStep);
            this.panel4.Controls.Add(this.button1);
            this.panel4.Controls.Add(this.button2);
            this.panel4.Controls.Add(this.button_script);
            this.panel4.Controls.Add(this.button_run_all);
            this.panel4.Controls.Add(this.listBox_script_steps);
            this.panel4.Controls.Add(this.listBox_scripts);
            this.panel4.Controls.Add(this.label_scripts);
            this.panel4.Location = new System.Drawing.Point(12, 12);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1088, 227);
            this.panel4.TabIndex = 3;
            // 
            // button_runStep
            // 
            this.button_runStep.BackColor = System.Drawing.SystemColors.Control;
            this.button_runStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_runStep.ForeColor = System.Drawing.Color.Black;
            this.button_runStep.Location = new System.Drawing.Point(883, 187);
            this.button_runStep.Name = "button_runStep";
            this.button_runStep.Size = new System.Drawing.Size(94, 25);
            this.button_runStep.TabIndex = 12;
            this.button_runStep.Text = "Run Step";
            this.button_runStep.UseVisualStyleBackColor = false;
            this.button_runStep.Click += new System.EventHandler(this.button_runStep_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.SystemColors.Control;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.Location = new System.Drawing.Point(582, 187);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(94, 25);
            this.button2.TabIndex = 10;
            this.button2.Text = "Kill All";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button_script
            // 
            this.button_script.BackColor = System.Drawing.SystemColors.Control;
            this.button_script.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_script.ForeColor = System.Drawing.Color.Black;
            this.button_script.Location = new System.Drawing.Point(416, 187);
            this.button_script.Name = "button_script";
            this.button_script.Size = new System.Drawing.Size(160, 31);
            this.button_script.TabIndex = 6;
            this.button_script.Text = "Launch Script";
            this.button_script.UseVisualStyleBackColor = false;
            this.button_script.Click += new System.EventHandler(this.button_script_Click);
            // 
            // button_run_all
            // 
            this.button_run_all.BackColor = System.Drawing.SystemColors.Control;
            this.button_run_all.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_run_all.ForeColor = System.Drawing.Color.Black;
            this.button_run_all.Location = new System.Drawing.Point(983, 187);
            this.button_run_all.Name = "button_run_all";
            this.button_run_all.Size = new System.Drawing.Size(94, 25);
            this.button_run_all.TabIndex = 9;
            this.button_run_all.Text = "Run All";
            this.button_run_all.UseVisualStyleBackColor = false;
            this.button_run_all.Click += new System.EventHandler(this.button_run_all_Click);
            // 
            // listBox_script_steps
            // 
            this.listBox_script_steps.FormattingEnabled = true;
            this.listBox_script_steps.Location = new System.Drawing.Point(582, 34);
            this.listBox_script_steps.Name = "listBox_script_steps";
            this.listBox_script_steps.Size = new System.Drawing.Size(501, 147);
            this.listBox_script_steps.TabIndex = 8;
            this.listBox_script_steps.SelectedIndexChanged += new System.EventHandler(this.listBox_script_steps_SelectedIndexChanged);
            // 
            // listBox_scripts
            // 
            this.listBox_scripts.FormattingEnabled = true;
            this.listBox_scripts.Location = new System.Drawing.Point(9, 34);
            this.listBox_scripts.Name = "listBox_scripts";
            this.listBox_scripts.Size = new System.Drawing.Size(567, 147);
            this.listBox_scripts.TabIndex = 7;
            this.listBox_scripts.SelectedIndexChanged += new System.EventHandler(this.listBox_scripts_SelectedIndexChanged);
            // 
            // label_scripts
            // 
            this.label_scripts.AutoSize = true;
            this.label_scripts.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_scripts.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.label_scripts.Location = new System.Drawing.Point(-1, 0);
            this.label_scripts.Name = "label_scripts";
            this.label_scripts.Size = new System.Drawing.Size(98, 31);
            this.label_scripts.TabIndex = 3;
            this.label_scripts.Text = "Scripts";
            this.label_scripts.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // listBox_dump_meta
            // 
            this.listBox_dump_meta.FormattingEnabled = true;
            this.listBox_dump_meta.Location = new System.Drawing.Point(3, 109);
            this.listBox_dump_meta.Name = "listBox_dump_meta";
            this.listBox_dump_meta.Size = new System.Drawing.Size(350, 303);
            this.listBox_dump_meta.TabIndex = 3;
            // 
            // listBox_dump_data
            // 
            this.listBox_dump_data.FormattingEnabled = true;
            this.listBox_dump_data.Location = new System.Drawing.Point(3, 109);
            this.listBox_dump_data.Name = "listBox_dump_data";
            this.listBox_dump_data.Size = new System.Drawing.Size(346, 303);
            this.listBox_dump_data.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1102, 749);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "PADI-FS";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label_clients;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label_scripts;
        private System.Windows.Forms.Label label_metadata;
        private System.Windows.Forms.Label label_data;
        private System.Windows.Forms.Button button_script;
        private System.Windows.Forms.ListBox listBox_clients;
        private System.Windows.Forms.ListBox listBox_metadata;
        private System.Windows.Forms.ListBox listBox_data;
        private System.Windows.Forms.ListBox listBox_scripts;
        private System.Windows.Forms.Button button_run_all;
        private System.Windows.Forms.ListBox listBox_script_steps;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button_runStep;
        private System.Windows.Forms.ListBox listBox_dump_client;
        private System.Windows.Forms.ListBox listBox_dump_meta;
        private System.Windows.Forms.ListBox listBox_dump_data;
    }
}