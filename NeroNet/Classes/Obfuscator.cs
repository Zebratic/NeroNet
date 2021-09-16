using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeroNet.Classes
{
    public class Obfuscator
    {
        public bool ShouldRename(MethodDef m)
        {
            if (m.Name == "InitializeComponent")
            {
                return false;
            }

            string str = m.Name.ToLower();
            if (str != "<Module>" && str != ".ctor" && str != ".cctor" && str != "new" && str != "dispose" && str != "finalize" && !m.IsConstructor && !m.IsRuntime && !m.IsRuntimeSpecialName && !m.IsSpecialName && !m.IsVirtual && !m.IsAbstract && m.Overrides.Count <= 0 && !m.Name.StartsWith("<") && !m.IsPinvokeImpl)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ShouldRename(TypeDef c)
        {
            if (c.Name != "<Module>" && !c.IsRuntimeSpecialName && !c.IsSpecialName && !c.Name.Contains("Resources") && !c.Name.StartsWith("<") && !c.Name.Contains("__") && !c.IsEnum)
            {
                return true;
            }

            return false;
        }
        public static Random r = new Random();
        public AssemblyDef Rename(AssemblyDef assembly, int charactersetindex)
        {
            foreach (var i in assembly.Modules)
            {
                foreach (var i2 in i.Types)
                {
                    string ori = i2.Name;
                    if (ShouldRename(i2))
                    {
                        i2.Name = GetRandomisedName(GetCharacterSet(charactersetindex), r.Next(20, 50));
                        i2.Namespace = GetRandomisedName(GetCharacterSet(charactersetindex), r.Next(20, 30));
                    }

                    foreach (var r53 in i.Resources)
                    {
                        if (r53.Name.Contains(ori))
                        {
                            //r53.Name = i2.FullName + ".resources";
                        }
                    }

                    foreach (var i3 in i2.Methods)
                    {
                        if (ShouldRename(i3))
                        {
                            i3.Name = GetRandomisedName(GetCharacterSet(charactersetindex), r.Next(30, 50));
                        }
                    }
                }
            }

            return assembly;
        }

        public string[] GetCharacterSet(int charactersetindex)
        {
            switch (charactersetindex)
            {
                case 0:
                    return Chinese;
                case 1:
                    return Nazi;
                case 2:
                    return Joker;
                case 3:
                    return Haha;
                case 4:
                    return Blocks;
            }
            return Joker;
        }

        private static string[] Chinese = new[] { "女", "书", "标", "准", "字", "典" };
        private static string[] Nazi = new[] { "卐", "卍" };
        private static string[] Joker = new[] { "Ｊ", "Ｏ", "Ｋ", "Ｅ", "Ｒ", "ＪＯＫＥＲ" };
        private static string[] Haha = new[] { "Ｈ", "Ａ" };
        private static string[] Blocks = new[] { "▚", "▋" };
        public static string GetRandomisedName(string[] characterset, int length)
        {
            string f = "";
            for (int i = 0, loopTo = length - 1; i <= loopTo; i++)
                f += characterset[r.Next(0, characterset.Length)];

            return f;
        }

        public AssemblyDef FakeAttributes(AssemblyDef assembly)
        {
            foreach (var i in assembly.Modules)
            {
                i.Types.Add(new TypeDefUser("De4DotConfuser", "JokerObf"));
                i.Types.Add(new TypeDefUser("ConfusedByAttribute", "JokerObf"));
                i.Types.Add(new TypeDefUser("DotFuscator", "JokerObf"));
                i.Types.Add(new TypeDefUser("EazFuscator", "JokerObf"));
                i.Types.Add(new TypeDefUser("YanoAttribute", "JokerObf"));
                i.Types.Add(new TypeDefUser("Xenocode.Client.Attributes.AssemblyAttributes.ProcessedByXenocode", "JokerObf"));
                i.Types.Add(new TypeDefUser("PoweredByAttribute", "JokerObf"));
                i.Types.Add(new TypeDefUser("ObfuscatedByGoliath", "JokerObf"));
                i.Types.Add(new TypeDefUser("NineRays.Obfuscator.Evaluation", "ZYXDNGuarder"));
                i.Types.Add(new TypeDefUser("dotNetProtector", "DNGuard HVM"));
                i.Types.Add(new TypeDefUser("DotNetPatcherPackerAttribute", "Manco .NET Obfuscator"));
                i.Types.Add(new TypeDefUser("CryptoObfuscator.ProtectedWithCryptoObfuscatorAttribute", "Obfuscator attribute"));
                i.Types.Add(new TypeDefUser("BabelAttribute", "BabelObfuscatorAttribute"));
                i.Types.Add(new TypeDefUser("SecureTeam.Attributes.Obfuscated-By-CliSecure-Attribute", "BitHelmet Obfuscator"));
                i.Types.Add(new TypeDefUser("SecureTeam.Attributes.Obfuscated-By-Agile-Dot-Net-Attribute.", "YanoAttribute"));
                i.Types.Add(new TypeDefUser("Confuser", "Yano Obfuscator"));
            }

            return assembly;
        }
    }
}
