using AsyncAwaitBestPractices.MVVM;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Views;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class PlayersViewModel : Base.ViewModel
    {
        public event EventHandler<GameInfoMessage> NewGameReceived;
        public event EventHandler<GameInfoMessage> GameUpdated;
        //public event EventHandler<GameInfoMessage> GameRemoved;

        public event EventHandler<GameInfoMessage> GameLaunched;
        public event EventHandler<GameInfoMessage> GameEnd;
        public event EventHandler<GameInfoMessage> GameClosed;

        public event EventHandler<string[]> PlayersLeftFromGame;
        public event EventHandler<KeyValuePair<GameInfoMessage, string[]>> PlayersJoinedToGame;

        public event EventHandler<KeyValuePair<GameInfoMessage, PlayerInfoMessage[]>> PlayersJoinedGame;
        public event EventHandler<KeyValuePair<GameInfoMessage, PlayerInfoMessage[]>> PlayersLeftGame;
        public event EventHandler<KeyValuePair<GameInfoMessage, PlayerInfoMessage[]>> PlayersFinishedGame;


        private readonly LobbyClient Lobby;

        public PlayersViewModel(LobbyClient lobby)
        {
            Lobby = lobby;
            lobby.PlayersReceived += Lobby_PlayersReceived;
            lobby.PlayerReceived += Lobby_PlayerReceived;
        }

        #region Games
        public CollectionViewSource PlayersSource { get; private set; }
        public ICollectionView PlayersView => PlayersSource?.View;
        private ObservableCollection<PlayerInfoMessage> _Players;
        public ObservableCollection<PlayerInfoMessage> Players
        {
            get => _Players;
            set
            {
                if (Set(ref _Players, value))
                {
                    PlayersSource = new()
                    {
                        Source = value
                    };
                    PlayersSource.Filter += PlayersSource_Filter;
                    OnPropertyChanged(nameof(PlayersView));
                }
            }
        }

        private void PlayersSource_Filter(object sender, FilterEventArgs e)
        {
            var player = (PlayerInfoMessage)e.Item;
            e.Accepted = false;
            e.Accepted = true;
        }
        #endregion

        private void Lobby_PlayerReceived(object sender, PlayerInfoMessage e)
        {
            throw new NotImplementedException();
        }

        private void Lobby_PlayersReceived(object sender, PlayerInfoMessage[] e) =>
            Application.Current.Dispatcher.Invoke(() =>
            {
                Players = new(e);
            }, DispatcherPriority.Background);
    }
    public class GamesViewModel : Base.ViewModel
    {
        private readonly LobbyClient Lobby;
        private readonly GameLauncher GameLauncher;
        private readonly DispatcherTimer Timer;
        private readonly IceManager IceManager;

        private readonly SnackbarService SnackbarService;
        private readonly ContainerViewModel ContainerViewModel;

        public GamesViewModel(LobbyClient lobby, GameLauncher gameLauncher, SnackbarService snackbarService, ContainerViewModel containerViewModel, IceManager iceManager)
        {
            Lobby = lobby;
            GameLauncher = gameLauncher;
            SnackbarService = snackbarService;
            lobby.GamesReceived += Lobby_GamesReceived;
            lobby.GameReceived += Lobby_GameReceived;
            //lobby.NotificationReceived += (s, e) => snackbarService.Show("Notification from server", e.text);

            GameLauncher.LeftFromGame += GameLauncher_LeftFromGame;

            SelectedGameMode = "Custom";

            Timer = new(interval: TimeSpan.FromSeconds(1), DispatcherPriority.Background, (s, e) =>
            {
                if (Games == null) return;
                foreach (var game in Games)
                {
                    if (game.LaunchedAt.HasValue)
                    {
                        game.OnPropertyChanged(nameof(game.HumanLaunchedAt));
                    }
                }
            }, Application.Current.Dispatcher);
            ContainerViewModel = containerViewModel;
            IceManager = iceManager;
        }

        private void GameLauncher_LeftFromGame(object sender, EventArgs e)
        {
            ContainerViewModel.SplashProgressVisibility = Visibility.Visible;
            ContainerViewModel.SplashVisibility = Visibility.Collapsed;
            ContainerViewModel.Content = null;
        }

        #region SelectedRatingType
        private RatingType _SelectedRatingType;
        public RatingType SelectedRatingType
        {
            get => _SelectedRatingType;
            set => Set(ref _SelectedRatingType, value);
        }
        #endregion
        #region SelectedGameType
        private GameType _SelectedGameType;
        public GameType SelectedGameType
        {
            get => _SelectedGameType;
            set => Set(ref _SelectedGameType, value);
        }
        #endregion
        #region SelectedGameMode
        private string _SelectedGameMode;
        public string SelectedGameMode
        {
            get => _SelectedGameMode;
            set
            {
                if (Set(ref _SelectedGameMode, value))
                {
                    SelectedRatingType = value switch
                    {
                        "Coop" or
                        "Custom" => RatingType.global,
                        "Ladder 1 vs 1" => RatingType.ladder_1v1,
                        "TMM 2 vs 2" => RatingType.tmm_2v2,
                        "TMM 4 vs 4" => RatingType.tmm_4v4_full_share,
                    };
                    SelectedGameType = value switch
                    {
                        "Coop" => GameType.Coop,
                        "Custom" => GameType.Custom,
                        "Ladder 1 vs 1" or
                        "TMM 2 vs 2" or
                        "TMM 4 vs 4" => GameType.MatchMaker
                    };
                    IsLiveInputEnabled = value switch
                    {
                        "Coop" or
                        "Custom" => true,
                        "Ladder 1 vs 1" or
                        "TMM 2 vs 2" or
                        "TMM 4 vs 4" => false
                    };
                    if (!IsLiveInputEnabled)
                    {
                        _IsLive = true;
                    }
                    else
                    {
                        _IsLive = false;
                    }
                    OnPropertyChanged(nameof(IsLive));
                    OnPropertyChanged(nameof(HostGameButtonVisibility));
                    GamesView?.Refresh();
                }
            }
        }
        #endregion
        public static string[] GameModes => new string[]
        {
            "Coop",
            "Custom",
            "Ladder 1 vs 1",
            "TMM 2 vs 2",
            "TMM 4 vs 4",
        };
        #region IsLive
        private bool _IsLive;
        public bool IsLive
        {
            get => _IsLive;
            set
            {
                if (Set(ref _IsLive, value))
                {
                    GamesView?.Refresh();
                }
            }
        }
        #endregion
        #region IsLiveInputDisabled
        private bool _IsLiveInputEnabled;
        public bool IsLiveInputEnabled
        {
            get => _IsLiveInputEnabled;
            set => Set(ref _IsLiveInputEnabled, value);
        }
        #endregion

        public Visibility HostGameButtonVisibility => 
            SelectedGameType is GameType.Custom or GameType.Coop ? 
            Visibility.Visible :
            Visibility.Collapsed;

        #region Games
        public CollectionViewSource GamesSource { get; private set; }
        public ICollectionView GamesView => GamesSource?.View;
        private ObservableCollection<GameInfoMessage> _Games;
        public ObservableCollection<GameInfoMessage> Games
        {
            get => _Games;
            set
            {
                if (Set(ref _Games, value))
                {
                    GamesSource = new()
                    {
                        Source = value
                    };
                    GamesSource.Filter += GamesSource_Filter;
                    OnPropertyChanged(nameof(GamesView));
                }
            }
        }
        #endregion

        private void GamesSource_Filter(object sender, FilterEventArgs e)
        {
            var game = (GameInfoMessage)e.Item;
            e.Accepted = false;
            if (game.GameType != SelectedGameType) return;
            if (game.RatingType != SelectedRatingType) return;
            if (IsLive && !game.LaunchedAt.HasValue) return;
            else if (!IsLive && game.LaunchedAt.HasValue) return;
            e.Accepted = true;
        }

        private void Lobby_GameReceived(object sender, GameInfoMessage e)
        {
            var games = Games;
            if (games is null) return;
            var found = false;
            for (int i = 0; i < games.Count; i++)
            {
                var g = games[i];
                if (g.Uid == e.Uid)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (e.State == GameState.Closed) games.RemoveAt(i);
                        else games[i] = e;
                    }, DispatcherPriority.Send);
                    found = true;
                }
                else if (g.Host == e.Host || g.LaunchedAt.HasValue && g.NumPlayers == 0)
                {
                    Application.Current.Dispatcher.Invoke(() => games.RemoveAt(i), DispatcherPriority.Send);
                }
            }
            if (!found)
                Application.Current.Dispatcher.Invoke(() => games.Add(e), DispatcherPriority.Send);
        }

        private void Lobby_GamesReceived(object sender, GameInfoMessage[] e) =>
            Application.Current.Dispatcher.Invoke(() =>
            {
                Games = new(e);
            }, DispatcherPriority.Send);

        #region HostGame
        private AsyncCommand _HostGameCommand;
        private AsyncCommand HostGameCommand => _HostGameCommand ??= new AsyncCommand(OnHostGameCommandAsync, CanHostGameCommand);

        private bool CanHostGameCommand(object arg) => GameLauncher.ForgedAlliance is null;

        private async Task OnHostGameCommandAsync()
        {
            Lobby.SendAsync(ServerCommands.HostGame("Ethereal FAF Client 2.0 [Test]", FeaturedMod.FAF.ToString(), "SCMP_001"));
        }
        #endregion

        #region JoinGameCommand
        private AsyncCommand<GameInfoMessage> _JoinGameCommand;
        public AsyncCommand<GameInfoMessage> JoinGameCommand => _JoinGameCommand ??= new AsyncCommand<GameInfoMessage>(OnJoinTask, CanJoin);

        private bool CanJoin(object arg) => true;

        private CancellationTokenSource CancellationTokenSource;

        private async Task OnJoinTask(GameInfoMessage game)
        {
            if (game.LaunchedAt.HasValue)
            {
                return;
            }
            if (game.GameType is GameType.MatchMaker) return;
            if (game.NumPlayers == 0)
            {
                SnackbarService.Show("Warning", "Game is empty, possibly broken", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            if (game.Mapname.Contains("neroxis"))
            {
                SnackbarService.Show("Warning", "Neroxis generator not supported", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            if (game.SimMods is not null && game.SimMods.Count > 0)
            {
                SnackbarService.Show("Warning", "SIM mods not supported", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            //snackbar.Timeout = int.MaxValue;
            //snackbar.Closed += Snackbar_Closed;
            CancellationTokenSource = new CancellationTokenSource();
            ContainerViewModel.SplashVisibility = Visibility.Visible;
            ContainerViewModel.SplashText = "Confirming patch";
            IProgress<string> progress = new Progress<string>(e => ContainerViewModel.SplashText = e);
            try
            {
                await GameLauncher.JoinGame(game, CancellationTokenSource.Token, progress);
            }
            catch
            {

            }
            if (!CancellationTokenSource.IsCancellationRequested)
            {
                ContainerViewModel.Content = new MatchView(IceManager, game);
                ContainerViewModel.SplashProgressVisibility = Visibility.Collapsed;
            }
            progress.Report("Waiting ending of match");
            //snackbar.Timeout = 5000;
            var snackbar = SnackbarService.GetSnackbarControl();
            var notify = CancellationTokenSource.IsCancellationRequested ? "Operation was cancelled" : "Launching game";
            snackbar.Show("Notification", notify);
        } 
        #endregion

        private void Snackbar_Closed(Wpf.Ui.Controls.Snackbar sender, RoutedEventArgs e)
        {
            CancellationTokenSource.Cancel();
            sender.Closed -= Snackbar_Closed;
        }
    }
}
