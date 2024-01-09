using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Utils
{
    internal static class JavaHelper
    {
        public static bool HasJavaRuntime()
        {
            return Directory.Exists("External/jre");
        }
        public static async Task PrepareJavaRuntime(IProgress<string>? progress = null)
        {
            if (!HasJavaRuntime())
            {
                progress?.Report("Extracting portable Java runtime");
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "External/7z.exe",
                        Arguments = "x External/jre.7z -o External/",
                        UseShellExecute = false
                    }

                };
                process.Start();
                await process.WaitForExitAsync();
                progress?.Report("Poratble Java runtime extracted");
            }
        }
    }
}
