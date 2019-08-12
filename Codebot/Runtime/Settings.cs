using System;
using System.IO;
using System.Configuration;

namespace Codebot.Runtime
{
    public static class Settings
    {
        private static Configuration machine;

        public static Configuration Machine
        {
            get
            {
                if (machine == null)
                    machine = ConfigurationManager.OpenMachineConfiguration();
                return machine;
            }
        }

        public static string ConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        public static void Refresh()
        {
            ConfigurationManager.RefreshSection("appSettings");
        }

        public static string ReadString(string name, string defaultValue = "")
        {
            string value = ConfigurationManager.AppSettings[name];
            return value.Length > 0 ? value : defaultValue;
        }

        public static void WriteString(string name, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(name);
            config.AppSettings.Settings.Add(name, value);
            config.Save(ConfigurationSaveMode.Modified);
        }

        public static int ReadInt(string name, int defaultValue = 0)
        {
            return int.TryParse(ConfigurationManager.AppSettings[name], out int value) ? value : defaultValue;
        }

        public static double ReadFloat(string name, double defaultValue = 0)
        {
            return double.TryParse(ConfigurationManager.AppSettings[name], out double value) ? value : defaultValue;
        }

        public static bool ReadBool(string name, bool defaultValue = false)
        {
            return bool.TryParse(ConfigurationManager.AppSettings[name], out bool value) ? value : defaultValue;
        }
    }
}
