using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Models;
using FAF.Domain.LobbyServer.Enums;
using Meziantou.Framework.WPF.Collections;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public abstract class MapsHostingVM: JsonSettingsViewModel
    {
        //protected readonly LobbyClient LobbyClient;
        protected readonly ContainerViewModel Container;
        //protected readonly PatchClient PatchClient;
        //protected readonly IceManager IceManager;
        protected readonly NotificationService NotificationService;
        protected readonly IConfiguration Configuration;

        protected MapsHostingVM(ContainerViewModel container, IConfiguration configuration, NotificationService notificationService,
            ServerManager serverManager)
        {
            LocalMaps = new();
            MapsSource = new()
            {
                Source = LocalMaps.AsObservable
            };
            MapsSource.Filter += MapsSource_Filter;
            //LobbyClient = lobbyClient;
            Container = container;
            //PatchClient = patchClient;
            //IceManager = iceManager;
            Configuration = configuration;
            NotificationService = notificationService;
            SelectedServer = serverManager;
        }

        #region FilterText
        private string _FilterText;
        public string FilterText
        {
            get => _FilterText;
            set
            {
                if (Set(ref _FilterText, value))
                {
                    MapsView.Refresh();
                }
            }
        }
        #endregion

        private void MapsSource_Filter(object sender, FilterEventArgs e)
        {
            var map = (LocalMap)e.Item;
            if (!string.IsNullOrWhiteSpace(FilterText))
            {
                e.Accepted = map.Scenario.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
            }
        }

        #region SelectedServer
        private ServerManager _SelectedServer;
        public ServerManager SelectedServer
        {
            get => _SelectedServer;
            set => Set(ref _SelectedServer, value);
        }
        #endregion

        

        #region Game
        private GameHostingModel _Game;
        public GameHostingModel Game
        {
            get => _Game;
            set => Set(ref _Game, value);
        }
        #endregion

        private readonly CollectionViewSource MapsSource;
        public ICollectionView MapsView => MapsSource?.View;
        protected bool IsRefresh;

        protected ConcurrentObservableCollection<LocalMap> LocalMaps;

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
            //Container.SplashVisibility = Visibility.Visible;
            //Container.SplashProgressVisibility = Visibility.Visible;
            //Container.SplashText = "Confirming patch";
            //Container.Content = null;
            var progress = new Progress<string>();
            //await SelectedServer?.GetPatchClient().ConfirmPatchAsync(Game.FeaturedMod, 0, false, cancellationTokenSource.Token, progress)
            //.ContinueWith(t =>
            //{
            //    //Container.SplashProgressVisibility = Visibility.Collapsed;
            //    //Container.SplashVisibility = Visibility.Collapsed;
            //    if (t.IsFaulted)
            //    {
            //        NotificationService.Notify("Error", t.Exception.ToString());
            //        return;
            //    }
            //    SelectedServer.GetLobbyClient().HostGame(
            //    title: Game.Title,
            //    mod: Game.FeaturedMod,
            //    mapName: LocalMap.FolderName,
            //    minRating: Game.MinimumRating,
            //    maxRating: Game.MaximumRating,
            //    visibility: Game.IsFriendsOnly ?
            //    GameVisibility.Friends :
            //    GameVisibility.Public,
            //    isRatingResctEnforced: Game.EnforceRating,
            //    password: Game.Password);
            //});
        });
        #endregion

        protected bool Disposed;
        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;

            Disposed = true;
            _HostGameCommand = null;
            LocalMaps = null;
            LocalMap = null;
        }

        protected void SetSource(IEnumerable<LocalMap> source)
        {
            LocalMaps.Clear();
            LocalMaps.AddRange(source);
            //App.Current.Dispatcher.Invoke(UpdateMapsSource, System.Windows.Threading.DispatcherPriority.Background);
        }
        protected void AddMap(LocalMap map)
        {
            if (Disposed) return;
             LocalMaps.Add(map);
        }
        protected void AddRange(IEnumerable<LocalMap> maps)
        {
            if (Disposed) return;
            LocalMaps.AddRange(maps);
        }
    }
}
