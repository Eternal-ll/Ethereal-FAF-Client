using AsyncAwaitBestPractices.MVVM;
using Ethereal.FA.Scmap;
using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class DensitySetting
    {
        public bool IsEnabled { get; set; }
        public double Value { get; set; }
    }
    public class GenerateMapsVM : MapsHostingVM
    {
        private readonly LobbyClient LobbyClient;
        private readonly MapGenerator MapGenerator;
        private readonly string MapsFolder;

        private FileSystemWatcher FileSystemWatcher;

        public GenerateMapsVM(LobbyClient lobbyClient, MapGenerator mapGenerator, string mapsFolder)
        {
            LobbyClient = lobbyClient;
            MapGenerator = mapGenerator;
            MapsFolder = mapsFolder;
            FileSystemWatcher = new()
            {
                IncludeSubdirectories = true,
                Path = mapsFolder,
                Filter = "neroxis_map_generator_*.*.*_*_scenario.lua",
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };
            FileSystemWatcher.Changed += (s, e) =>
            {
                Task.Run(async () =>
                {
                    await Task.Delay(500);
                    ProcessGeneratedMap(e.FullPath);
                });
            };
            mapGenerator.MapGenerated += MapGenerator_MapGenerated;
            SelectedMapGeneratorVersion = "1.8.6";
            Task.Run(() =>
            {
                var maps = new List<LocalMap>();
                string[] scenarios = Directory.GetFiles(mapsFolder, "*_scenario.lua", SearchOption.AllDirectories);
                foreach (var file in scenarios)
                {
                    var name = file.Split('/', '\\')[^2];
                    if (!MapGenerator.IsNeroxisMap(name)) continue;
                    var map = new LocalMap();
                    map.FolderName = name;
                    map.Scenario = MapScenario.FromFile(file);
                    var preview = file.Replace("_scenario.lua", ".png");
                    var scmapPath = file.Replace("_scenario.lua", ".scmap");
                    map.Preview = preview;
                    if (!File.Exists(preview))
                    {
                        var scmap = Scmap.FromFile(scmapPath);
                    }
                    maps.Add(map);
                }
                SetSource(maps);
            });
        }

        private void MapGenerator_MapGenerated(object sender, string e)
        {

        }

        private void ProcessGeneratedMap(string scenario)
        {
            var name = scenario.Split('/', '\\')[^2];
            if (GeneratedMaps.Any(m => m == name)) return;
            GeneratedMaps.Add(name);
            var map = new LocalMap();
            map.FolderName = name;
            map.Scenario = MapScenario.FromFile(scenario);
            var preview = scenario.Replace("_scenario.lua", ".png");
            var scmapPath = scenario.Replace("_scenario.lua", ".scmap");
            map.Preview = preview;
            if (!File.Exists(preview))
            {
                var scmap = Scmap.FromFile(scmapPath);
            }
            AddMap(map);
        }

        public ObservableCollection<string> GeneratedMaps { get; set; } = new();

        #region DeleteGeneratedMaps
        private bool _DeleteGeneratedMaps;
        public bool DeleteGeneratedMaps
        {
            get => _DeleteGeneratedMaps;
            set => Set(ref _DeleteGeneratedMaps, value);
        }
        #endregion

        public string[] KnownVersions => MapGenerator.KnownVersions.OrderByDescending(s => s).ToArray();
        #region SelectedMapGeneratorVersion
        private string _SelectedMapGeneratorVersion;
        public string SelectedMapGeneratorVersion
        {
            get => _SelectedMapGeneratorVersion;
            set
            {
                if (Set(ref _SelectedMapGeneratorVersion, value))
                {
                    if (value is not null)
                    {
                        Task.Run(async () =>
                        {
                            IsGenerating = true;
                            if (!MapGenerator.IsMapGeneratorExist(value))
                            {
                                ProgressText = $"Downloading Map Generatior {value}";
                                await MapGenerator.ConfirmOrDownloadAsync(value);
                            }
                            ProgressText = $"Loading biomes...";
                            Biomes = await MapGenerator.GetBiomesAsync(value);
                            ProgressText = $"Loading styles...";
                            var styles = await MapGenerator.GetStylesAsync(value);
                            styles.Insert(0, string.Empty);
                            Styles = styles.ToArray();
                            IsGenerating = false;
                            ProgressText = null;
                        });
                    }
                }
            }
        }
        #endregion

        #region ProgressText
        private string _ProgressText;
        public string ProgressText
        {
            get => _ProgressText;
            set => Set(ref _ProgressText, value);
        }
        #endregion

        public string[] SelectedTerrainSymmetrySource => MapGenerator.TerrainSymmetries;
        #region SelectedTerrainSymmetry
        private string _SelectedTerrainSymmetry;
        public string SelectedTerrainSymmetry
        {
            get => _SelectedTerrainSymmetry;
            set => Set(ref _SelectedTerrainSymmetry, value);
        }
        #endregion

        #region Biomes
        private string[] _Biomes;
        public string[] Biomes
        {
            get => _Biomes;
            set
            {
                if (Set(ref _Biomes, value))
                {
                    if (value is not null)
                    {
                        if (value.Length == 0)
                        {
                            _Biomes = new string[]
                            {
                                "Brimstone",
                                "Desert",
                                "EarlyAutumn",
                                "Frithen",
                                "Loki",
                                "Mars",
                                "Moonlight",
                                "Prayer",
                                "Stones",
                                "Syrtis",
                                "WindingRiver",
                                "Wonder",
                            };
                            OnPropertyChanged(nameof(Biomes));
                        }
                        SelectedBiome = _Biomes[0];
                    }
                }
            }
        }
        #endregion

        #region SelectedBiome
        private string _SelectedBiome;
        public string SelectedBiome
        {
            get => _SelectedBiome;
            set => Set(ref _SelectedBiome, value);
        }
        #endregion

        #region BiomeEnabled
        private bool _BiomeEnabled;
        public bool BiomeEnabled
        {
            get => _BiomeEnabled;
            set => Set(ref _BiomeEnabled, value);
        }
        #endregion

        #region Styles
        private string[] _Styles;
        public string[] Styles
        {
            get => _Styles;
            set => Set(ref _Styles, value);
        }
        #endregion

        #region SelectedStyle
        private string _SelectedStyle;
        public string SelectedStyle
        {
            get => _SelectedStyle;
            set
            {
                if (Set(ref _SelectedStyle, value))
                {
                    SelectedMapVisibility = !string.IsNullOrWhiteSpace(value) ? null : "Casual";
                }
            }
        }
        #endregion

        public int[] MexsPerPlayerSource => Enumerable.Range(0, 32).ToArray();
        #region MexsPerPlayer
        private int _MexsPerPlayer = 6;
        public int MexsPerPlayer
        {
            get => _MexsPerPlayer;
            set => Set(ref _MexsPerPlayer, value);
        }
        #endregion

        public int[] SpawnsCountSource => Enumerable.Range(0, 17).ToArray();
        #region SpawnsCount
        private int _SpawnsCount = 8;
        public int SpawnsCount
        {
            get => _SpawnsCount;
            set => Set(ref _SpawnsCount, value);
        }
        #endregion

        public int[] TeamsCountSource => Enumerable.Range(0, 17).ToArray();
        #region TeamsCount
        private int _TeamsCount = 2;
        public int TeamsCount
        {
            get => _TeamsCount;
            set => Set(ref _TeamsCount, value);
        }
        #endregion

        public int SizeInPixels => _MapSize % 1.25 != 0 ? (int)((_MapSize + 0.625) / 1.25 * 1.25) : (int)(_MapSize * 51.2);
        public double[] MapSizeSource => new double[] { 5, 7.5, 10, 12.5, 15, 17.5, 20 };
        #region MapSize
        private double _MapSize = 10;
        public double MapSize
        {
            get => _MapSize;
            set
            {
                if (Set(ref _MapSize, value))
                {
                    OnPropertyChanged(nameof(SizeInPixels));
                }
            }
        }
        #endregion

        public int[] GenerateCountSource => Enumerable.Range(0, 10).ToArray();
        #region GenerateCount
        private int _GenerateCount = 1;
        public int GenerateCount
        {
            get => _GenerateCount;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                _GenerateCount = value;
                OnPropertyChanged(nameof(GenerateCount));
            }
        }
        #endregion

        public ObservableDictionary<string, DensitySetting> MapDensities { get; set; }
            = new ObservableDictionary<string, DensitySetting>()
        {
            { "Land", new() },
            { "Plateau", new() },
            { "Mountain", new() },
            { "Ramp", new() },
            { "Reclaim", new() },
            { "Mex", new() },
        };

        public static string[] MapVisibilities => new string[]
        {
            "Casual", "Tournament", "Blind", "Unexplored"
        };

        #region SelectedGenerationStyle
        private string _SelectedGenerationStyle;
        public string SelectedMapVisibility
        {
            get => _SelectedGenerationStyle;
            set
            {
                if (Set(ref _SelectedGenerationStyle, value))
                {
                    BiomeEnabled = value == "Casual";
                    SelectedStyle = !string.IsNullOrWhiteSpace(value) ? "" : SelectedStyle;
                }
            }
        }
        #endregion

        #region MapName
        private string _MapName;
        public string MapName
        {
            get => _MapName;
            set => Set(ref _SelectedMapGeneratorVersion, value);
        }
        #endregion

        public Visibility FormVisibility => IsGenerating ? Visibility.Collapsed : Visibility.Visible;
        public Visibility LoaderVisibility => IsGenerating ? Visibility.Visible : Visibility.Collapsed;

        #region IsGenerating
        private bool _IsGenerating;
        public bool IsGenerating
        {
            get => _IsGenerating;
            set
            {
                if (Set(ref _IsGenerating, value))
                {
                    OnPropertyChanged(nameof(FormVisibility));
                    OnPropertyChanged(nameof(LoaderVisibility));
                }
            }
        }
        #endregion

        #region GenerateCommand
        private ICommand _GenerateCommand;
        public ICommand GenerateCommand => _GenerateCommand ??= new LambdaCommand(OnGenerateCommand, CanGenerateCommand);

        private bool CanGenerateCommand(object arg) => true;

        private async void OnGenerateCommand(object arg) => await Task.Run(async () =>
        {
            IsGenerating = true;
            CancellationSource = new();
            ProgressText = "Generating...";
            var land = MapDensities["Land"];
            var plateau = MapDensities["Plateau"];
            var mountain = MapDensities["Mountain"];
            var ramp = MapDensities["Ramp"];
            var reclaim = MapDensities["Reclaim"];
            var mex = MapDensities["Mex"];
            var progress = new Progress<string>(d => ProgressText = d);

            var maps = await MapGenerator.GenerateMapAsync(
                version: SelectedMapGeneratorVersion,
                folder: MapsFolder,
                style: SelectedStyle,
                biome: SelectedBiome,
                spawns: SpawnsCount,
                visibility: SelectedMapVisibility is not "Casual" ? SelectedMapVisibility : null,
                teams: TeamsCount,
                landDensity: land.IsEnabled ? land.Value : null,
                plateauDensity: plateau.IsEnabled ? plateau.Value : null,
                mountainDensity: mountain.IsEnabled ? mountain.Value : null,
                rampDensity: ramp.IsEnabled ? ramp.Value : null,
                reclaimDensity: reclaim.IsEnabled ? reclaim.Value : null,
                mexDensity: mex.IsEnabled ? mex.Value : null,
                mexsCount: MexsPerPlayer,
                mapsize: SizeInPixels,
                generateCount: GenerateCount,
                progress: progress,
                cancellationToken: CancellationSource.Token);
            foreach (var map in maps)
            {
                //if (!string.IsNullOrWhiteSpace(map))
                    //ProcessGeneratedMap(map);
            }
            ProgressText = null;
            IsGenerating = false;
        }).ConfigureAwait(false);

        #endregion

        private CancellationTokenSource CancellationSource;
        #region CancelGenerationCommand
        private ICommand _CancelGenerationCommand;
        public ICommand CancelGenerationCommand => _CancelGenerationCommand ??= new LambdaCommand(OnCancelGenerationCommand, CanCancelGenerationCommand);

        private bool CanCancelGenerationCommand(object arg) => CancellationSource is not null && !CancellationSource.IsCancellationRequested;

        private void OnCancelGenerationCommand(object obj)
        {
            CancellationSource?.Cancel();
        }
        #endregion

        #region CleanCommand
        private ICommand _CleanCommand;
        public ICommand CleanCommand => _CleanCommand ??= new LambdaCommand(OnCleanCommand, CanCleanCommand);

        private bool CanCleanCommand(object arg) => GeneratedMaps.Any();

        private void OnCleanCommand(object obj)
        {

        }
        #endregion

        protected override bool CanHostGameCommand(object obj)
        {
            return true;
        }

        protected override void OnHostGameCommand(object obj)
        {

        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            FileSystemWatcher.Dispose();
        }
    }
}
