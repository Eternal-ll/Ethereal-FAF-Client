using AsyncAwaitBestPractices.MVVM;
using Ethereal.FAF.UI.Client.Infrastructure.Commands;
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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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
            lobby.WelcomeDataReceived += Lobby_WelcomeDataReceived;
        }

        public PlayerInfoMessage Self;

        private void Lobby_WelcomeDataReceived(object sender, WelcomeData e)
        {
            Self = e.me;
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
            if (e.Id == Self.Id)
            {
                Self = e;
            }
        }

        private void Lobby_PlayersReceived(object sender, PlayerInfoMessage[] e) =>
            Application.Current.Dispatcher.Invoke(() =>
            {
                Players = new(e);
            }, DispatcherPriority.Background);
    }




    public class GamesViewModel : Base.ViewModel
    {
        private readonly LobbyClient LobbyClient;
        private readonly DispatcherTimer Timer;
        private readonly IceManager IceManager;

        private readonly SnackbarService SnackbarService;
        private readonly ContainerViewModel ContainerViewModel;

                


        private readonly GameLauncher GameLauncher;

        private readonly HttpClient HttpClient;
        public GamesViewModel(LobbyClient lobby, GameLauncher gameLauncher, SnackbarService snackbarService, ContainerViewModel containerViewModel, IceManager iceManager, HttpClient httpClient)
        {
            LobbyClient = lobby;
            GameLauncher = gameLauncher;
            SnackbarService = snackbarService;
            lobby.GamesReceived += Lobby_GamesReceived;
            lobby.GameReceived += Lobby_GameReceived;
            //lobby.NotificationReceived += (s, e) => snackbarService.Show("Notification from server", e.text);

            GameLauncher.StateChanged += GameLauncher_StateChanged;

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
            HttpClient = httpClient;
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
                    GamesView.CurrentChanged += GamesView_CurrentChanged;
                    GamesSource.Filter += GamesSource_Filter;
                    IsRefresh = true;
                    OnPropertyChanged(nameof(GamesView));
                    IsRefresh = false;
                }
            }
        }
        private async void GamesView_CurrentChanged(object sender, EventArgs e)
        {
            var onView = GamesView.Cast<GameInfoMessage>().Where(g => g.SmallMapPreview is null).ToList();
            foreach (var game in onView)
            {
                await TrySetSmallPreviewAsync(game);
            }
        }
        #endregion

        private bool IsRefresh;
        private async void GamesSource_Filter(object sender, FilterEventArgs e)
        {
            var game = (GameInfoMessage)e.Item;
            e.Accepted = false;
            if (game.GameType != SelectedGameType) return;
            if (game.RatingType != SelectedRatingType) return;
            if (IsLive && !game.LaunchedAt.HasValue) return;
            else if (!IsLive && game.LaunchedAt.HasValue) return;

            if (!IsRefresh && !game.Mapname.StartsWith("neroxis") && game.SmallMapPreview is null)
            {
                await TrySetSmallPreviewAsync(game);
            }

            e.Accepted = true;
        }

        private async void Lobby_GameReceived(object sender, GameInfoMessage e)
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
                        else
                        {
                            if (g.SmallMapPreview is not null)
                            {
                                e.SmallMapPreview = g.SmallMapPreview;
                                e.OnPropertyChanged(nameof(e.SmallMapPreview));
                            }
                            games[i] = e;
                        }
                    }, DispatcherPriority.Background);
                    found = true;
                }
                else if (g.Host == e.Host || g.LaunchedAt.HasValue && g.NumPlayers == 0)
                {
                    Application.Current.Dispatcher.Invoke(() => games.RemoveAt(i), DispatcherPriority.Background);
                }
            }
            if (!found)
            {
                //await TrySetSmallPreviewAsync(e);
                Application.Current.Dispatcher.Invoke(() => games.Add(e), DispatcherPriority.Background);
            }
        }

        private static bool TrySetCachedImage(GameInfoMessage game, string cacheFolder, string cacheImage, bool skipCheck, out string cacheUrl)
        {
            if (!cacheImage.EndsWith(".png")) cacheImage += ".png";
            cacheUrl = cacheFolder + cacheImage;
            if (!skipCheck && !File.Exists(cacheUrl))
            {
                return false;
            }
            SetSmallPreview(game, cacheUrl);
            return true;
        }
        private static void SetSmallPreview(GameInfoMessage game, string cache)
        {
            game.SmallMapPreview = cache;
            game.OnPropertyChanged(nameof(game.SmallMapPreview));
        }
        List<string> DownloadsInWork = new List<string>();
        private async Task<bool> DownloadAndCacheImage(string download, string cache)
        {
            if (DownloadsInWork.Any(d => d == download)) return true;
            DownloadsInWork.Add(download);
            var client = HttpClient;
            var response = await client.GetAsync(download);
            if (!response.IsSuccessStatusCode) return false;
            using var fs = new FileStream(cache, FileMode.Create);
            await response.Content.CopyToAsync(fs);
            fs.Close();
            await fs.DisposeAsync();
            DownloadsInWork.Remove(download);
            return true;
        }
        List<string> MapGensInWork = new List<string>();
        private async Task<bool> TrySetSmallPreviewAsync(GameInfoMessage game)
        {
            if (TrySetCachedImage(game, "C:\\ProgramData\\FAForever\\cache\\maps\\small\\", game.Mapname, false, out var cache)) return true;
            if (game.Mapname.Contains("neroxis"))
            {
                if (game.SmallMapPreview is not null) return true;
                var mapgen = "C:\\ProgramData\\FAForever\\cache\\maps\\small\\neroxis_map_generator_preview.png";
                if (File.Exists(mapgen))
                {
                    SetSmallPreview(game, mapgen);
                    return true;
                }
                using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("Ethereal.FAF.UI.Client.Resources.neroxis_map_generator_preview.png");
                using var fs = new FileStream(mapgen, FileMode.Create);
                await s.CopyToAsync(fs);
                await s.FlushAsync();
                s.Close();
                fs.Close();
                await s.DisposeAsync();
                await fs.DisposeAsync();
                SetSmallPreview(game, mapgen);
                return true;
                //var map = maps + game.Mapname;
                //var preview = maps + '\\' + game.Mapname + '\\' + game.Mapname + "_preview.png";
                //if (MapGensInWork.Any(c => c == preview) || File.Exists(preview))
                //{
                //    SetSmallPreview(game, preview);
                //    return true;
                //}
                //MapGensInWork.Add(preview);
                //Task.Run(async () =>
                //{
                //    var args = new StringBuilder();
                //    args.Append("-jar \"C:\\ProgramData\\FAForever\\map_generator\\MapGenerator_1.8.5.jar\" ");
                //    args.Append($"--map-name {game.Mapname} ");
                //    args.Append($"--folder-path \"{maps}\" ");
                //    var t = args.ToString();
                //    var process = new Process()
                //    {
                //        StartInfo = new("java", args.ToString()),
                //    };
                //    process.Start();
                //    await process.WaitForExitAsync();
                //    MapGensInWork.Remove(preview);
                //    if (!File.Exists(preview)) return;
                //    SetSmallPreview(game, preview);
                //}).SafeFireAndForget();
                //return true;
            }
            var download = $"https://content.faforever.com/maps/previews/small/{game.Mapname}.png";
            await DownloadAndCacheImage(download, cache);
            SetSmallPreview(game, cache);
            return true;
        }

        private void Lobby_GamesReceived(object sender, GameInfoMessage[] e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Games = new(e);
            }, DispatcherPriority.Background);
        }

        private ICommand _ChangeLiveButton;
        public ICommand ChangeLiveButton => _ChangeLiveButton ??= new LambdaCommand(OnChangeButton, CanChangeButton);
        private bool CanChangeButton(object arg) => IsLiveInputEnabled;
        private void OnChangeButton(object obj) => IsLive = !IsLive;

        #region HostGame
        private AsyncCommand _HostGameCommand;
        public AsyncCommand HostGameCommand => _HostGameCommand ??= new AsyncCommand(OnHostGameCommandAsync, CanHostGameCommand);

        private bool CanHostGameCommand(object arg) => GameLauncher.State is GameLauncherState.Idle;

        private async Task OnHostGameCommandAsync()
        {
            if (GameLauncher.State is not GameLauncherState.Idle) return;
            var host = ServerCommands.HostGame("Ethereal FAF Client 2.0 [Test]", FeaturedMod.FAF.ToString(), "SCMP_001");
            LobbyClient.SendAsync(host);
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
            if (game.Mapname.Contains("neroxis_map_generator_") && !game.Mapname.Contains("1.8.5"))
            {
                SnackbarService.Show("Warning", "Client supports only 1.8.5 version of map generator", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
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
    