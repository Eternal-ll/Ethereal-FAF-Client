using AsyncAwaitBestPractices.MVVM;
using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.Views;
using Ethereal.FAF.UI.Client.Views.Hosting;
using FAF.Domain.LobbyServer.Enums;
using Meziantou.Framework.WPF.Collections;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class GamesViewModel : Base.ViewModel
    {
        private readonly LobbyClient LobbyClient;
        //private readonly DispatcherTimer Timer;
        private readonly IceManager IceManager;

        private readonly SnackbarService SnackbarService;
        private readonly ContainerViewModel ContainerViewModel;

        private readonly IServiceProvider ServiceProvider;


        private readonly string MapsPath;


        private readonly GameLauncher GameLauncher;

        public MatchmakingViewModel MatchmakingViewModel { get; }

        public GamesViewModel(LobbyClient lobby, GameLauncher gameLauncher, SnackbarService snackbarService, ContainerViewModel containerViewModel, IceManager iceManager, IServiceProvider serviceProvider, MatchmakingViewModel matchmakingViewModel)
        {
            LobbyClient = lobby;
            GameLauncher = gameLauncher;
            SnackbarService = snackbarService;
            lobby.GamesReceived += Lobby_GamesReceived;
            lobby.GameReceived += Lobby_GameReceived;
            //lobby.NotificationReceived += (s, e) => snackbarService.Show("Notification from server", e.text);

            GameLauncher.StateChanged += GameLauncher_StateChanged;

            SelectedGameMode = "Custom";

            //Timer = new(interval: TimeSpan.FromSeconds(1), DispatcherPriority.Background, (s, e) =>
            //{
            //    if (Games == null) return;
            //    foreach (var game in Games)
            //    {
            //        if (game.LaunchedAt.HasValue)
            //        {
            //            game.OnPropertyChanged(nameof(game.HumanLaunchedAt));
            //        }
            //    }
            //}, Application.Current.Dispatcher);
            ContainerViewModel = containerViewModel;
            IceManager = iceManager;
            ServiceProvider = serviceProvider;
            MapsPath = FaPaths.Path + "maps/";

            Task.Run(async () =>
            {
                while (true)
                {
                    var games = Games;
                    if (games is not null)
                        foreach (var game in games)
                        {
                            if (game.LaunchedAt.HasValue)
                            {
                                game.OnPropertyChanged(nameof(game.LaunchedAtTimeSpan));
                            }
                        }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });
            MatchmakingViewModel = matchmakingViewModel;
        }

        private void GameLauncher_StateChanged(object sender, GameLauncherState e)
        {
            if (e is GameLauncherState.Idle)
            {
                ContainerViewModel.SplashProgressVisibility = Visibility.Collapsed;
                ContainerViewModel.SplashVisibility = Visibility.Collapsed;
                ContainerViewModel.Content = null;
            }
        }

        public Visibility SearchButtonVisibility =>
            (SelectedGameType is GameType.MatchMaker ||

            (!IgnoreMatchmakingState && MatchmakingViewModel.State is not MatchmakingState.Idle) && MatchmakingViewModel.PartyViewModel.IsOwner)
            ? Visibility.Visible : Visibility.Collapsed;

        public Visibility LiveButtonVisibility => SelectedGameType is not GameType.MatchMaker ?
            Visibility.Visible : Visibility.Collapsed;

        public Visibility PartyVisibility =>
            (!IgnoreMatchmakingState && MatchmakingViewModel.State is MatchmakingState.Searching) ||
            MatchmakingViewModel.PartyViewModel.HasMembers ||
            SelectedRatingType is RatingType.ladder_1v1 or RatingType.tmm_2v2 or RatingType.tmm_4v4_full_share
            ? Visibility.Visible : Visibility.Collapsed;

        public Visibility QueueDataVisibility =>
            SelectedRatingType is RatingType.ladder_1v1 or RatingType.tmm_2v2 or RatingType.tmm_4v4_full_share
            ? Visibility.Visible : Visibility.Collapsed;

        #region SelectedRatingType
        private RatingType _SelectedRatingType;
        public RatingType SelectedRatingType
        {
            get => _SelectedRatingType;
            set
            {
                if (Set(ref _SelectedRatingType, value))
                {
                    MatchmakingViewModel.RatingType = value;
                }
            }
        }
        #endregion


        #region CancelQueuesCommand
        private ICommand _CancelQueuesCommand;
        public ICommand CancelQueuesCommand => _CancelQueuesCommand ??= new LambdaCommand(OnCancelQueuesCommand);
        private bool IgnoreMatchmakingState;
        private void OnCancelQueuesCommand(object obj)
        {
            MatchmakingViewModel.LeaveFromAllQueues();
            IgnoreMatchmakingState = true;
            OnPropertyChanged(nameof(SearchButtonVisibility));
            OnPropertyChanged(nameof(PartyVisibility));
            IgnoreMatchmakingState = false;
        }
        #endregion
        #region SelectedGameType
        private GameType _SelectedGameType;
        public GameType SelectedGameType
        {
            get => _SelectedGameType;
            set
            {
                if (Set(ref _SelectedGameType, value))
                {
                    OnPropertyChanged(nameof(SearchButtonVisibility));
                    OnPropertyChanged(nameof(LiveButtonVisibility));
                    OnPropertyChanged(nameof(HostGameButtonVisibility));
                }
            }
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
                        "1 vs 1" => RatingType.ladder_1v1,
                        "2 vs 2" => RatingType.tmm_2v2,
                        "4 vs 4" => RatingType.tmm_4v4_full_share,
                    };
                    SelectedGameType = value switch
                    {
                        "Coop" => GameType.Coop,
                        "Custom" => GameType.Custom,
                        "1 vs 1" or
                        "2 vs 2" or
                        "4 vs 4" => GameType.MatchMaker
                    };
                    IsLiveInputEnabled = value switch
                    {
                        "Coop" or
                        "Custom" => true,
                        "1 vs 1" or
                        "2 vs 2" or
                        "4 vs 4" => false
                    };
                    if (!IsLiveInputEnabled)
                    {
                        _IsLive = true;
                    }
                    else
                    {
                        _IsLive = false;
                    }
                    OnPropertyChanged(nameof(PartyVisibility));
                    OnPropertyChanged(nameof(QueueDataVisibility));
                    OnPropertyChanged(nameof(IsLive));
                    GamesView?.Refresh();
                }
            }
        }
        #endregion
        public static string[] GameModes => new string[]
        {
            "Coop",
            "Custom",
            "1 vs 1",
            "2 vs 2",
            "4 vs 4",
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
            SelectedGameType is GameType.Custom or GameType.Coop &&
            MatchmakingViewModel.State is MatchmakingState.Idle &&
            MatchmakingViewModel.PartyViewModel.IsOwner &&
            !MatchmakingViewModel.PartyViewModel.HasMembers
            ? Visibility.Visible : Visibility.Collapsed;

        public CollectionViewSource GamesSource { get; private set; }
        public ICollectionView GamesView => GamesSource?.View;
        private bool IsRefresh;
        #region Games
        private ConcurrentObservableCollection<Game> _Games;
        public ConcurrentObservableCollection<Game> Games
        {
            get => _Games;
            set
            {
                if (Set(ref _Games, value))
                {
                    GamesSource = new()
                    {
                        Source = value.AsObservable
                    };
                    GamesSource.Filter += GamesSource_Filter;
                    IsRefresh = true;
                    OnPropertyChanged(nameof(GamesView));
                    IsRefresh = false;
                }
            }
        }

        private void SetMapScenario(Game game)
        {
            if (game.MapScenario is not null) return;
            var scenario = MapsPath + game.Mapname + "/" + game.Mapname.Split('.')[0] + "_scenario.lua";
            if (File.Exists(scenario))
            {
                game.MapScenario = FA.Vault.MapScenario.FromFile(scenario);
            }
        }
        #endregion
        private void GamesSource_Filter(object sender, FilterEventArgs e)
        {
            var game = (Game)e.Item;
            e.Accepted = false;
            if (game.GameType != SelectedGameType) return;
            if (game.RatingType != SelectedRatingType) return;
            if (IsLive && !game.LaunchedAt.HasValue) return;
            else if (!IsLive && game.LaunchedAt.HasValue) return;

            SetMapScenario(game);

            e.Accepted = true;
        }

        private void Lobby_GameReceived(object sender, Game e)
        {
            var games = Games;
            if (games is null) return;

            e.SmallMapPreview = $"https://content.faforever.com/maps/previews/small/{e.Mapname}.png";
            var found = false;
            for (int i = 0; i < games.Count; i++)
            {
                var g = games[i];
                if (g.Uid == e.Uid)
                {
                    if (e.State == GameState.Closed) games.RemoveAt(i);
                    else
                    {
                        if (g.SmallMapPreview is not null)
                        {
                            e.SmallMapPreview = g.SmallMapPreview;
                            e.OnPropertyChanged(nameof(e.SmallMapPreview));
                        }
                        e.MapGeneratorState = g.MapGeneratorState;
                        games[i] = e;
                    }
                    found = true;
                }
                else if (g.Host == e.Host || g.LaunchedAt.HasValue && g.NumPlayers == 0)
                {
                    games.RemoveAt(i);
                }
            }
            if (!found)
            {
                //await TrySetSmallPreviewAsync(e);
                games.Add(e);
            }
        }
        private static void SetSmallPreview(Game game, string cache)
        {
            game.SmallMapPreview = cache;
            game.OnPropertyChanged(nameof(game.SmallMapPreview));
        }

        private void Lobby_GamesReceived(object sender, Game[] e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var g in e)
                {
                    g.SmallMapPreview = $"https://content.faforever.com/maps/previews/small/{g.Mapname}.png";
                }
                var obs = new ConcurrentObservableCollection<Game>();
                obs.AddRange(e);
                Games = obs;
            }, DispatcherPriority.Background);
        }

        private ICommand _ChangeLiveButton;
        public ICommand ChangeLiveButton => _ChangeLiveButton ??= new LambdaCommand(OnChangeButton, CanChangeButton);
        private bool CanChangeButton(object arg) => IsLiveInputEnabled;
        private void OnChangeButton(object obj) => IsLive = !IsLive;

        #region HostGame
        private ICommand _HostGameCommand;
        public ICommand HostGameCommand => _HostGameCommand ??= new LambdaCommand(OnHostGameCommand, CanHostGameCommand);

        private bool CanHostGameCommand(object arg) =>
            GameLauncher.State is GameLauncherState.Idle && MatchmakingViewModel.State is MatchmakingState.Idle;
        private void OnHostGameCommand(object arg)
        {
            ContainerViewModel.Content = ServiceProvider.GetService<HostGameView>();
        }
        #endregion

        #region JoinGameCommand
        private AsyncCommand<Game> _JoinGameCommand;
        public AsyncCommand<Game> JoinGameCommand => _JoinGameCommand ??= new AsyncCommand<Game>(OnJoinTask, CanJoin);

        private bool CanJoin(object arg) => true;

        private CancellationTokenSource CancellationTokenSource;

        private async Task OnJoinTask(Game game)
        {
            if (game.LaunchedAt.HasValue)
            {
                return;
            }
            if (GameLauncher.State is not GameLauncherState.Idle)
            {
                SnackbarService.Show("Warning", $"Cant join game while launcher in state [{GameLauncher.State}]", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            if (game.GameType is GameType.MatchMaker) return;
            if (game.NumPlayers == 0)
            {
                SnackbarService.Show("Warning", "Game is empty, possibly broken", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            if (game.PasswordProtected)
            {
                SnackbarService.Show("Warning", "Game is password protected", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            //if (game.Mapname.Contains("neroxis_map_generator_") && !game.Mapname.Contains("1.8.5"))
            //{
            //    SnackbarService.Show("Warning", "Client supports only 1.8.5 version of map generator", Wpf.Ui.Common.SymbolRegular.Warning24);
            //    return;
            //}
            if (game.SimMods is not null && game.SimMods.Count > 0)
            {
                SnackbarService.Show("Warning", "SIM mods not supported", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            CancellationTokenSource = new CancellationTokenSource();
            ContainerViewModel.SplashVisibility = Visibility.Visible;
            ContainerViewModel.SplashText = "Confirming patch";
            IProgress<string> progress = new Progress<string>(e => ContainerViewModel.SplashText = e);

            await GameLauncher.JoinGame(game, progress, CancellationTokenSource.Token)
                .ContinueWith(async t =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        await SnackbarService.ShowAsync("Exception", t.Exception.Message, Wpf.Ui.Common.SymbolRegular.Warning24);
                        ContainerViewModel.SplashVisibility = Visibility.Collapsed;
                        ContainerViewModel.SplashProgressVisibility = Visibility.Collapsed;
                    }
                    if (!CancellationTokenSource.IsCancellationRequested)
                    {
                        ContainerViewModel.Content = new MatchView(IceManager, game);
                        ContainerViewModel.SplashProgressVisibility = Visibility.Collapsed;
                    }
                    progress.Report("Waiting ending of match");
                    var snackbar = SnackbarService.GetSnackbarControl();
                    var notify = CancellationTokenSource.IsCancellationRequested ? "Operation was cancelled" : "Launching game";
                    snackbar.Show("Notification", notify);
                });
        } 
        #endregion

        private void Snackbar_Closed(Wpf.Ui.Controls.Snackbar sender, RoutedEventArgs e)
        {
            CancellationTokenSource.Cancel();
            sender.Closed -= Snackbar_Closed;
        }
    }
}
    