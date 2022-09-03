using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        public event EventHandler<string> MapGenerated;
        public static List<string> KnownVersions = new List<string>()
        {
            "1.8.5", // "1.8.4", "1.8.4",
        };

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
                    CreateNoWindow = true,
                }
            };
            if (!string.IsNullOrWhiteSpace(PreviewPath))
            {
                process.StartInfo.Arguments += MapGeneratorArguments.SetPreviewPath(PreviewPath);
            }
            return process;
        }

        private Process GetProcessByVersion(string version)
        {
            var mapgen = MapGeneratorFile(version);
            var process = GetProcess(mapgen);
            return process;
        }

        public async Task<(bool Success, string Description)> GenerateMap(string map, string targetFolder,
            CancellationToken cancellationToken = default, IProgress<string> progress = null)
        {
            var data = Pattern.Match(map);
            var mapgenVersion = data.Groups[1].Value;
            await ConfirmOrDownloadAsync(mapgenVersion, progress);
            progress?.Report($"Launching map generator [{mapgenVersion}]");
            var process = GetProcessByVersion(mapgenVersion);
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

        public async Task<string[]> GetStylesAsync(string version)
        {
            var process = GetProcessByVersion(version);
            process.StartInfo.Arguments += MapGeneratorArguments.Styles;
            var styles = new List<string>();
            while (!process.StandardOutput.EndOfStream)
            {
                var output = await process.StandardOutput.ReadLineAsync();
                if (!output.Contains(' ')) styles.Add(output);
            }
            return styles.ToArray();
        }
        public async Task<string[]> GetStylesWeightsAsync(string version)
        {
            var process = GetProcessByVersion(version);
            process.StartInfo.Arguments += MapGeneratorArguments.Weights;
            var styles = new List<string>();
            while (!process.StandardOutput.EndOfStream)
            {   
                var output = await process.StandardOutput.ReadLineAsync();
                if (!output.Contains(' ')) styles.Add(output);
            }
            return styles.ToArray();
        }
        public async Task<string[]> GetBiomesAsync(string version)
        {
            var process = GetProcessByVersion(version);
            process.StartInfo.Arguments += MapGeneratorArguments.Weights;
            var styles = new List<string>();
            while (!process.StandardOutput.EndOfStream)
            {
                var output = await process.StandardOutput.ReadLineAsync();
                if (!output.Contains(' ')) styles.Add(output);
            }
            return styles.ToArray();
        }

        public bool IsMapGeneratorExist(string version) => File.Exists(MapGeneratorFile(version));

        public async Task ConfirmOrDownloadAsync(string version, IProgress<string> progress = null)
        {
            var mapgen = MapGeneratorFile(version);
            progress?.Report($"Confirming map generator [{version}]");
            if (!File.Exists(mapgen))
            {
                var client = HttpClientFactory.CreateClient();
                client.BaseAddress = new Uri(MapGeneratorRepository);
                progress?.Report($"Downloading map generator [{version}]");
                var response = await client.GetAsync($"{version}/NeroxisGen_{version}.jar");
                //if (!response.IsSuccessStatusCode)
                //{
                //    return (false, null);
                //}
                var fs = new FileStream(mapgen, FileMode.Create);
                await response.Content.CopyToAsync(fs);
                await fs.DisposeAsync();
                fs.Close();
                progress?.Report($"Map generator [{version}] downloaded");
            }
        }

        public async Task<string[]> GenerateMapAsync(
            string version,
            string folder = null,
            string seed = null,
            string mapname = null,
            string style = null,
            string biome = null,
            int? spawns = null,
            int? teams = null,
            double? landDensity = null,
            double? plateauDensity = null,
            double? mountainDensity = null,
            double? rampDensity = null,
            double? reclaimDensity = null,
            double? mexDensity = null,
            int? mexsCount = null,
            int? mapsize = null,
            bool isTournament = false,
            bool isBlind = false,
            bool isUnexplored = false,
            int generateCount = 1,
            IProgress<string> progress = null,
            CancellationToken cancellationToken = default
            )
        {
            // TODO to other method
            await ConfirmOrDownloadAsync(version, progress);


            var process = GetProcessByVersion(version);
            var args = process.StartInfo.Arguments;

            if (isTournament) args += MapGeneratorArguments.TournamentStyle;
            else if (isBlind) args += MapGeneratorArguments.Blind;
            else if (isUnexplored) args += MapGeneratorArguments.Unexplored;

            if (style is not null) args += MapGeneratorArguments.SetStyle(style);
            if (biome is not null) args += MapGeneratorArguments.SetBiome(biome);
            if (spawns is not null) args += MapGeneratorArguments.SetSpawnCount(spawns.Value);
            if (teams is not null) args += MapGeneratorArguments.SetTeamsCount(spawns.Value);

            if (landDensity is not null) args += MapGeneratorArguments.SetLandDensity(landDensity.Value);
            if (plateauDensity is not null) args += MapGeneratorArguments.SetPlateauDensity(plateauDensity.Value);
            if (mountainDensity is not null) args += MapGeneratorArguments.SetMountainDensity(mountainDensity.Value);
            if (rampDensity is not null) args += MapGeneratorArguments.SetRampDensity(rampDensity.Value);
            if (reclaimDensity is not null) args += MapGeneratorArguments.SetReclaimDensity(reclaimDensity.Value);
            if (mexDensity is not null) args += MapGeneratorArguments.SetMexsDensity(mexDensity.Value);
            if (mexsCount is not null) args += MapGeneratorArguments.SetMexsCountPerPlayer(mexsCount.Value);
            if (mapsize is not null) args += MapGeneratorArguments.SetMapSie(mapsize.Value);

            if (folder is not null) args += MapGeneratorArguments.SetFolderPath(folder);
            args += MapGeneratorArguments.SetCountToGenerate(generateCount);

            process.StartInfo.Arguments = args;

            folder ??= Environment.CurrentDirectory;
            var maps = new string[generateCount];

            int i = 0;
            while (!process.StandardOutput.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var output = await process.StandardOutput.ReadLineAsync();
                progress?.Report(output);
                if (Pattern.IsMatch(output))
                {
                    // map scenario
                    var scenario = output + "_scenario.lua";
                    var file = Path.Combine(folder, scenario);
                    maps[i] = file;
                    MapGenerated?.Invoke(this, file);
                    i++;
                }
            }

            if (!process.HasExited)
            {
                process.Close();
                process.Dispose();
            }
            return maps;
        }
    }
}
