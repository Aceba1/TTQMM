using Harmony;
using Newtonsoft.Json;
using QModManager.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine.UI;

namespace QModManager
{
    public class QModPatcher
    {
        #region Patching

        internal class QMod
        {
            public string Id = "Mod.ID";

            public string DisplayName = "Display name";

            public string Author = "Author name";

            public string Version = "0.0.0";

            //public string[] Requires = new string[] { };

            public bool Enable = true;

            public string AssemblyName = "DLL Filename";

            public string EntryMethod = "Namespace.Class.Method";

            public string Priority = "First or Last";

            //public Dictionary<string, object> Config = new Dictionary<string, object>();

            [JsonIgnore]
            public Assembly LoadedAssembly;

            [JsonIgnore]
            public string ModAssemblyPath;

            public string[] LoadThisModAfter = new string[] { };

            public string[] LoadThisModBefore = new string[] { };

            [JsonIgnore]
            public string[] LoadAfterOtherMods = new string[] { }; // Might not be needed

            [JsonIgnore]
            public string[] LoadBeforeOtherMods = new string[] { }; // Might not be needed

            //public QMod() { }

            public static QMod FromJsonFile(string file)
            {
                try
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    string json = File.ReadAllText(file);
                    QMod mod = JsonConvert.DeserializeObject<QMod>(json);

                    return mod;
                }
                catch (Exception e)
                {
                    AddLog("ERROR! mod.json deserialization failed!");
                    AddLog(e.Message);
                    AddLog(e.StackTrace);

                    return null;
                }
            }
        }

        /// <summary>
        /// Public method which is called by the game to load the mods
        /// </summary>
        public static void Patch()
        {
            try
            {
                LoadMods();
            }
            catch (Exception e)
            {
                AddLog("EXCEPTION CAUGHT!");
                AddLog(e.Message);
                AddLog(e.StackTrace);
                if (e.InnerException != null)
                {
                    AddLog(e.InnerException.Message);
                    AddLog(e.InnerException.StackTrace);
                }
                Console.WriteLine(ParseLog());
            }
        }

        /// <summary>
        /// Loads the mods
        /// </summary>
        internal static void LoadMods()
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
                try
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

                    mod.LoadedAssembly = Assembly.LoadFrom(modAssemblyPath);
                    mod.ModAssemblyPath = modAssemblyPath;

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
                catch(Exception E)
                {
                    AddLog($"ERROR! Failed to read mod \"{subDir}\": {E.Message}\n{E.StackTrace}");
                }
            }

            // LoadBefore and LoadAfter stuff

            AddLog(" ");
            AddLog("Installed mods:");

            foreach (var mod in firstMods)
            {
                if (mod != null)
                {
                    loadedMods.Add(LoadMod(mod));
                    LogModLoadTime(mod);
                }
            }

            foreach (var mod in otherMods)
            {
                if (mod != null)
                {
                    loadedMods.Add(LoadMod(mod));
                    LogModLoadTime(mod);
                }
            }

            foreach (var mod in lastMods)
            {
                if (mod != null)
                {
                    loadedMods.Add(LoadMod(mod));
                    LogModLoadTime(mod);
                }
            }

            if (sw.IsRunning)
                sw.Stop();

            FlagGame();

            Console.WriteLine(ParseLog());
        }

        private static void LogModLoadTime(QMod mod)
        {
            if (elapsedTimes.TryGetValue(mod, out string elapsed))
                AddLog($"- {mod.DisplayName} ({mod.Id}) - {elapsed}");
            else
                AddLog($"- {mod.DisplayName} ({mod.Id}) - Unknown load time");
        }

        /// <summary>
        /// The path of the QMods folder. If game is not launched through
        /// </summary>
        internal static string QModBaseDir = Environment.CurrentDirectory.Contains("system32") && Environment.CurrentDirectory.Contains("Windows") ? "ERR" : Environment.CurrentDirectory + @"/QMods";

        /// <summary>
        /// A list of all of the loaded mods
        /// </summary>
        internal static List<QMod> loadedMods = new List<QMod>();

        /// <summary>
        /// Whether the game is patched or not
        /// </summary>
        internal static bool patched = false;

        /// <summary>
        /// Uses reflection to load a mod
        /// </summary>
        /// <param name="mod">The mod to load</param>
        /// <returns>The loaded mod</returns>
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

                    MethodInfo qPatchMethod = mod.LoadedAssembly.GetType(entryType).GetMethod(entryMethod);
                    qPatchMethod.Invoke(mod.LoadedAssembly, new object[] { });

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

        #endregion

        #region Timer

        /// <summary>
        /// A timer to display mod loading times
        /// </summary>
        internal static Stopwatch sw = new Stopwatch();

        /// <summary>
        /// Contains every mod and their loading times
        /// </summary>
        internal static Dictionary<QMod, string> elapsedTimes = new Dictionary<QMod, string>();

        /// <summary>
        /// Uses the default stopwatch to return a parsed time which can be used in the logs
        /// </summary>
        /// <returns>The parsed loading time of a mod</returns>
        internal static string ParseTime()
            => ParseTime(sw);

        /// <summary>
        /// Uses the provided stopwatch to return a parsed time which can be used in the logs
        /// </summary>
        /// <param name="sw">The stopwatch</param>
        /// <returns>The parsed loading time of a mod</returns>
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

        #endregion

        #region Flagging

        /// <summary>
        /// The current version of QModManager
        /// </summary>
        internal static Version version = new Version(1, 3, 1);

        /// <summary>
        /// Gets a line that is used in <see cref="Patches.UIScreenBugReport_Post"/> and <see cref="Patches.UIScreenBugReport_PostIt"/>
        /// </summary>
        /// <returns>The line that needs to be used</returns>
        internal static string GetModsLine()
            => $"This game is modded! Using QModManager {version}, with {loadedMods.Count} loaded mods. (Check the output log for a complete list of installed mods)";

        /// <summary>
        /// Flags the game as being modded by doing some patches from the <see cref="Patches"/> class
        /// </summary>
        internal static void FlagGame()
        {
            if (sw.IsRunning)
                sw.Stop();
            sw.Reset();
            sw.Start();

            string Id = "ttqmm.internal.modflag";
            string Name = "Game Flagging";

            try
            {
                HarmonyInstance.Create(Id).PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                AddLog("EXCEPTION CAUGHT! (Internal patch)");
                AddLog(e.Message);
                AddLog(e.StackTrace);
                if (e.InnerException != null)
                {
                    AddLog(e.InnerException.Message);
                    AddLog(e.InnerException.StackTrace);
                }
            }
			try
			{
				LobbySystem.PROTOCOL_VERSION+= 1000;
			}
			catch (Exception e)
			{
				AddLog("EXCEPTION CAUGHT! (Changing lobby protocol version)");
                AddLog(e.Message);
                AddLog(e.StackTrace);
                if (e.InnerException != null)
                {
                    AddLog(e.InnerException.Message);
                    AddLog(e.InnerException.StackTrace);
                }
			}

            sw.Stop();

            AddLog("Internal stuff:");
            AddLog($"- {Name} ({Id}) - {ParseTime(sw)}");
        }

        #endregion

        #region Logging

        /// <summary>
        /// A list of lines which need to be outputted
        /// </summary>
        internal static List<string> rawLines = new List<string>();

        /// <summary>
        /// Adds a line to the <see cref="rawLines"/> list
        /// </summary>
        /// <param name="line">The line to add</param>
        internal static void AddLog(string line)
        {
            foreach (string segment in line.Split(new char[]{'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries))
                rawLines.Add(segment);
        }

        /// <summary>
        /// Parses the lines in the <see cref="rawLines"/> and returns an output
        /// </summary>
        /// <returns>A parsed string, ready to be outputted to the console</returns>
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
            rawLines.Clear();
            return output;
        }

        /// <summary>
        /// Generates a blank line for <see cref="ParseLog"/> based on a maximum lengh
        /// </summary>
        /// <param name="maxLength">The maxmimum length of the line</param>
        /// <returns>A blank line</returns>
        internal static string Blank(int maxLength)
        {
            string output = "";
            output += "#  ";
            for (int i = " ".Length; i < maxLength; i++)
                output += " ";
            output += " #\n";
            return output;
        }

        #endregion
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
            internal static MethodInfo TargetMethod()
                => AccessTools.FirstInner(typeof(UIScreenBugReport), (type) => type.Name.Contains("<PostIt>")).GetMethod("MoveNext", AccessTools.all);

            [HarmonyTranspiler]
            [HarmonyPriority(MaxPriority.Last)]
            internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var codes = new List<CodeInstruction>(instructions);
                var codesToInsert = new List<CodeInstruction>();
                //for (int i = 0; i < codes.Count; i++)
                //    Console.WriteLine($"[IL-DEBUG] {i}: {codes[i].opcode} : {codes[i].operand}");

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
                    QModPatcher.AddLog($"ERROR! Could not patch UIScreenBugReport.PostIt!");
                    QModPatcher.AddLog($"platformStringIndex: {platformStringIndex}, callIndex: {callIndex}, ldargs0Index: {ldarg0Index}");
                }

                //for (int i = 0; i < codes.Count; i++)
                //    Console.WriteLine($"[IL-DEBUG] {i}: {codes[i].opcode} : {codes[i].operand}");

                return codes;
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
}