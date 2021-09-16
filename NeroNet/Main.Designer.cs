
namespace NeroNet
{
    partial class Main
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
            this.components = new System.ComponentModel.Container();
            this.OutPutLog = new System.Windows.Forms.RichTextBox();
            this.btnStartServer = new System.Windows.Forms.Button();
            this.btnStopServer = new System.Windows.Forms.Button();
            this.gpClientControls = new System.Windows.Forms.GroupBox();
            this.lblClientGPU = new System.Windows.Forms.Label();
            this.btnRemote = new System.Windows.Forms.Button();
            this.lbConnectedClients = new System.Windows.Forms.ListView();
            this.chConnectionId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chIP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnUpdate = new System.Windows.Forms.Button();
            this.lblClientRAM = new System.Windows.Forms.Label();
            this.lblClientDISK = new System.Windows.Forms.Label();
            this.lblClientCPU = new System.Windows.Forms.Label();
            this.btnKill = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.lblSettingsIP = new System.Windows.Forms.Label();
            this.gbSettings = new System.Windows.Forms.GroupBox();
            this.nudSettingsPort = new System.Windows.Forms.NumericUpDown();
            this.txtSettingsIP = new System.Windows.Forms.TextBox();
            this.cbRemoveInstallPrompt = new System.Windows.Forms.CheckBox();
            this.btnBuildClient = new System.Windows.Forms.Button();
            this.cbRunAtStartup = new System.Windows.Forms.CheckBox();
            this.lblSettingsPort = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.GetDataLoop = new System.Windows.Forms.Timer(this.components);
            this.txtSettingsURL = new System.Windows.Forms.TextBox();
            this.lblSettingsURL = new System.Windows.Forms.Label();
            this.gpClientControls.SuspendLayout();
            this.gbSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSettingsPort)).BeginInit();
            this.SuspendLayout();
            // 
            // OutPutLog
            // 
            this.OutPutLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OutPutLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.OutPutLog.Location = new System.Drawing.Point(12, 12);
            this.OutPutLog.Name = "OutPutLog";
            this.OutPutLog.Size = new System.Drawing.Size(771, 205);
            this.OutPutLog.TabIndex = 0;
            this.OutPutLog.Text = "";
            // 
            // btnStartServer
            // 
            this.btnStartServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnStartServer.Location = new System.Drawing.Point(12, 223);
            this.btnStartServer.Name = "btnStartServer";
            this.btnStartServer.Size = new System.Drawing.Size(75, 23);
            this.btnStartServer.TabIndex = 1;
            this.btnStartServer.Text = "Start Server";
            this.btnStartServer.UseVisualStyleBackColor = true;
            this.btnStartServer.Click += new System.EventHandler(this.btnStartServer_Click);
            // 
            // btnStopServer
            // 
            this.btnStopServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnStopServer.Location = new System.Drawing.Point(12, 252);
            this.btnStopServer.Name = "btnStopServer";
            this.btnStopServer.Size = new System.Drawing.Size(75, 23);
            this.btnStopServer.TabIndex = 2;
            this.btnStopServer.Text = "Stop Server";
            this.btnStopServer.UseVisualStyleBackColor = true;
            this.btnStopServer.Click += new System.EventHandler(this.btnStopServer_Click);
            // 
            // gpClientControls
            // 
            this.gpClientControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gpClientControls.Controls.Add(this.lblClientGPU);
            this.gpClientControls.Controls.Add(this.btnRemote);
            this.gpClientControls.Controls.Add(this.lbConnectedClients);
            this.gpClientControls.Controls.Add(this.btnUpdate);
            this.gpClientControls.Controls.Add(this.lblClientRAM);
            this.gpClientControls.Controls.Add(this.lblClientDISK);
            this.gpClientControls.Controls.Add(this.lblClientCPU);
            this.gpClientControls.Controls.Add(this.btnKill);
            this.gpClientControls.Controls.Add(this.btnDisconnect);
            this.gpClientControls.Location = new System.Drawing.Point(281, 223);
            this.gpClientControls.Name = "gpClientControls";
            this.gpClientControls.Size = new System.Drawing.Size(502, 159);
            this.gpClientControls.TabIndex = 5;
            this.gpClientControls.TabStop = false;
            this.gpClientControls.Text = "Client Control";
            // 
            // lblClientGPU
            // 
            this.lblClientGPU.AutoSize = true;
            this.lblClientGPU.Location = new System.Drawing.Point(9, 137);
            this.lblClientGPU.Name = "lblClientGPU";
            this.lblClientGPU.Size = new System.Drawing.Size(50, 13);
            this.lblClientGPU.TabIndex = 16;
            this.lblClientGPU.Text = "GPU: ?%";
            // 
            // btnRemote
            // 
            this.btnRemote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemote.Location = new System.Drawing.Point(198, 127);
            this.btnRemote.Name = "btnRemote";
            this.btnRemote.Size = new System.Drawing.Size(70, 23);
            this.btnRemote.TabIndex = 15;
            this.btnRemote.Text = "Remote";
            this.btnRemote.UseVisualStyleBackColor = true;
            this.btnRemote.Click += new System.EventHandler(this.btnRemote_Click);
            // 
            // lbConnectedClients
            // 
            this.lbConnectedClients.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lbConnectedClients.BackColor = System.Drawing.Color.White;
            this.lbConnectedClients.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbConnectedClients.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chConnectionId,
            this.chIP});
            this.lbConnectedClients.ForeColor = System.Drawing.Color.Black;
            this.lbConnectedClients.FullRowSelect = true;
            this.lbConnectedClients.HideSelection = false;
            this.lbConnectedClients.Location = new System.Drawing.Point(12, 19);
            this.lbConnectedClients.Name = "lbConnectedClients";
            this.lbConnectedClients.Size = new System.Drawing.Size(489, 94);
            this.lbConnectedClients.TabIndex = 14;
            this.lbConnectedClients.UseCompatibleStateImageBehavior = false;
            this.lbConnectedClients.View = System.Windows.Forms.View.Details;
            this.lbConnectedClients.SelectedIndexChanged += new System.EventHandler(this.lbConnectedClients_SelectedIndexChanged);
            // 
            // chConnectionId
            // 
            this.chConnectionId.Text = "ID";
            this.chConnectionId.Width = 126;
            // 
            // chIP
            // 
            this.chIP.Text = "Public IP";
            this.chIP.Width = 363;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdate.Location = new System.Drawing.Point(274, 127);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(70, 23);
            this.btnUpdate.TabIndex = 10;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // lblClientRAM
            // 
            this.lblClientRAM.AutoSize = true;
            this.lblClientRAM.Location = new System.Drawing.Point(108, 119);
            this.lblClientRAM.Name = "lblClientRAM";
            this.lblClientRAM.Size = new System.Drawing.Size(51, 13);
            this.lblClientRAM.TabIndex = 9;
            this.lblClientRAM.Text = "RAM: ?%";
            // 
            // lblClientDISK
            // 
            this.lblClientDISK.AutoSize = true;
            this.lblClientDISK.Location = new System.Drawing.Point(107, 137);
            this.lblClientDISK.Name = "lblClientDISK";
            this.lblClientDISK.Size = new System.Drawing.Size(52, 13);
            this.lblClientDISK.TabIndex = 8;
            this.lblClientDISK.Text = "DISK: ?%";
            // 
            // lblClientCPU
            // 
            this.lblClientCPU.AutoSize = true;
            this.lblClientCPU.Location = new System.Drawing.Point(9, 119);
            this.lblClientCPU.Name = "lblClientCPU";
            this.lblClientCPU.Size = new System.Drawing.Size(49, 13);
            this.lblClientCPU.TabIndex = 7;
            this.lblClientCPU.Text = "CPU: ?%";
            // 
            // btnKill
            // 
            this.btnKill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnKill.Location = new System.Drawing.Point(426, 127);
            this.btnKill.Name = "btnKill";
            this.btnKill.Size = new System.Drawing.Size(70, 23);
            this.btnKill.TabIndex = 6;
            this.btnKill.Text = "Kill";
            this.btnKill.UseVisualStyleBackColor = true;
            this.btnKill.Click += new System.EventHandler(this.btnKill_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDisconnect.Location = new System.Drawing.Point(350, 127);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(70, 23);
            this.btnDisconnect.TabIndex = 5;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // lblSettingsIP
            // 
            this.lblSettingsIP.AutoSize = true;
            this.lblSettingsIP.Location = new System.Drawing.Point(6, 22);
            this.lblSettingsIP.Name = "lblSettingsIP";
            this.lblSettingsIP.Size = new System.Drawing.Size(20, 13);
            this.lblSettingsIP.TabIndex = 11;
            this.lblSettingsIP.Text = "IP:";
            // 
            // gbSettings
            // 
            this.gbSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.gbSettings.Controls.Add(this.lblSettingsURL);
            this.gbSettings.Controls.Add(this.txtSettingsURL);
            this.gbSettings.Controls.Add(this.nudSettingsPort);
            this.gbSettings.Controls.Add(this.txtSettingsIP);
            this.gbSettings.Controls.Add(this.cbRemoveInstallPrompt);
            this.gbSettings.Controls.Add(this.btnBuildClient);
            this.gbSettings.Controls.Add(this.cbRunAtStartup);
            this.gbSettings.Controls.Add(this.lblSettingsPort);
            this.gbSettings.Controls.Add(this.lblSettingsIP);
            this.gbSettings.Location = new System.Drawing.Point(93, 223);
            this.gbSettings.Name = "gbSettings";
            this.gbSettings.Size = new System.Drawing.Size(182, 159);
            this.gbSettings.TabIndex = 12;
            this.gbSettings.TabStop = false;
            this.gbSettings.Text = "Settings";
            // 
            // nudSettingsPort
            // 
            this.nudSettingsPort.Location = new System.Drawing.Point(35, 45);
            this.nudSettingsPort.Name = "nudSettingsPort";
            this.nudSettingsPort.Size = new System.Drawing.Size(141, 20);
            this.nudSettingsPort.TabIndex = 13;
            // 
            // txtSettingsIP
            // 
            this.txtSettingsIP.Location = new System.Drawing.Point(35, 19);
            this.txtSettingsIP.Name = "txtSettingsIP";
            this.txtSettingsIP.Size = new System.Drawing.Size(141, 20);
            this.txtSettingsIP.TabIndex = 18;
            this.txtSettingsIP.TextChanged += new System.EventHandler(this.txtSettingsIP_TextChanged);
            // 
            // cbRemoveInstallPrompt
            // 
            this.cbRemoveInstallPrompt.AutoSize = true;
            this.cbRemoveInstallPrompt.Location = new System.Drawing.Point(10, 112);
            this.cbRemoveInstallPrompt.Name = "cbRemoveInstallPrompt";
            this.cbRemoveInstallPrompt.Size = new System.Drawing.Size(155, 17);
            this.cbRemoveInstallPrompt.TabIndex = 17;
            this.cbRemoveInstallPrompt.Text = "Remove Installation Prompt";
            this.cbRemoveInstallPrompt.UseVisualStyleBackColor = true;
            // 
            // btnBuildClient
            // 
            this.btnBuildClient.Location = new System.Drawing.Point(9, 130);
            this.btnBuildClient.Name = "btnBuildClient";
            this.btnBuildClient.Size = new System.Drawing.Size(167, 23);
            this.btnBuildClient.TabIndex = 16;
            this.btnBuildClient.Text = "Build Client";
            this.btnBuildClient.UseVisualStyleBackColor = true;
            this.btnBuildClient.Click += new System.EventHandler(this.btnBuildClient_Click);
            // 
            // cbRunAtStartup
            // 
            this.cbRunAtStartup.AutoSize = true;
            this.cbRunAtStartup.Location = new System.Drawing.Point(10, 94);
            this.cbRunAtStartup.Name = "cbRunAtStartup";
            this.cbRunAtStartup.Size = new System.Drawing.Size(125, 17);
            this.cbRunAtStartup.TabIndex = 13;
            this.cbRunAtStartup.Text = "Client Run At Startup";
            this.cbRunAtStartup.UseVisualStyleBackColor = true;
            // 
            // lblSettingsPort
            // 
            this.lblSettingsPort.AutoSize = true;
            this.lblSettingsPort.Location = new System.Drawing.Point(4, 47);
            this.lblSettingsPort.Name = "lblSettingsPort";
            this.lblSettingsPort.Size = new System.Drawing.Size(29, 13);
            this.lblSettingsPort.TabIndex = 12;
            this.lblSettingsPort.Text = "Port:";
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(9, 369);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(37, 13);
            this.lblStatus.TabIndex = 14;
            this.lblStatus.Text = "Offline";
            // 
            // GetDataLoop
            // 
            this.GetDataLoop.Interval = 200;
            this.GetDataLoop.Tick += new System.EventHandler(this.GetDataLoop_Tick);
            // 
            // txtSettingsURL
            // 
            this.txtSettingsURL.Location = new System.Drawing.Point(35, 71);
            this.txtSettingsURL.Name = "txtSettingsURL";
            this.txtSettingsURL.Size = new System.Drawing.Size(141, 20);
            this.txtSettingsURL.TabIndex = 19;
            this.txtSettingsURL.TextChanged += new System.EventHandler(this.txtSettingsURL_TextChanged);
            // 
            // lblSettingsURL
            // 
            this.lblSettingsURL.AutoSize = true;
            this.lblSettingsURL.Location = new System.Drawing.Point(3, 74);
            this.lblSettingsURL.Name = "lblSettingsURL";
            this.lblSettingsURL.Size = new System.Drawing.Size(32, 13);
            this.lblSettingsURL.TabIndex = 20;
            this.lblSettingsURL.Text = "URL:";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(795, 394);
            this.Controls.Add(this.gbSettings);
            this.Controls.Add(this.gpClientControls);
            this.Controls.Add(this.btnStopServer);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnStartServer);
            this.Controls.Add(this.OutPutLog);
            this.MinimumSize = new System.Drawing.Size(811, 218);
            this.Name = "Main";
            this.Text = "Server";
            this.Load += new System.EventHandler(this.Server_Load);
            this.gpClientControls.ResumeLayout(false);
            this.gpClientControls.PerformLayout();
            this.gbSettings.ResumeLayout(false);
            this.gbSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSettingsPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox OutPutLog;
        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.Button btnStopServer;
        private System.Windows.Forms.GroupBox gpClientControls;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Button btnKill;
        private System.Windows.Forms.Label lblClientRAM;
        private System.Windows.Forms.Label lblClientDISK;
        private System.Windows.Forms.Label lblClientCPU;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Label lblSettingsIP;
        private System.Windows.Forms.GroupBox gbSettings;
        private System.Windows.Forms.Label lblSettingsPort;
        private System.Windows.Forms.NumericUpDown nudSettingsPort;
        private System.Windows.Forms.Timer GetDataLoop;
        private System.Windows.Forms.ListView lbConnectedClients;
        private System.Windows.Forms.ColumnHeader chConnectionId;
        private System.Windows.Forms.ColumnHeader chIP;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnRemote;
        private System.Windows.Forms.Button btnBuildClient;
        private System.Windows.Forms.Label lblClientGPU;
        private System.Windows.Forms.CheckBox cbRunAtStartup;
        private System.Windows.Forms.CheckBox cbRemoveInstallPrompt;
        private System.Windows.Forms.TextBox txtSettingsIP;
        private System.Windows.Forms.Label lblSettingsURL;
        private System.Windows.Forms.TextBox txtSettingsURL;
    }
}

