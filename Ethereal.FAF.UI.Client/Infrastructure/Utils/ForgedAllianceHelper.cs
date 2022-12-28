using System.IO;

namespace Ethereal.FAF.UI.Client.Infrastructure.Utils
{
    internal static class ForgedAllianceHelper
    {
        public const string DefaultInstallationLocation = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Supreme Commander Forged Alliance";
        public const string DefaultVaultLocation = "C:\\Users\\%username%\\Documents\\My Games\\Gas Powered Games\\Supreme Commander Forged Alliance";
        /// <summary>
        /// Steam PID of game
        /// </summary>
        public const int SteamPID = 9420;
        /// <summary>
        /// Registry key of Steam version of game
        /// </summary>
        public const string WindowsSteamRegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 9420";
        public const string WindowsNoSteamRegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Supreme Commander Forged Alliance_is1";
        public const string WindowsNoSteam2RegistryKey = "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Supreme Commander Forged Alliance_is1";

        public static readonly string[] FilesToCopy =
        {
            "bin\\BsSndRpt.exe",
            "bin\\BugSplat.dll",
            "bin\\BugSplatRc.dll",
            "bin\\DbgHelp.dll",
            "bin\\GDFBinary.dll",
            "bin\\msvcm80.dll",
            "bin\\msvcp80.dll",
            "bin\\msvcr80.dll",
            "bin\\SHSMP.DLL",
            "bin\\sx32w.dll",
            "bin\\wxmsw24u-vs80.dll",
            "bin\\zlibwapi.dll"
        };

        #region Is game installed
        public static bool TryFindGameDirectory(out string directory, bool searchRegistry = false) =>
            TryGetGameLocationByDefault(out directory) ||
            TryGetGameLocationByFaPath(out directory) ||
            TryGetGameLocationByRegistryKey(WindowsSteamRegistryKey, out directory) ||
            TryGetGameLocationByRegistryKey(WindowsNoSteamRegistryKey, out directory) ||
            TryGetGameLocationByRegistryKey(WindowsNoSteam2RegistryKey, out directory) ||
            (searchRegistry && TryGetGameLocationByRegistrySearch(out directory));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchRegistry"></param>
        /// <returns></returns>
        public static bool IsGameInstalledByRegistryKey(bool searchRegistry = false)
        {
            var gameFound =
                IsGameInstalledByRegistryKey(WindowsSteamRegistryKey) ||
                IsGameInstalledByRegistryKey(WindowsNoSteamRegistryKey) ||
                IsGameInstalledByRegistryKey(WindowsNoSteam2RegistryKey);
            if (gameFound) return true;
            return false;
            //return searchRegistry && IsGameInstalledByRegistrySearch();
        }
        private static string GetGameDirectoryByRegistryKey(string registryKey)
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKey);
            var location = key?.GetValue("InstallLocation")?.ToString();
            return location;
        }
        private static bool TryGetGameLocationByFaPath(out string location)
        {
            return FaPaths.TryGetFaPath(out location) && DirectoryHasAnyGameFile(location);
        }
        private static bool TryGetGameLocationByRegistryKey(string registryKey, out string location)
        {
            location = GetGameDirectoryByRegistryKey(registryKey);
            return DirectoryHasAnyGameFile(location);
        }
        private static bool TryGetGameLocationByDefault(out string location)
        {
            location = DefaultInstallationLocation;
            return DirectoryHasAnyGameFile(location);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="registryKey"></param>
        /// <returns></returns>
        private static bool IsGameInstalledByRegistryKey(string registryKey) =>
            DirectoryHasAnyGameFile(GetGameDirectoryByRegistryKey(registryKey));
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static bool TryGetGameLocationByRegistrySearch(out string location)
        {
            location = null;
            return false;
        }
        private static string GetGameLocationByRegistrySearch()
        {
            return null;
        }
        public static bool DirectoryHasAnyGameFile(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory)) return false;
            foreach (var file in FilesToCopy)
            {
                if (File.Exists(Path.Combine(directory, file)))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
