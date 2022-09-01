using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal static class MapGeneratorArguments
    {
        /// <summary>
        /// set the seed for the generated map
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static string SetSeed(string seed) => $"--seed {seed} ";
        /// <summary>
        /// set the target folder for the generated map
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string SetFolderPath(string path) => $"--folder-path \"{path}\" ";
        public static string SetMapName(string map) => $"--map-name {map} ";
    }
    public class MapGenerator
    {
        public const string Version = "1.8.5";

        private readonly string JavaRuntime;
        private readonly string Jar;
        private readonly string Logging;

        public MapGenerator(string javaRuntime, string jar, string logging)
        {
            JavaRuntime = javaRuntime;
            Jar = jar;
            Logging = logging;
        }

        private Process GetProcess() => new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = JavaRuntime,
                Arguments = $"-jar \"{Jar}\" ",
                RedirectStandardOutput = true,
            }
        };

        public async Task<(bool Success, string Description)> GenerateMap(string seed, string targetFolder,
            CancellationToken cancellationToken = default, IProgress<string> progress = null)
        {
            var process = GetProcess();
            var args = process.StartInfo.Arguments;
            args += MapGeneratorArguments.SetMapName(seed);
            args += MapGeneratorArguments.SetFolderPath(targetFolder);
            process.StartInfo.Arguments = args;
            try
            {
                if (!process.Start())
                {
                    return (false, "");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            while (!process.HasExited)
            {
                var data = await process.StandardOutput.ReadLineAsync();
                progress?.Report(data);
            }
            await process.WaitForExitAsync(cancellationToken);
            return (true, "");
        }

        //public bool IsNeroxisMap(string map)
    }
}
