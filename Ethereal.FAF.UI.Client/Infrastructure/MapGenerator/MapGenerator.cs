using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    public class MapGenerator
    {
        public event EventHandler<string> MapGenerated;
        public List<string> KnownVersions = new List<string>();

        private readonly string JavaRuntime;
        private readonly string Logging;
        private readonly string PreviewPath;
        private readonly string MapGeneratorsFolder;
        private readonly string GeneratedMapsFolder;
        private readonly string MapGeneratorRepository;

        private readonly IHttpClientFactory HttpClientFactory;
        private readonly ILogger Logger;

        private static Regex Pattern = new Regex(@"neroxis_map_generator_(\d+\.\d+\.\d+)_(.*)");

        public string[] TerrainSymmetries => new string[]
        {
            string.Empty, "POINT2", "POINT3", "POINT4", "POINT5", "POINT6", "POINT7", "POINT8",
            "POINT9", "POINT10", "POINT11", "POINT12", "POINT13", "POINT14", "POINT15",
            "POINT16", "XZ", "ZX", "X", "Z", "QUAD", "DIAG", "NONE"
        };

        public MapGenerator(string javaRuntime, string logging, string previewPath, string mapGeneratorsFolder, string mapGeneratorRepository, string generatedMapsFolder, IHttpClientFactory httpClientFactory, ILogger logger)
        {
            JavaRuntime = javaRuntime;
            Logging = logging;
            PreviewPath = previewPath;
            MapGeneratorsFolder = mapGeneratorsFolder;
            GeneratedMapsFolder = generatedMapsFolder;
            HttpClientFactory = httpClientFactory;
            MapGeneratorRepository = mapGeneratorRepository;
            Logger = logger;

            var files = Directory.GetFiles(mapGeneratorsFolder, "MapGenerator_*.*.*.jar", SearchOption.AllDirectories);
            foreach (var mapgen in files)
            {
                var version = mapgen.Split('/')[^1].Split('_')[^1].Replace(".jar", null);
                if (!KnownVersions.Any(v => v == version)) KnownVersions.Add(version);
            }
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
            args += MapGeneratorArguments.SetFolderPath(GeneratedMapsFolder ?? targetFolder);
            if (!string.IsNullOrWhiteSpace(PreviewPath))
            {
                args += MapGeneratorArguments.SetPreviewPath(PreviewPath);
            }
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

        public async Task<List<string>> GetStylesAsync(string version)
        {
            var process = GetProcessByVersion(version);
            process.StartInfo.Arguments += MapGeneratorArguments.Styles;
            var styles = new List<string>();
            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                var output = await process.StandardOutput.ReadLineAsync();
                if (!output.Contains(' ')) styles.Add(output);
            }
            return styles;
        }
        public async Task<string[]> GetStylesWeightsAsync(string version)
        {
            var process = GetProcessByVersion(version);
            process.StartInfo.Arguments += MapGeneratorArguments.Weights;
            var styles = new List<string>();
            process.Start();
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
            process.StartInfo.Arguments += MapGeneratorArguments.Biomes;
            var styles = new List<string>();
            process.Start();
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
            if (!KnownVersions.Any(v => v == version)) KnownVersions.Add(version);
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
            string version = "1.8.6",
            string folder = null,
            string seed = null,
            string mapname = null,
            string style = null,
            string biome = null,
            string visibility = null,
            string terrainSymmetry = null,
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
            int generateCount = 1,
            IProgress<string> progress = null,
            CancellationToken cancellationToken = default
            )
        {
            // TODO to other method
            await ConfirmOrDownloadAsync(version, progress);

            var process = GetProcessByVersion(version);
            var args = process.StartInfo.Arguments;
            if (!string.IsNullOrWhiteSpace(PreviewPath))
            {
                args += MapGeneratorArguments.SetPreviewPath(PreviewPath);
            }
            
            int.TryParse(version[^3].ToString(), out var minor);
            int.TryParse(version[^1].ToString(), out var bug);

            if (mapname is not null) args += MapGeneratorArguments.SetMapName(mapname);
            else if (visibility is not null)
            {
                if (bug <= 5)
                {
                    if (visibility.Equals("tournament", StringComparison.OrdinalIgnoreCase)) args += MapGeneratorArguments.TournamentStyle;
                    else if (visibility.Equals("blind", StringComparison.OrdinalIgnoreCase)) args += MapGeneratorArguments.Blind;
                    else if (visibility.Equals("unexplored", StringComparison.OrdinalIgnoreCase)) args += MapGeneratorArguments.Unexplored;
                }
                else
                {
                    args += MapGeneratorArguments.SetVisibility(visibility);
                }
            }
            else if (!string.IsNullOrWhiteSpace(style)) args += MapGeneratorArguments.SetStyle(style);
            else
            {
                if (biome is not null) args += MapGeneratorArguments.SetBiome(biome);
                if (terrainSymmetry is not null) args += MapGeneratorArguments.SetTerrainSymmetry(terrainSymmetry);

                if (landDensity is not null) args += MapGeneratorArguments.SetLandDensity(landDensity.Value);
                if (plateauDensity is not null) args += MapGeneratorArguments.SetPlateauDensity(plateauDensity.Value);
                if (mountainDensity is not null) args += MapGeneratorArguments.SetMountainDensity(mountainDensity.Value);
                if (rampDensity is not null) args += MapGeneratorArguments.SetRampDensity(rampDensity.Value);
                if (reclaimDensity is not null) args += MapGeneratorArguments.SetReclaimDensity(reclaimDensity.Value);
                if (mexDensity is not null) args += MapGeneratorArguments.SetMexsDensity(mexDensity.Value);
                if (mexsCount is not null && bug <= 5 && minor == 8) args += MapGeneratorArguments.SetMexsCountPerPlayer(mexsCount.Value);
            }
            if (spawns is not null) args += MapGeneratorArguments.SetSpawnCount(spawns.Value);
            if (teams is not null) args += MapGeneratorArguments.SetTeamsCount(teams.Value);
            if (mapsize is not null && mapsize != 0) args += MapGeneratorArguments.SetMapSie(mapsize.Value);

            if (folder is null) args += MapGeneratorArguments.SetFolderPath(GeneratedMapsFolder);
            else args += MapGeneratorArguments.SetFolderPath(folder);
            args += MapGeneratorArguments.SetCountToGenerate(generateCount);

            process.StartInfo.Arguments = args;
            var maps = new List<string>();

            process.Start();
            string latest = null;
            while (!process.StandardOutput.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var output = await process.StandardOutput.ReadLineAsync();
                progress?.Report(output);
                if (output.Contains("Saving")) continue;
                if (Pattern.IsMatch(output))
                {
                    // map scenario
                    var scenario = output + '/' + output + "_scenario.lua";
                    var file = Path.Combine(folder, scenario);
                    if (maps.Any(m => m == file)) continue;
                    maps.Add(file);
                    if (latest is not null) 
                        MapGenerated?.Invoke(this, file);
                    latest = file;
                }
            }
            if (latest is not null) MapGenerated?.Invoke(this, latest);

            if (!process.HasExited)
            {
                process.Kill();
                process.Dispose();
            }
            return maps.ToArray();
        }
    }
}
