using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using QModInstaller;

namespace ModHelper
{
    /// <summary>
    /// A helper class to manage your mod's config.json file and use it's values
    /// </summary>
    public class ModConfig
    {
        /// <summary>
        /// Allow the use of System.Reflection within these methods to read and write to binded class fields
        /// </summary>
        public bool UseReflection = false;

        /// <summary>
        /// Get or set a Config value. If it doesn't exist, make a new one (If Reflection : Setting will also set corresponding variable)
        /// </summary>
        /// <param name="key">The name of the variable to index</param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                return config[key];
            }
            set
            {
                config[key] = value;
                if (UseReflection)
                {
                    var e = FieldRefList[key];
                    if (e != null)
                    {
                        ConfigToFieldRef(e[1], (FieldInfo)e[0], key);
                    }
                }
            }
        }
        private Dictionary<string, object> config = new Dictionary<string, object>();
        private Dictionary<string, object[]> FieldRefList = new Dictionary<string, object[]>();
        private Dictionary<string, int> FieldRefRepeatCount = new Dictionary<string, int>();

        /// <summary>
        /// The location of the Config file
        /// </summary>
        [JsonIgnore]
        public string ConfigLocation;

        /// <summary>
        /// Load the Config from the current mod's directory
        /// </summary>
        public ModConfig()
        {
            string path = Path.Combine(Assembly.GetCallingAssembly().Location, "..\\config.json");
            ConfigLocation = path;
            ReadConfigJsonFile(this);
        }
        /// <summary>
        /// Load the Config file from it's path
        /// </summary>
        /// <param name="path">The path of the Config file</param>
        public ModConfig(string path)
        {
            ConfigLocation = path;
            ReadConfigJsonFile(this);
        }

        /// <summary>
        /// (Reflection) Get the FieldInfo of a class's variable
        /// </summary>
        /// <typeparam name="T">The holding class to get the variable from</typeparam>
        /// <param name="VariableName">The name of the variable</param>
        /// <returns>FieldInfo representing the class's variable</returns>
        public static FieldInfo GetFieldInfo<T>(string VariableName) => typeof(T).GetField(VariableName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// (Reflection) Get the FieldInfo of a class's variable
        /// </summary>
        /// <param name="T">The holding class to get the variable from</param>
        /// <param name="VariableName">The name of the variable</param>
        /// <returns>FieldInfo representing the class's variable</returns>
        public static FieldInfo GetFieldInfo(Type T, string VariableName) => T.GetField(VariableName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// (Reflection) Bind a field to the Config for loading and saving (WARNING : Using this will set UseReflection to true)
        /// </summary>
        /// <param name="instance">The class instance to use, null if static</param>
        /// <param name="field">The variable to use, acquire with 'typeof(Class).GetField("variableName")', or ModConfig.GetFieldInfo</param>
        /// <param name="UpdateRef">Set the value of the variable to what's in the Config, if it exists</param>
        public void BindConfig(object instance, FieldInfo field, bool UpdateRef = true)
        {
            if (!UseReflection)
                UseReflection = true;

            int cache = 0;
            string ats = "";
            if (FieldRefRepeatCount.TryGetValue(field.Name, out cache))
            {
                ats = "/" + cache.ToString();
            }

            FieldRefRepeatCount[field.Name] = cache + 1;

            FieldRefList.Add(field.Name + ats, new object[] { field, instance });

            if (UpdateRef)
                ConfigToFieldRef(instance, field, field.Name + ats);
        }
        /// <summary>
        /// (Reflection) Bind a field to the Config for loading and saving (WARNING : Using this will set UseReflection to true)
        /// </summary>
        /// <typeparam name="T">The class type</typeparam>
        /// <param name="instance">The class instance to use, null if static</param>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="UpdateRef">Set the value of the variable to what's in the Config, if it exists</param>
        public void BindConfig<T>(T instance, string VariableName, bool UpdateRef = true)
        {
            BindConfig(instance, GetFieldInfo<T>(VariableName), UpdateRef);
        }

        private void ConfigToFieldRef(object instance, FieldInfo field, string Search)
        {
            try
            {
                if (field.FieldType == typeof(float))
                {
                    float cache = 0f;
                    if (TryGetConfigF(Search, ref cache))
                    {
                        field.SetValue(instance, cache);
                    }
                }
                else
                {
                    object cache = null;
                    if (TryGetConfig(Search, ref cache))
                    {
                        try
                        {
                            field.SetValue(instance, cache);
                        }
                        catch
                        {
                            try
                            {
                                field.SetValue(instance, Convert.ChangeType(cache, field.FieldType));
                            }
                            catch
                            {
                                field.SetValue(instance, ((Newtonsoft.Json.Linq.JObject)cache).ToObject(field.FieldType));
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {

                throw new Exception("Something wrong happened while trying to set a FieldInfo value\n" + e.Message + "\nThe FieldInfo was " + (field.FieldType == typeof(float) ? "a float (" : "not a float (") + field.FieldType.ToString() + ")\nThe name being searched for: " + Search);
            }
        }

        /// <summary>
        /// Get a value of a specified name from the Config
        /// </summary>
        /// <typeparam name="T">The type of object being acquired</typeparam>
        /// <param name="ConfigID">The name of the object to try to get</param>
        /// <param name="value">Returns the object as type if it exists</param>
        /// <returns>Returns true if the object exists</returns>
        public bool TryGetConfig<T>(string ConfigID, ref T value)
        {
            object cache = null;
            bool result = this.config.TryGetValue(ConfigID, out cache);
            if (result)
            {
                value = (T)cache;
            }
            return result;
        }
        /// <summary>
        /// Get a float value of a specified name from the Config
        /// </summary>
        /// <param name="ConfigID">The name of the float value to try to get</param>
        /// <param name="value">Returns the float value if it exists</param>
        /// <returns>Returns true if the object exists</returns>
        public bool TryGetConfigF(string ConfigID, ref float value)
        {
            object cache = null;
            bool result = this.config.TryGetValue(ConfigID, out cache);
            if (result)
            {
                value = Convert.ToSingle(cache);
            }
            return result;
        }

        /// <summary>
        /// Write Config data to the file (If Reflection: Apply all referenced fields to the Config before serializing)
        /// </summary>
        /// <returns>Returns true if successful</returns>
        public bool WriteConfigJsonFile()
        {
            return WriteConfigJsonFile(this);
        }
        /// <summary>
        /// Write Config data to the file (If Reflection: Apply all referenced fields to the Config before serializing)
        /// </summary>
        /// <returns>Returns true if successful</returns>
        public static bool WriteConfigJsonFile(ModConfig Config)
        {
            try
            {
                if (Config.UseReflection)
                    foreach (var field in Config.FieldRefList)
                    {
                        FieldInfo finfo = (FieldInfo)field.Value[0];
                        Config[finfo.Name] = finfo.GetValue(field.Value[1]);
                    }

                string json = JsonConvert.SerializeObject(Config.config,Formatting.Indented);

                File.WriteAllText(Config.ConfigLocation, json);

                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("ERROR! config.json deserialization failed.\n" + e.Message + "\n" + e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// (Reflection) Reload all the Config values and push them to the references
        /// </summary>
        public void ReapplyConfigToRef()
        {
            if (UseReflection)
            foreach (var field in FieldRefList)
            {
                ConfigToFieldRef(field.Value[1], (FieldInfo)field.Value[0], field.Key);
            }
        }

        /// <summary>
        /// Reload the Config file
        /// </summary>
        /// <param name="Config">The ModConfig class to add changes to (If instance uses Reflect: It will apply Config changes to binded fields)</param>
        /// <returns>Returns true if successful</returns>
        public static bool ReadConfigJsonFile(ModConfig Config)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                string json = File.ReadAllText(Config.ConfigLocation);
                var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, settings);
                foreach (var pair in config)
                {
                    Config[pair.Key] = pair.Value;
                }
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("ERROR! config.json deserialization failed.\n" + e.Message + "\n" + e.StackTrace);
                return false;
            }
        }
    }
}
