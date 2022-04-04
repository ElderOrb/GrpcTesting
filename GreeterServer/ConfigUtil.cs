using System;
using System.Collections;
using System.IO;
using System.Configuration;
using System.Reflection;
using System.Collections.Generic;

namespace Test.Configuration
{
    public class ConfigUtil
    {
        private const string kAppSettingsKey = "appSettings";

        /// <summary>
        /// The name of the application configuration file.
        /// </summary>
        private const string kAppConfig = "App.config";

        /// <summary>
        /// A prefix found on assembly paths returned by Assembly.GetExecutingAssembly().CodeBase.
        /// </summary>
        private const string kFilePrefix = @"file:\";
        public const string kPathSeparator = "/";
        private Hashtable appSettings = null;
        private bool initialized = false;
        private static string localConfigFile = string.Empty;
        private static string FilePrefix
        {
            get
            {
                return kFilePrefix;
            }
        }

        public ConfigUtil()
        {
        }
        public string getAppSetting(string key, Type callerType, bool logErrors)
        {
            string rval = null;

            if (!initialized)
            {
                Initialize();
            }

            rval = ConfigurationManager.AppSettings[key];

            if ((rval == null) && logErrors)
            {
                Console.WriteLine("could not retrieve AppSetting {0} for {1}", key, callerType.FullName);
            }

            return rval;
        }

        public void UpdateAppSetting(string key, string stringValue)
        {
            if (!initialized)
            {
                Initialize();
            }

            string currentValue = getAppSetting(key, typeof(object), true);
            if (string.Compare(currentValue, stringValue) != 0)
            {
                System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove(key);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                config.AppSettings.Settings.Add(key, stringValue);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        public void Initialize()
        {
            if (!initialized)
            {
                initialized = true;
            }
        }

        public List<KeyValuePair<string, object>> GetSettingsByPrefix(string sectionKey)
        {
            var typedSettings = new List<KeyValuePair<string, object>>();
            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                var value = ConfigurationManager.AppSettings[key].ToString();
                if (key.Split('.')[0] == sectionKey)
                {
                    if (int.TryParse(value, out int intValue))
                    {
                        typedSettings.Add(new KeyValuePair<string, object>(key, intValue));
                    }
                    else
                    {
                        typedSettings.Add(new KeyValuePair<string, object>(key, value.ToString()));
                    }
                }

            }

            return typedSettings;
        }

        public string GetAppSetting(string key, System.Type callerType)
        {
            return getAppSetting(key, callerType, true);
        }

        public string GetAppSetting(string key, object caller)
        {
            System.Type callerType = (caller != null) ? caller.GetType() : typeof(object);

            return getAppSetting(key, callerType, true);
        }

        public string TryGetAppSetting(string key, System.Type callerType)
        {
            return getAppSetting(key, callerType, false);
        }

        public string TryGetAppSetting(string key, object caller)
        {
            System.Type callerType = (caller != null) ? caller.GetType() : typeof(object);

            return getAppSetting(key, callerType, false);
        }

        public T GetConfigSection<T>(string sectionName) where T : class
        {
            return ConfigurationManager.GetSection(sectionName) as T;
        }

        public void Close()
        {
            if (appSettings != null)
            {
                appSettings = null;
            }
        }

        public static string GetConfigFilePath(string filename)
        {
            string rval = null;

            FileInfo fi = new FileInfo(filename);
            if (fi.Exists)
            {
                rval = fi.FullName;
            }
            else
            {
                string localFilename = Path.GetDirectoryName(Assembly.GetCallingAssembly().CodeBase).Replace(FilePrefix, "")
                  + System.IO.Path.DirectorySeparatorChar + filename;

                if (File.Exists(localFilename))
                {
                    rval = localFilename;
                }
            }

            return rval;
        }

        public static string GetConfigFileDirectory()
        {
            string configFilePath;
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configFilePath = config.FilePath;

            return Path.GetDirectoryName(configFilePath);
        }

        public static string GetConfigurationPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase).Replace(FilePrefix, "") + System.IO.Path.DirectorySeparatorChar;
        }

        public static Assembly GetAssembly()
        {
            return Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        }

        public static Version GetAssemblyVersion()
        {
            return GetAssembly().GetName().Version;
        }
    }
}