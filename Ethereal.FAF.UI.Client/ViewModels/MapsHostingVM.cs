using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Models;
using FAF.Domain.LobbyServer.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public abstract class MapsHostingVM: Base.ViewModel
    {
        private readonly LobbyClient LobbyClient;
        private readonly ContainerViewModel Container;
        private readonly PatchClient PatchClient;
        private readonly IceManager IceManager;
        private readonly SnackbarService SnackbarService;

        protected MapsHostingVM(LobbyClient lobbyClient, ContainerViewModel container, PatchClient patchClient, IceManager iceManager)
        {
            LobbyClient = lobbyClient;
            Container = container;
            PatchClient = patchClient;
            IceManager = iceManager;
        }

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
        protected virtual bool CanHostGameCommand(object obj) => LocalMap is not null;
        protected virtual void OnHostGameCommand(object obj) => Task.Run(async () =>
        {
            var cancellationTokenSource = new CancellationTokenSource();
            Container.SplashVisibility = Visibility.Visible;
            Container.SplashProgressVisibility = Visibility.Visible;
            Container.SplashText = "Confirming patch";
            Container.Content = null;
            var progress = new Progress<string>(e => Container.SplashText = e);
            await PatchClient.UpdatePatch(Game.FeaturedMod, 0, false, cancellationTokenSource.Token, progress);
            Container.SplashProgressVisibility = Visibility.Collapsed;
            Container.SplashVisibility = Visibility.Collapsed;
            LobbyClient.HostGame(
            title: Game.Title,
            mod: Game.FeaturedMod,
            mapName: LocalMap.FolderName,
            minRating: Game.MinimumRating,
            maxRating: Game.MaximumRating,
            visibility: Game.IsFriendsOnly ?
            GameVisibility.Friends :
            GameVisibility.Public,
            isRatingResctEnforced: Game.EnforceRating,
            password: Game.Password);
        });
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
}
