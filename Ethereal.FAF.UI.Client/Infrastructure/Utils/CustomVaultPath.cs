using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ethereal.FAF.UI.Client.Infrastructure.Utils
{
    internal static class FaPaths
    {
        public const string Config = "fa_path.lua";
        //public static string Path;

        //public static string Maps => System.IO.Path.Combine(Path, "maps");
        //public static string Mods => System.IO.Path.Combine(Path, "mods");

        //public static string path => System.IO.Path.Combine("C:\\ProgramData\\FAForever", "fa_path.lua");

        public static string GetCustomVaultPath(string path) => Get("custom_vault_path", path);
        public static string GetFaPAth(string path) => Get("fa_path", path);
        public static void SetCustomVaultPath(string target, string path) => 
            SetOrUpdate("custom_vault_path", target, path);
        public static void SetFaPath(string target, string path) =>
            SetOrUpdate("fa_path", target, path);


        public static IEnumerable<(string Key, string Value)> GetSettings(string path)
        {
            using var fs = new FileStream(Path.Combine(path, Config), FileMode.OpenOrCreate);
            using var sr = new StreamReader(fs);
            var settings = sr.ReadToEnd().Split('\n');
            foreach (var setting in settings)
            {
                if (string.IsNullOrWhiteSpace(setting)) continue;
                var data = setting.Split('=');
                if (data.Length > 0) yield return (data[0].Trim(), data[1].Replace("\"", null).Trim());
                else yield return (data[0].Trim(), null);
            }
        }
        public static string Get(string key, string path)
        {
            var settings = GetSettings(path);
            foreach (var (Key, Value) in settings)
            {
                if (Key == key) return Value;
            }
            return null;
        }
        public static string GetOrSetDefault(string key, string value, string path)
        {
            var settings = GetSettings(path);
            foreach (var (Key, Value) in settings)
            {
                if (Key == key) return Value;
            }
            File.AppendAllText(path, GetSetting(key, value));
            return value;
        }
        public static void SetOrUpdate(string key, string value, string path)
        {
            var settings = GetSettings(path);
            StringBuilder sb = new();
            var found = false;
            foreach (var (Key, Value) in settings)
            {
                if (Key != key)
                {
                    sb.Append(GetSetting(Key, Value));
                    continue;
                }
                found = true;
                sb.Append(GetSetting(key, value));
            }
            if (!found) sb.Append(GetSetting(key, value));
            File.WriteAllText(path, sb.ToString());
        }
        private static string GetSetting(string key, string value) => $"{key} = \"{value}\"\n";
    }
}
