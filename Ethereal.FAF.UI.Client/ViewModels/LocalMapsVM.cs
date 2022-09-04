using Ethereal.FA.Scmap;
using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using FAF.Domain.LobbyServer.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class LocalMap
    {
        public string FolderName { get; set; }
        public MapScenario Scenario { get; set; }
        public string Preview { get; set; }
    }
    public abstract class MapsHostingVM: Base.ViewModel
    {

        #region Game
        private GameHostingModel _Game;
        public GameHostingModel Game
        {
            get => _Game;
            set => Set(ref _Game, value);
        }
        #endregion

        public CollectionViewSource MapsSource { get; private set; }
        public ICollectionView MapsView => MapsSource?.View;
        protected bool IsRefresh;

        #region LocalMaps
        private ObservableCollection<LocalMap> _LocalMaps;
        public ObservableCollection<LocalMap> LocalMaps
        {
            get => _LocalMaps;
            set
            {
                if (Set(ref _LocalMaps, value))
                {
                    MapsSource = new()
                    {
                        Source = value
                    };
                    //MapsView.CurrentChanged += GamesView_CurrentChanged;
                    //MapsSource.Filter += GamesSource_Filter;
                    IsRefresh = true;
                    OnPropertyChanged(nameof(MapsView));
                    IsRefresh = false;
                }
            }
        }
        #endregion

        #region LocalMap
        private LocalMap _LocalMap;
        public LocalMap LocalMap
        {
            get => _LocalMap;
            set
            {
                if (Set(ref _LocalMap, value))
                {
                    Game.Map = LocalMap.FolderName;
                }
            }
        }
        #endregion


        #region HostGameCommand
        private ICommand _HostGameCommand;
        public ICommand HostGameCommand => _HostGameCommand ??= new LambdaCommand(OnHostGameCommand, CanHostGameCommand);
        protected abstract bool CanHostGameCommand(object obj);
        protected abstract void OnHostGameCommand(object obj);
        #endregion

        protected override void Dispose(bool disposing)
        {
            _HostGameCommand = null;
            LocalMaps = null;
            LocalMap = null;
        }

        protected void SetSource(IEnumerable<LocalMap> source) => App.Current.Dispatcher.Invoke(() =>
                                               LocalMaps = new(source), System.Windows.Threading.DispatcherPriority.Background);
        protected void AddMap(LocalMap map) => App.Current.Dispatcher.Invoke(() =>
                                               LocalMaps.Add(map), System.Windows.Threading.DispatcherPriority.Background);
    }
    public class LocalMapsVM : MapsHostingVM
    {
        private readonly string MapsDirectory;
        private readonly string SmallMapsPreviewsFolder;

        private readonly LobbyClient LobbyClient;

        public LocalMapsVM(string mapsDirectory, string smallMapsPreviewsFolder, LobbyClient lobbyClient)
        {
            MapsDirectory = mapsDirectory;
            SmallMapsPreviewsFolder = smallMapsPreviewsFolder;
            //MapsDirectory = new DirectoryInfo(mapsDirectory);

            Task.Run(() =>
            {
                var maps = new List<LocalMap>();
                string[] scenarios = Directory.GetFiles(mapsDirectory, "*_scenario.lua", SearchOption.AllDirectories);
                foreach (var file in scenarios)
                {
                    var map = new LocalMap();
                    map.FolderName = file.Split('/', '\\')[^2];
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
            LobbyClient = lobbyClient;
        }

        protected override bool CanHostGameCommand(object obj) => true;
        protected override void OnHostGameCommand(object obj) => LobbyClient.HostGame(
            title: Game.Title,
            mod: FeaturedMod.FAF,
            mapName: Game.Map,
            minRating: Game.MinimumRating,
            maxRating: Game.MaximumRating,
            visibility: Game.IsFriendsOnly ?
            GameVisibility.Friends :
            GameVisibility.Public,
            isRatingResctEnforced: Game.EnforeRating,
            password: Game.Password);
    }
}
