using Harmony;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace QModInstaller
{
    public class QModPatcher
    {
        private static string qModBaseDir = Environment.CurrentDirectory + @"\QMods";
        private static List<QMod> loadedMods = new List<QMod>();
        private static bool patched = false;

        public static Version version = new Version(1, 3);

        public static void Patch()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var allDlls = new DirectoryInfo(qModBaseDir).GetFiles("*.dll", SearchOption.AllDirectories);
                foreach (var dll in allDlls)
                {
                    Console.WriteLine(Path.GetFileNameWithoutExtension(dll.Name) + " " + args.Name);
                    if (args.Name.Contains(Path.GetFileNameWithoutExtension(dll.Name)))
                    {
                        return Assembly.LoadFrom(dll.FullName);
                    }
                }

                return null;
            };

            if (patched) return;

            patched = true;

            AddLog($"This game is modded! (QModManager {version})");

            if (!Directory.Exists(qModBaseDir))
            {
                AddLog("QMods directory was not found! Creating...");
                try
                {
                    Directory.CreateDirectory(qModBaseDir);
                    AddLog("QMods directory created successfully!");
                }
                catch (Exception e)
                {
                    AddLog("EXCEPTION CAUGHT!");
                    AddLog(e.Message);
                    AddLog(e.StackTrace);
                    if (e.InnerException != null)
                    {
                        AddLog("INNER EXCEPTION:");
                        AddLog(e.InnerException.Message);
                        AddLog(e.InnerException.StackTrace);
                    }
                }
                Console.WriteLine(ParseLog());
                return;
            }

            var subDirs = Directory.GetDirectories(qModBaseDir);
            var lastMods = new List<QMod>();
            var firstMods = new List<QMod>();
            var otherMods = new List<QMod>();

            foreach (var subDir in subDirs)
            {
                var jsonFile = Path.Combine(subDir, "mod.json");

                if (!File.Exists(jsonFile))
                {
                    AddLog($"ERROR! No \"mod.json\" file found in folder \"{subDir}\"");
                    File.WriteAllText(jsonFile, JsonConvert.SerializeObject(new QMod()));
                    AddLog("A template file was created");
                    continue;
                }

                QMod mod = QMod.FromJsonFile(Path.Combine(subDir, "mod.json"));

                if (mod == (null))
                    continue;

                if (mod.Enable == false)
                {
                    AddLog($"{mod.DisplayName} is disabled via config, skipping");
                    continue;
                }

                var modAssemblyPath = Path.Combine(subDir, mod.AssemblyName);

                if (!File.Exists(modAssemblyPath))
                {
                    AddLog($"ERROR! No matching dll found at \"{modAssemblyPath}\" for mod \"{mod.DisplayName}\"");
                    continue;
                }

                mod.loadedAssembly = Assembly.LoadFrom(modAssemblyPath);
                mod.modAssemblyPath = modAssemblyPath;

                if (mod.Priority.Equals("Last"))
                {
                    lastMods.Add(mod);
                    continue;
                }
                else if (mod.Priority.Equals("First"))
                {
                    firstMods.Add(mod);
                    continue;
                }
                else
                {
                    otherMods.Add(mod);
                    continue;
                }
            }

            foreach (var mod in firstMods)
            {
                if (mod != null)
                    loadedMods.Add(LoadMod(mod));
            }

            foreach (var mod in otherMods)
            {
                if (mod != null)
                    loadedMods.Add(LoadMod(mod));
            }

            foreach (var mod in lastMods)
            {
                if (mod != null)
                    loadedMods.Add(LoadMod(mod));
            }

            var mods = firstMods;
            mods.AddRange(otherMods);
            mods.AddRange(lastMods);
            mods.Sort();

            var modNames = mods.Select(mod => new { mod.Id, mod.DisplayName });

            AddLog("Installed mods:");
            foreach (var mod in modNames)
                AddLog("- " + mod.DisplayName + " (" + mod.Id + ")");

            FlagGame();
            Console.WriteLine(ParseLog());
        }

        public static void FlagGame()
        {
            string Id = "qmm.internal.modflag";
            string Name = "Internal Flagging";
            HarmonyInstance.Create(Id).PatchAll(Assembly.GetExecutingAssembly());
            AddLog("- " + Name + " (" + Id + ")");
        }

        private static QMod LoadMod(QMod mod)
        {
            if (mod == null) return null;

            if (string.IsNullOrEmpty(mod.EntryMethod))
            {
                AddLog($"ERROR! No EntryMethod specified for mod {mod.DisplayName}");
            }
            else
            {
                try
                {
                    var entryMethodSig = mod.EntryMethod.Split('.');
                    var entryType = String.Join(".", entryMethodSig.Take(entryMethodSig.Length - 1).ToArray());
                    var entryMethod = entryMethodSig[entryMethodSig.Length - 1];

                    MethodInfo qPatchMethod = mod.loadedAssembly.GetType(entryType).GetMethod(entryMethod);
                    qPatchMethod.Invoke(mod.loadedAssembly, new object[] { });
                }
                catch (ArgumentNullException e)
                {
                    AddLog($"ERROR! Could not parse entry method {mod.AssemblyName} for mod {mod.DisplayName}");
                    if (e.InnerException != null)
                    {
                        AddLog(e.InnerException.Message);
                        AddLog(e.InnerException.StackTrace);
                    }
                    return null;
                }
                catch (TargetInvocationException e)
                {
                    AddLog($"ERROR! Invoking the specified entry method {mod.EntryMethod} failed for mod {mod.Id}");
                    if (e.InnerException != null)
                    {
                        AddLog(e.InnerException.Message);
                        AddLog(e.InnerException.StackTrace);
                    }
                    return null;
                }
                catch (Exception e)
                {
                    AddLog("ERROR! An unexpected error occurred!");
                    AddLog(e.Message);
                    AddLog(e.StackTrace);
                    if (e.InnerException != null)
                    {
                        AddLog(e.InnerException.Message);
                        AddLog(e.InnerException.StackTrace);
                    }
                    return null;
                }
            }

            return mod;
        }

        private static List<string> rawLines = new List<string>();

        private static void AddLog(string line)
        {
            rawLines.Add(line);
        }

        private static string ParseLog()
        {
            string output = "";
            int maxLength = 0;
            foreach (string line in rawLines)
            {
                if (line.Length > maxLength)
                    maxLength = line.Length;
            }
            string separator = "";
            string title = $" QMODMANAGER {version} ";
            if (maxLength + 4 < title.Length) maxLength = title.Length;
            int spacingLength = maxLength + 4 - title.Length;
            for (int i = maxLength + 3; i >= 0; i--)
            {
                separator += "#";
            }
            output += separator + "\n";

            int length = (spacingLength >> 1) + (spacingLength & 1); // Gets the roofed half using bitwise operators:
            //Shift bits right by 1 (which divides by 2, floored)
            //And add the first bit of the value. (1 or 0 if it is odd or even)

            string toAdd = "";
            for (int i = length - 1; i > 0; i--)
            {
                toAdd += "#";
            }
            output += toAdd + ((spacingLength & 1) == 0 ? "#" : "") + title + toAdd + "#\n";
            output += separator + "\n";
            output += Blank(maxLength);
            foreach (string line in rawLines)
            {
                output += "# " + line;
                for (int i = line.Length; i < maxLength; i++)
                    output += " ";
                output += " #\n";
            }
            output += Blank(maxLength);
            output += separator;
            return output;
        }

        public static string Blank(int maxLength)
        {
            string output = "";
            output += "#  ";
            for (int i = " ".Length; i < maxLength; i++)
                output += " ";
            output += " #\n";
            return output;
        }
    }
    class Patches
    {
        [HarmonyPatch(typeof(UIScreenBugReport))]
        [HarmonyPatch("Post")]
        class UIScreenBugReport_PostIt
        {
            static void Prefix(UIScreenBugReport __instance)
            {
                UnityEngine.Debug.Log("Bug report UI screen sending!");
                var m_Body = Traverse.Create(__instance).Field("m_Body");
                var Text = m_Body.Property("text");
                var text = m_Body.Field("m_Text");
                string value = text.GetValue<string>();
                if (!value.ToLower().Contains("this game is modded"))
                {
                    value += "This game is modded\n\n";
                    UnityEngine.Debug.Log("Couldn't find modded flag! Adding");
                    Text.SetValue(value);
                    text.SetValue(value);
                    Text.SetValue(value);
                    text.SetValue(value);
                }
            }
        }
    }
}