using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace QModInstaller
{
    public class QMod
    {
        public string Id = "Mod_ID";
        public string DisplayName = "Display name";
        public string Author = "Author name";
        public string Version = "0.0.0";
        public string[] Requires = new string[] { };
        public bool Enable = true;
        public string AssemblyName = "DLL Filename";
        public string EntryMethod = "Namespace.Class.Method";
        public string Priority = "First or Last"; 
        public Dictionary<string, object> Config = new Dictionary<string, object>();

        [JsonIgnore]
        public Assembly loadedAssembly;

        [JsonIgnore]
        public string modAssemblyPath;


        public QMod() { }

        public static QMod FromJsonFile(string file)
        {
            try
            {
                var json = File.ReadAllText(file);
                return JsonConvert.DeserializeObject<QMod>(json);
            }
            catch(Exception e)
            {
                Console.WriteLine("QMOD ERR: mod.json deserialization failed!");
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
