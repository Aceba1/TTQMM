using Harmony;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine.UI;
using UnityEngine;

namespace QModInstaller
{
    public class QModPatcher
    {
        public static void Patch()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var allDlls = new DirectoryInfo(QModBaseDir).GetFiles("*.dll", SearchOption.AllDirectories);
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

            if (!Directory.Exists(QModBaseDir))
            {
                AddLog("QMods directory was not found! Creating...");
                if (QModBaseDir == "ERR")
                {
                    AddLog("There was an error creating the QMods directory");
                    AddLog("Please make sure that you ran TerraTech from Steam");
                }
                try
                {
                    Directory.CreateDirectory(QModBaseDir);
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

            var subDirs = Directory.GetDirectories(QModBaseDir);
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

            // Enable Stopwatch just before loading mods.
            if (sw == null)
                sw = new Stopwatch();

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

            if (sw.IsRunning)
                sw.Stop();

            var mods = firstMods;
            mods.AddRange(otherMods);
            mods.AddRange(lastMods);
            mods.Sort();

            FlagGame();

            AddLog(" ");
            AddLog("Installed mods:");

            foreach (var mod in mods)
            {
                bool success = elapsedTimes.TryGetValue(mod, out string elapsed);
                if (success)
                    AddLog($"- {mod.DisplayName} ({mod.Id}) - {elapsed}");
                else
                    AddLog($"- {mod.DisplayName} ({mod.Id}) - Unkown load time");
                // - Internal Flagging (qmm.internal.modflag)
            }

            Console.WriteLine(ParseLog());
        }

        internal static string QModBaseDir = Directory.GetCurrentDirectory().Contains("system32") ? "ERR" : Directory.GetCurrentDirectory() + @"\QMods";

        internal static List<QMod> loadedMods = new List<QMod>();

        internal static bool patched = false;

        internal static Stopwatch sw = null;

        internal static Version version = new Version(1, 3);

        internal static string GetModsLine()
            => $"This game is modded! Using QModManager {version}, amount of mods loaded: {loadedMods.Count} (Check the output log for a complete list of installed mods and respective their load times)";

        internal static void FlagGame()
        {
            if (sw.IsRunning)
                sw.Stop();
            sw.Reset();
            sw.Start();

            string Id = "ttqmm.internal.modflag";
            string Name = "Internal Flagging";
            HarmonyInstance.Create(Id).PatchAll(Assembly.GetExecutingAssembly());

            sw.Stop();

            AddLog("Internal stuff:");
            AddLog($"- {Name} ({Id}) - {ParseTime(sw)}");
        }

        internal static Dictionary<QMod, string> elapsedTimes = new Dictionary<QMod, string>();

        internal static QMod LoadMod(QMod mod)
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
                    if (sw.IsRunning)
                        sw.Stop();
                    sw.Reset();
                    sw.Start();

                    var entryMethodSig = mod.EntryMethod.Split('.');
                    var entryType = String.Join(".", entryMethodSig.Take(entryMethodSig.Length - 1).ToArray());
                    var entryMethod = entryMethodSig[entryMethodSig.Length - 1];

                    MethodInfo qPatchMethod = mod.loadedAssembly.GetType(entryType).GetMethod(entryMethod);
                    qPatchMethod.Invoke(mod.loadedAssembly, new object[] { });

                    sw.Stop();

                    string elapsedTime = ParseTime(sw);

                    elapsedTimes.Add(mod, elapsedTime);
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

        internal static string ParseTime(Stopwatch sw)
        {
            string elapsedTime = "";
            if (sw.Elapsed.Hours == 0)
                if (sw.Elapsed.Minutes == 0)
                    if (sw.Elapsed.Seconds == 0)
                        if (sw.Elapsed.Milliseconds == 0)
                            return "Loaded immediately";
                        else
                            elapsedTime = String.Format("{0:00}ms", sw.Elapsed.Milliseconds / 10);
                    else
                        elapsedTime = String.Format("{0:00}s{1:00}ms", sw.Elapsed.Seconds, sw.Elapsed.Milliseconds / 10);
                else
                    elapsedTime = String.Format("{0:00}m{1:00}s{2:00}ms", sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.Milliseconds / 10);
            else
                elapsedTime = String.Format("{0:00}h{1:00}m{2:00}s{3:00}ms", sw.Elapsed.Hours, sw.Elapsed.Minutes, sw.Elapsed.Seconds, sw.Elapsed.Milliseconds / 10);
            return "Loaded in " + elapsedTime;
        }

        internal static List<string> rawLines = new List<string>();

        internal static void AddLog(string line)
        {
            rawLines.Add(line);
        }

        internal static string ParseLog()
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

        internal static string Blank(int maxLength)
        {
            string output = "";
            output += "#  ";
            for (int i = " ".Length; i < maxLength; i++)
                output += " ";
            output += " #\n";
            return output;
        }
    }

    internal class Patches
    {
        internal static class MaxPriority
        {
            internal const int First = int.MaxValue;
            internal const int Last = int.MinValue;
        }

        [HarmonyPatch(typeof(UIScreenBugReport))]
        [HarmonyPatch("Post")]
        internal static class UIScreenBugReport_Post
        {
            [HarmonyPrefix]
            [HarmonyPriority(MaxPriority.Last)]
            internal static void Prefix(UIScreenBugReport __instance)
            {
                Text m_Body = __instance.GetInstanceField("m_Body") as Text;
                m_Body.text = QModPatcher.GetModsLine() + "\n\n" + m_Body.text;
                __instance.SetInstanceField("m_Body", m_Body);
            }
        }

        [HarmonyPatch]
        internal static class UIScreenBugReport_PostIt
        {
            public static MethodInfo TargetMethod()
                => AccessTools.FirstInner(typeof(UIScreenBugReport), (type) => type.Name.Contains("<PostIt>")).GetMethod("MoveNext", AccessTools.all);

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var codes = new List<CodeInstruction>(instructions);
                var codesToInsert = new List<CodeInstruction>();

                int platformStringIndex = -1;
                int callIndex = -1;
                int ldarg0Index = -1;

                // Find where the string "platform" is defined (since it's the last one in the AddField calls)
                if (InstructionsHelper.TryFindInstruction(codes, 0, OpCodes.Ldstr, "platform", out platformStringIndex) &&
                    // First found Callvirt will be AddField
                    InstructionsHelper.TryFindInstruction(codes, platformStringIndex, OpCodes.Callvirt, null, out callIndex) &&
                    // Find where this is put onto the IL Stack (since it calls this.{reference}.AddField())
                    InstructionsHelper.TryFindInstructionBefore(codes, platformStringIndex, OpCodes.Ldarg_0, null, out ldarg0Index))
                {
                    // Loop from ldarg.0 till one before ldstr "platform"
                    for (int i = ldarg0Index; i < platformStringIndex; i++)
                    {
                        //Console.WriteLine($"{i}: {codes[i].opcode} : {codes[i].operand}");
                        codesToInsert.Add(new CodeInstruction(codes[i]));
                    }

                    // Add the string "mods"
                    codesToInsert.Add(new CodeInstruction(codes[platformStringIndex])
                    {
                        operand = "mods"
                    });

                    // Add the call to GetModsLine
                    var getModsLineMethod = AccessTools.Method(typeof(QModPatcher), nameof(QModPatcher.GetModsLine));
                    codesToInsert.Add(new CodeInstruction(OpCodes.Call, getModsLineMethod));

                    // Add the Callvirt
                    codesToInsert.Add(new CodeInstruction(codes[callIndex]));

                    codes.InsertRange(callIndex, codesToInsert);
                }
                else
                {
                    QModPatcher.AddLog($"Couldn't find it | DEBUG | platformStringIndex: {platformStringIndex} | callIndex: {callIndex} | ldargs0Index: {ldarg0Index}");
                }

                return codes;
            }
        }

        internal static class InstructionsHelper
        {
            public static bool TryFindInstruction(List<CodeInstruction> codes, int startIndex, OpCode targetOpCode, object targetObject, out int targetFoundAt, int skipAmount = 0)
            {
                for (int i = startIndex; i < codes.Count; i++)
                {
                    if (codes[i].opcode == targetOpCode && (targetObject == null || (codes[i].operand != null && codes[i].operand.Equals(targetObject))))
                    {
                        if (skipAmount < 1)
                        {
                            targetFoundAt = i;
                            return true;
                        }
                        else
                            skipAmount--;
                    }
                }

                targetFoundAt = -1;
                return false;
            }

            public static bool TryFindInstructionBefore(List<CodeInstruction> codes, int startIndex, OpCode targetOpCode, object targetObject, out int targetFoundAt, int skipAmount = 0)
            {
                for (int i = startIndex; i >= 0; i--)
                {
                    if (codes[i].opcode == targetOpCode && (targetObject == null || (codes[i].operand != null && codes[i].operand.Equals(targetObject))))
                    {
                        if (skipAmount < 1)
                        {
                            targetFoundAt = i;
                            return true;
                        }
                        else
                            skipAmount--;
                    }
                }

                targetFoundAt = -1;
                return false;
            }
        }
    }
}