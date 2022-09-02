using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ethereal.FAF.UI.Client.Infrastructure.Utils
{
    internal static  class CustomVaultPath
    {
        public static bool TryGetCustomVaultPath(out string customVaultPath)
        {
            var configLua = @"C:\ProgramData\FAForever\fa_path.lua";
            customVaultPath = null;
            if (File.Exists(configLua))
            {
                var settings = File.ReadAllLines(configLua);
                var customVaultRegex = new Regex("(custom_vault_path)(.*)\"(.*)\"");
                var custom = settings.FirstOrDefault(s => customVaultRegex.IsMatch(s));
                if (custom is not null)
                {
                    customVaultPath = customVaultRegex.Matches(custom).FirstOrDefault().Groups[^1].Value;
                    if (customVaultPath[^1] != '/') customVaultPath += '/';
                    return true;
                }
            }
            return false;
        }
    }
}
