using AsyncAwaitBestPractices.MVVM;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using FAF.Domain.LobbyServer;
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

        private readonly SnackbarService SnackbarService;

        public GamesViewModel(LobbyClient lobby, GameLauncher gameLauncher, SnackbarService snackbarService)
        {
            Lobby = lobby;
            GameLauncher = gameLauncher;
            SnackbarService = snackbarService;
            lobby.GamesReceived += Lobby_GamesReceived;
            lobby.GameReceived += Lobby_GameReceived;
            lobby.NotificationReceived += (s, e) => snackbarService.Show("Notification from server", e.text);

            SelectedGameMode = "Custom";

            Timer = new(interval: TimeSpan.FromSeconds(1), DispatcherPriority.Background, (s, e) =>
            {
                if (Games == null) return;
                foreach (var game in Games)
                {
                    if (game.LaunchedAt.HasValue) OnPropertyChanged(game.HumanLaunchedAt);
                }
            }, Application.Current.Dispatcher);
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
                        OnPropertyChanged(nameof(IsLive));
                    }
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
            return;
            var games = Games;
            if (games is null) return;
            var found = false;
            for (int i = 0; i < games.Count; i++)
            {
                if (Application.Current is null) return;
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
            }, DispatcherPriority.Background);

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
            var snackbar = SnackbarService.GetSnackbarControl();
            snackbar.Timeout = int.MaxValue;
            snackbar.Closed += Snackbar_Closed;
            CancellationTokenSource = new CancellationTokenSource();
            snackbar.Show("Patch confirmation", "Preparing patch", Wpf.Ui.Common.SymbolRegular.Alert16);
            IProgress<string> progress = new Progress<string>(e => snackbar.Message = e);
            try
            {
                await GameLauncher.JoinGame(game, CancellationTokenSource.Token, progress);
            }
            catch
            {

            }
            snackbar.Timeout = 5000;

            snackbar.Show("Notification", "Operation was cancelled");
        }

        private void Snackbar_Closed(Wpf.Ui.Controls.Snackbar sender, RoutedEventArgs e)
        {
            CancellationTokenSource.Cancel();
            sender.Closed -= Snackbar_Closed;
        }
    }
}
