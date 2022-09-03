using AsyncAwaitBestPractices.MVVM;
using Ethereal.FA.Scmap;
using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Models;
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

        public GenerateMapsVM(LobbyClient lobbyClient, MapGenerator mapGenerator, string mapsFolder)
        {
            LobbyClient = lobbyClient;
            MapGenerator = mapGenerator;
            MapsFolder = mapsFolder;
            mapGenerator.MapGenerated += MapGenerator_MapGenerated;
            SelectedMapGeneratorVersion = "1.8.5";
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
                LocalMaps = maps;
            });
        }

        private void MapGenerator_MapGenerated(object sender, string e) => ProcessGeneratedMap(e);

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
            LocalMaps.Add(map);
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

        public string[] KnownVersions => MapGenerator.KnownVersions.ToArray();
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
                            Styles = await MapGenerator.GetStylesAsync(value);
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

        #region Biomes
        private string[] _Biomes;
        public string[] Biomes
        {
            get => _Biomes;
            set => Set(ref _Biomes, value);
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
            set => Set(ref _SelectedStyle, value);
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

        #region MapSize
        private double _MapSize;
        public double MapSize
        {
            get => _MapSize;
            set => Set(ref _MapSize, value);
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

        public static string[] GenerationStyles => new string[]
        {
            "Casual", "Tournament", "Blind", "Unexplorer"
        };

        #region SelectedGenerationStyle
        private string _SelectedGenerationStyle;
        public string SelectedGenerationStyle
        {
            get => _SelectedGenerationStyle;
            set
            {
                if (Set(ref _SelectedGenerationStyle, value))
                {
                    BiomeEnabled = value == "Casual";
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
        private AsyncCommand _GenerateCommand;
        public AsyncCommand GenerateCommand => _GenerateCommand ??= new AsyncCommand(OnGenerateCommandAsync, CanGenerateCommand);

        private bool CanGenerateCommand(object arg) => !IsGenerating;

        private async Task OnGenerateCommandAsync()
        {
            IsGenerating = true;
            CancellationSource = new();

            var land = MapDensities["Land"];
            var plateau = MapDensities["Plateau"];
            var mountain = MapDensities["Mountain"];
            var ramp = MapDensities["Ramp"];
            var reclaim = MapDensities["Reclaim"];
            var mex = MapDensities["Mex"];

            var maps = await MapGenerator.GenerateMapAsync(
                version: SelectedMapGeneratorVersion,
                folder: MapsFolder,
                style: SelectedStyle,
                biome: SelectedBiome,
                spawns: SpawnsCount,
                teams: TeamsCount,
                landDensity: land.IsEnabled ? land.Value : null,
                plateauDensity: plateau.IsEnabled ? plateau.Value : null,
                mountainDensity: mountain.IsEnabled ? mountain.Value : null,
                rampDensity: ramp.IsEnabled ? ramp.Value : null,
                reclaimDensity: reclaim.IsEnabled ? reclaim.Value : null,
                mexDensity: mex.IsEnabled ? mex.Value : null,
                mexsCount: MexsPerPlayer,
                mapsize: (int)MapSize,
                isTournament: SelectedGenerationStyle == "Tournament",
                isBlind: SelectedGenerationStyle == "Blind",
                isUnexplored: SelectedGenerationStyle == "Unexplored",
                generateCount: GenerateCount,

                cancellationToken : CancellationSource.Token);
            foreach (var map in maps)
            {
                ProcessGeneratedMap(map);
            }
            IsGenerating = false;
        }
        #endregion

        private CancellationTokenSource CancellationSource;
        #region CancelGenerationCommand
        private ICommand _CancelGenerationCommand;
        public ICommand CancelGenerationCommand => _CancelGenerationCommand ??= new LambdaCommand(OnCancelGenerationCommand, CanCancelGenerationCommand);

        private bool CanCancelGenerationCommand(object arg) => CancellationSource is not null && !CancellationSource.IsCancellationRequested;

        private void OnCancelGenerationCommand(object obj)
        {
            CancellationSource?.Cancel();
            IsGenerating = false;
        }
        #endregion

        protected override bool CanHostGameCommand(object obj)
        {
            return true;
        }

        protected override void OnHostGameCommand(object obj)
        {

        }
    }
}
