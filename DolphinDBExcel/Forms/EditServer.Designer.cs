using DolphinDBForExcel.Ribbon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Serialization;
using static DolphinDBForExcel.ServerInfosXmlSerializer;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DolphinDBForExcel.Forms
{
    partial class EditServer : Form
    {
        public class ServerInfoString
        {
            public ServerInfoString(ServerInfo serverInfo)
            {
                Name = serverInfo.Name;
                Host = serverInfo.Host;
                Port = serverInfo.Port.ToString();
                Username = serverInfo.Username;
                Password = serverInfo.Password;
            }


            public string Name { get; set; }
            public string Host { get; set; }
            public string Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }


            /// <summary>
            /// Required designer variable.
            /// </summary>
        private System.ComponentModel.IContainer components = null;
        private DataGridView dataGridView;


        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                e.Value = "******";
            }
        }

        public EditServer()
        {
            InitializeComponent();
            this.Text = "Server Editor";
            BindingList<ServerInfoString> serverInfoList = new BindingList<ServerInfoString>();
            List<ServerInfo> servers = ConnectionController.Instance.LoadServerInfos();
            foreach (ServerInfo serverInfo in servers)
            {
                serverInfoList.Add(new ServerInfoString(serverInfo));
            }
            this.dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 11);
            this.dataGridView.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView.RowsDefaultCellStyle.Font = new Font("宋体", 11);


            dataGridView.Dock = DockStyle.Fill;
            dataGridView.DataSource = serverInfoList;
            this.dataGridView.CellFormatting += dataGridView1_CellFormatting;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            AddinRibbon.RibbonController.Invalidate();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }


        private void ok_button_Click(object sender, EventArgs e)
        {
            bool failed = false;
            try
            {
                int size = dataGridView.RowCount;
                List<ServerInfo> servers = new List<ServerInfo>();
                for (int i = 0; i < size; ++i)
                {
                    ServerInfoString selectedServerInfo = (ServerInfoString)dataGridView.Rows[i].DataBoundItem;
                    if (selectedServerInfo == null) break;
                    ServerInfo serverInfo = new ServerInfo();
                    int port;
                    if (!int.TryParse(selectedServerInfo.Port, out port))
                    {
                        throw new Exception("Incorrect port.");
                    }   
                    serverInfo.Port = port;
                    serverInfo.Host = selectedServerInfo.Host;
                    serverInfo.Name = selectedServerInfo.Name;
                    serverInfo.Username = selectedServerInfo.Username;
                    serverInfo.Password = selectedServerInfo.Password;
                    servers.Add(serverInfo);
                }
                ConnectionController.Instance.SaveServerInfos(servers);
            }catch(Exception ex)
            {
                failed = true;
                System.Windows.MessageBox.Show("Failed to save server :" + ex.Message);
            }
            if (!failed)
            {
                this.Close();
            }
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            Close();
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.save = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView.Location = new System.Drawing.Point(0, 0);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.Size = new System.Drawing.Size(588, 280);
            this.dataGridView.TabIndex = 0;
            this.dataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellContentClick);
            // 
            // save
            // 
            this.save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.save.Location = new System.Drawing.Point(384, 245);
            this.save.Name = "OK";
            this.save.Size = new System.Drawing.Size(75, 23);
            this.save.TabIndex = 2;
            this.save.Text = "OK";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.ok_button_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(481, 245);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(588, 280);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.save);
            this.Controls.Add(this.dataGridView);
            this.Name = "EditServer";
            this.Text = "EditServer";
            this.Load += new System.EventHandler(this.EditServer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        private void DataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dataGridView.CurrentCell.ColumnIndex == 2 ) // yourPasswordColumnIndex 是你希望设置为密码列的列的索引  
            {
                this.dataGridView.CurrentCell.Value = "*";
            }
        }

        #endregion

        private System.Windows.Forms.Button save;
        private System.Windows.Forms.Button button1;
    }
}