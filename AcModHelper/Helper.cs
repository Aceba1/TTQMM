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
    public class ModConfig
    {
        public Dictionary<string, object> Config = new Dictionary<string, object>();

        public Dictionary<string, object[]> FieldRefList = new Dictionary<string, object[]>();
        private Dictionary<string, int> FieldRefRepeatCount = new Dictionary<string, int>();

        public string ConfigLocation;

        /// <summary>
        /// Load the Config to a new instance from the caller's directory
        /// </summary>
        public ModConfig()
        {
            string path = Path.Combine(Assembly.GetCallingAssembly().Location, "..\\config.json");
            ConfigLocation = path;
            ReadConfigJsonFile(false);
        }

        /// <summary>
        /// Get the FieldInfo of a class's variable
        /// </summary>
        /// <typeparam name="T">The class to get the variable from</typeparam>
        /// <param name="VariableName">The name of the variable</param>
        /// <returns>FieldInfo representing the class's variable</returns>
        public static FieldInfo GetFieldInfo<T>(string VariableName) => typeof(T).GetField(VariableName);
        /// <summary>
        /// Get the FieldInfo of a class's variable
        /// </summary>
        /// <param name="T">The class to get the variable from</param>
        /// <param name="VariableName">The name of the variable</param>
        /// <returns>FieldInfo representing the class's variable</returns>
        public static FieldInfo GetFieldInfo(Type T, string VariableName) => T.GetField(VariableName);

        /// <summary>
        /// Bind a field to the Config for loading and saving
        /// </summary>
        /// <param name="instance">The class instance to use, null if static</param>
        /// <param name="field">The variable to use, acquire with 'typeof(Class).GetField("variableName")', or GetFieldInfo</param>
        /// <param name="UpdateRef">Set the value of the variable to what's in the Config, if it exists</param>
        public void BindConfig(object instance, FieldInfo field, bool UpdateRef = true)
        {
            int cache = 0;
            string ats = "";
            if (FieldRefRepeatCount.TryGetValue(field.Name, out cache))
            {
                ats = "/_" + cache.ToString();
            }

            FieldRefRepeatCount[field.Name] = cache + 1;

            FieldRefList.Add(field.Name + ats, new object[] { field, instance });
            if (UpdateRef)
                ConfigToFieldRef(instance, field, field.Name + ats);
        }
        /// /// <summary>
        /// Bind a field to the Config for loading and saving
        /// </summary>
        /// /// <typeparam name="T">The class type</typeparam>
        /// <param name="instance">The class instance to use, null if static</param>
        /// <param name="VariableName">The name of the variable</param>
        /// <param name="UpdateRef">Set the value of the variable to what's in the Config, if it exists</param>
        public void BindConfig<T>(T instance, string VariableName, bool UpdateRef = true)
        {
            BindConfig(instance, GetFieldInfo<T>(VariableName), UpdateRef);
        }

        private void ConfigToFieldRef(object instance, FieldInfo field, string Search)
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
                    field.SetValue(instance, cache);
                }
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

            bool result = this.Config.TryGetValue(ConfigID, out cache);
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
            bool result = this.Config.TryGetValue(ConfigID, out cache);
            if (result)
            {
                value = Convert.ToInt64(cache);
            }
            return result;
        }
        /// <summary>
        /// Apply binded fields to the Config, and write changes to the Config file
        /// </summary>
        /// <param name="UpdateFromRefList">Apply binded fields to the Config and file</param>
        /// <returns>Returns true if successful</returns>
        public bool WriteConfigJsonFile()
        {
            try
            {
                foreach (var field in FieldRefList)
                {
                    FieldInfo finfo = (FieldInfo)field.Value[0];
                    Config[finfo.Name] = finfo.GetValue(field.Value[1]);
                }
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                string json = JsonConvert.SerializeObject(Config, settings);
                File.WriteAllText(ConfigLocation, json);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR! config.json deserialization failed.\n" + e.Message + "\n" + e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Reload all the Config values and push them to the references
        /// </summary>
        public void ReapplyConfigToRef()
        {
            foreach (var field in FieldRefList)
            {
                ConfigToFieldRef(field.Value[1], (FieldInfo)field.Value[0], field.Key);
            }
        }

        /// <summary>
        /// Reload the Config file
        /// </summary>
        /// <param name="ApplyToRefList">Apply changes loaded from the Config file to binded fields</param>
        /// <returns>Returns true if successful</returns>
        public bool ReadConfigJsonFile(bool ApplyToRefList = true)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                string json = File.ReadAllText(ConfigLocation);
                Config = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, settings);
                if (ApplyToRefList)
                {
                    ReapplyConfigToRef();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR! config.json deserialization failed.\n" + e.Message + "\n" + e.StackTrace);
                return false;
            }
        }
    }
}
