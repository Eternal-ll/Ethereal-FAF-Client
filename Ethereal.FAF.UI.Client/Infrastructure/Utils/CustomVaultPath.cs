using System.IO;
using System.Linq;

namespace Ethereal.FAF.UI.Client.Infrastructure.Utils
{
    internal static  class FaPaths
    {
        public static string Path;

        public static string Maps => System.IO.Path.Combine(Path, "maps");
        public static string Mods => System.IO.Path.Combine(Path, "mods");
        public static bool TryGetCustomVaultPath(out string customVaultPath)
        {
            var configLua = System.IO.Path.Combine(Properties.Paths.Default.Patch, "fa_path.lua");
            customVaultPath = null;
            if (File.Exists(configLua))
            {
                var settings = File.ReadAllLines(configLua);
                var custom = settings.FirstOrDefault(s => s.Split('=')[0].Trim() == "custom_vault_path");
                if (custom is not null)
                {
                    customVaultPath = custom.Split('=')[1].Trim().Trim('\"');
                    if (customVaultPath[^1] != '/') customVaultPath += '/';
                    return true;
                }
            }
            return customVaultPath is not null;
        }
    }
}
