using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    public class MapGenerator
    {
        private readonly string JavaRuntime;
        private readonly string Logging;
        private readonly string PreviewPath;
        private readonly string MapGeneratorsFolder;
        private readonly string MapGeneratorRepository;

        private readonly IHttpClientFactory HttpClientFactory;
        private readonly ILogger Logger;

        private static Regex Pattern = new Regex(@"neroxis_map_generator_(\d+\.\d+\.\d+)_(.*)");
        public MapGenerator(string javaRuntime, string logging, string previewPath, string mapGeneratorsFolder, string mapGeneratorRepository, IHttpClientFactory httpClientFactory, ILogger logger)
        {
            JavaRuntime = javaRuntime;
            Logging = logging;
            PreviewPath = previewPath;
            MapGeneratorsFolder = mapGeneratorsFolder;
            HttpClientFactory = httpClientFactory;
            MapGeneratorRepository = mapGeneratorRepository;
            Logger = logger;
        }

        private string MapGeneratorFile(string version) => MapGeneratorsFolder + $"MapGenerator_{version}.jar";

        private Process GetProcess(string path)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = JavaRuntime,
                    Arguments = $"-jar \"{path}\" ",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                }
            };
            var args = process.StartInfo.Arguments;
            if (!string.IsNullOrWhiteSpace(PreviewPath))
            {
                args += MapGeneratorArguments.SetPreviewPath(PreviewPath);
            }
            return process;
        }

        public async Task<(bool Success, string Description)> GenerateMap(string map, string targetFolder,
            CancellationToken cancellationToken = default, IProgress<string> progress = null)
        {
            var data = Pattern.Match(map);
            var mapgenVersion = data.Groups[1].Value;
            var mapgen = MapGeneratorFile(mapgenVersion);
            progress?.Report($"Confirming map generator [{mapgenVersion}]");
            if (!File.Exists(mapgen))
            {
                var client = HttpClientFactory.CreateClient();
                client.BaseAddress = new Uri(MapGeneratorRepository);
                progress?.Report($"Downloading map generator [{mapgenVersion}]");
                var response = await client.GetAsync($"{mapgenVersion}/NeroxisGen_{mapgenVersion}.jar");
                if (!response.IsSuccessStatusCode)
                {
                    return (false, null);
                }
                var fs = new FileStream(mapgen, FileMode.Create);
                await response.Content.CopyToAsync(fs);
                await fs.DisposeAsync();
                fs.Close();
                progress?.Report($"Map generator [{mapgenVersion}] downloaded");
            }
            progress?.Report($"Launching map generator [{mapgenVersion}]");
            var process = GetProcess(mapgen);
            var args = process.StartInfo.Arguments;
            args += MapGeneratorArguments.SetMapName(map);
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
                progress?.Report(await process.StandardOutput.ReadLineAsync());
            }
            await process.WaitForExitAsync(cancellationToken);
            return (true, "");
        }

        public bool IsNeroxisMap(string map) => Pattern.IsMatch(map);
    }
}
