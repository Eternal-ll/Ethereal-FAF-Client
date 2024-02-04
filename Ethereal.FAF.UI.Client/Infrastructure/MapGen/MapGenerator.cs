using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Progress;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.MapGen
{
    public record struct NeroxisMapInfo(string Raw, string Version, string Seed);
    public sealed class MapGenerator : INeroxisMapGenerator
    {
        public event EventHandler<string> MapGenerated;
        public List<string> KnownVersions = new List<string>();

        private readonly IJavaRuntime _javaRuntime;
        private readonly ISettingsManager _settingsManager;
        private readonly IDownloadService _downloadService;
        private readonly ILogger<MapGenerator> Logger;

        public static string LatestVersion = "1.11.0";

        private readonly static Regex Pattern = new Regex(@"neroxis_map_generator_(\d+\.\d+\.\d+)_(.*)");
        private const string LogsEnvironmentVariable = "LOG_DIR";

        public string[] TerrainSymmetries => new string[]
        {
            string.Empty, "POINT2", "POINT3", "POINT4", "POINT5", "POINT6", "POINT7", "POINT8",
            "POINT9", "POINT10", "POINT11", "POINT12", "POINT13", "POINT14", "POINT15",
            "POINT16", "XZ", "ZX", "X", "Z", "QUAD", "DIAG", "NONE"
        };

        public MapGenerator(ILogger<MapGenerator> logger, IDownloadService downloadService, ISettingsManager settingsManager, IJavaRuntime javaRuntime)
        {
            Logger = logger;
            _downloadService = downloadService;
            _settingsManager = settingsManager;
            _javaRuntime = javaRuntime;
        }
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            KnownVersions.Clear();
            var location = GetMapGeneratorVersionsLocation();
            if (!Directory.Exists(location))
            {
                Directory.CreateDirectory(location);
                await ConfirmOrDownloadAsync(LatestVersion);
            }
            foreach (var file in Directory.GetFiles(location, "MapGenerator_*.*.*.jar", SearchOption.AllDirectories))
            {
                var version = Path.GetFileNameWithoutExtension(file).Split('_')[^1];
                if (!KnownVersions.Any(v => v == version)) KnownVersions.Add(version);
            }
        }

        public string GetMapGeneratorVersionsLocation()
        {
            return Path.Combine(_settingsManager.Settings.FAForeverLocation, "map_generator");
        }

        private string MapGeneratorFile(string version) => Path.Combine(GetMapGeneratorVersionsLocation(), $"MapGenerator_{version}.jar");

        private Process GetProcess(string path)
        {
            return null;
        }

        private Process GetProcessByVersion(string version)
        {
            var mapgen = MapGeneratorFile(version);
            var process = GetProcess(mapgen);
            return process;
        }

        public static bool IsGeneratedMap(string map) => Pattern.IsMatch(map);
        public static NeroxisMapInfo Parse(string data)
        {
            if (!IsGeneratedMap(data)) return new();
            var result = Pattern.Match(data);
            if (result.Groups.Count == 3)
            {
                return new(data, result.Groups[1].Value, result.Groups[2].Value);
            }
            return new();
        }

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

        public async Task ConfirmOrDownloadAsync(string version, IProgress<ProgressReport> progress = null)
        {
            if (!KnownVersions.Any(v => v == version)) KnownVersions.Add(version);
            var mapgen = MapGeneratorFile(version);
            //progress?.Report($"Confirming map generator [{version}]");
            if (File.Exists(mapgen)) return;
            var location = GetMapGeneratorVersionsLocation();
            if (!Directory.Exists(location))
            {
                Logger.LogInformation("Creating directory [{location}] for Map Generator [{version}]", location, version);
                Directory.Exists(location);
            }
            var baseAddress = "https://github.com/FAForever/Neroxis-Map-Generator/releases/download/";
            var url = baseAddress + $"{version}/NeroxisGen_{version}.jar";
            await _downloadService.DownloadToFileAsync(url, mapgen, progress);
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
            if (mapname is not null)
            {
                var data = Pattern.Match(mapname);
                version = data.Groups[1].Value;
            }
            // TODO to other method
            await ConfirmOrDownloadAsync(version);

            var process = GetProcessByVersion(version);
            var args = process.StartInfo.Arguments;
            //if (!string.IsNullOrWhiteSpace(PreviewPath))
            //{
            //    args += MapGeneratorArguments.SetPreviewPath(PreviewPath);
            //}

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

            //if (string.IsNullOrWhiteSpace(folder))
            //{
            //    folder = Configuration.GetMapsLocation();
            //}
            if (!Directory.Exists(folder))
            {
                Logger.LogInformation("Creating directory [{location}] for generated map", folder);
                Directory.CreateDirectory(folder);
            }
            args += MapGeneratorArguments.SetFolderPath(folder);
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
                    {
                        MapGenerated?.Invoke(this, file);
                    }
                    latest = file;
                }
            }
            if (latest is not null)
            {
                MapGenerated?.Invoke(this, latest);
            }

            if (!process.HasExited)
            {
                process.Kill();
                process.Dispose();
            }
            return maps.ToArray();
        }

        public bool IsNeroxisGeneratedMap(string map) => Pattern.IsMatch(map);

        public async Task GenerateMapAsync(string name, string folder, IProgress<ProgressReport> progress = null, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var data = Parse(name);
            var mapgenVersion = data.Version;

            var javaRuntime = await _javaRuntime.EnsurJavaRuntimeExist(progress, cancellationToken);
            await ConfirmOrDownloadAsync(mapgenVersion, progress);
            var mapgen = MapGeneratorFile(mapgenVersion);
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = javaRuntime,
                    Arguments = $"-jar \"{mapgen}\" ",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            //process.StartInfo.EnvironmentVariables.Add(LogsEnvironmentVariable, Configuration.GetMapGeneratorLoggingLocation());
            var args = process.StartInfo.Arguments;
            args += MapGeneratorArguments.SetMapName(name);
            args += MapGeneratorArguments.SetFolderPath(folder);
            //if (!string.IsNullOrWhiteSpace(PreviewPath))
            //{
            //    args += MapGeneratorArguments.SetPreviewPath(PreviewPath);
            //}
            process.StartInfo.Arguments = args;
            process.Start();
            if (progress != null)
            {
                while (!process.HasExited)
                {
                    var output = await process.StandardOutput.ReadLineAsync();
                    progress.Report(new(-1, "Neroxis Map Generator", output, true));
                }
            }
            await process.WaitForExitAsync(cancellationToken);
        }
    }
}
