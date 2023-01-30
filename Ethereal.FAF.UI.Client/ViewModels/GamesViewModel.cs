using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.DataTemplateSelectors;
using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.Views.Hosting;
using FAF.Domain.LobbyServer.Enums;
using Meziantou.Framework.WPF.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class GamesViewModel : JsonSettingsViewModel
    {
        private readonly GameLauncher GameLauncher;
        private readonly ServersManagement ServersManagement;

        private readonly NotificationService NotificationService;
        private readonly INavigationService NavigationService;
        private readonly DialogService DialogService;
        private readonly PlayersViewModel PlayersViewModel;
        private readonly MapsService MapsService;

        private readonly ILogger Logger;
        private readonly IConfiguration Configuration;

        private MatchmakingViewModel _MatchmakingViewModel;
        public MatchmakingViewModel MatchmakingViewModel
        {
            get => _MatchmakingViewModel;
            set => Set(ref _MatchmakingViewModel, value);
        }

        public GamesViewModel(
            GameLauncher gameLauncher,
            NotificationService notificationService,
            PlayersViewModel playersViewModel,
            ILogger<GamesViewModel> logger,
            DialogService dialogService,
            IConfiguration configuration,
            INavigationService navigationService,
            ServersManagement serversManagement,
            MapsService mapsService)
        {
            ChangeLiveButton = new LambdaCommand(OnChangeButton, CanChangeButton);
            HostGameCommand = new LambdaCommand(OnHostGameCommand, CanHostGameCommand);
            JoinGameCommand = new LambdaCommand(OnJoinGameCommand);
            WatchGameCommand = new LambdaCommand(OnWatchGameCommand);


            AddMapToBlacklistCommand = new LambdaCommand(OnAddMapToBlacklistCommand);
            RemoveMapFromBlacklistCommand = new LambdaCommand(OnRemoveMapFromBlacklistCommand);

            MapsBlacklist = new();
            MapsBlacklistSource = new()
            {
                Source = MapsBlacklist.AsObservable
            };
            MapsBlacklistSource.Filter += MapsBlacklistSource_Filter;

            _OnlyGeneratedMaps = configuration.GetValue<bool>("Lobby:Games:OnlyGeneratedMaps");
            _OnlyLobbiesWithFriends = configuration.GetValue<bool>("Lobby:Games:OnlyLobbiesWithFriends");
            _HidePrivateLobbies = configuration.GetValue<bool>("Lobby:Games:HidePrivateLobbies");
            _EnableMapsBlacklist = configuration.GetValue<bool>("Lobby:Games:EnableMapsBlacklist");
            var banned = configuration.GetSection("Lobby:Games:MapsBlacklist").Get<string[]>();
            if (banned is not null)
                MapsBlacklist.AddRange(banned);


            Games = new();
            //Games.SupportRangeNotifications = true;
            GamesSource = new()
            {
                Source = Games.AsObservable
            };
            GamesSource.Filter += GamesSource_Filter;

            serversManagement.ServerManagerAdded += (s, e) =>
            {
                MatchmakingViewModel = e.GetMatchmakingViewModel();

                var lobby = e.GetLobbyClient();
                lobby.StateChanged += (s, state) =>
                {
                    if (state is not LobbyState.Disconnecting) return;
                    lobby.GamesReceived -= e.GamesReceived;
                    lobby.GameReceived -= e.GameReceived;
                    foreach (var game in Games)
                    {
                        if (game.ServerManager.Equals(s))
                        {
                            Games.Remove(game);
                        }
                    }
                };
                e.GameReceived = (s, game) =>
                {
                    game.SetServerManager(e);
                    Lobby_GameReceived(s, game);
                };
                e.GamesReceived = (s, games) =>
                {
                    foreach (var game in games)
                    {
                        game.SetServerManager(e);
                    }
                    Lobby_GamesReceived(s, games);
                };
                lobby.GamesReceived += e.GamesReceived;
                lobby.GameReceived += e.GameReceived;
            };

            SelectedGameMode = "Custom";

            GameLauncher = gameLauncher;
            Configuration = configuration;
            Logger = logger;

            NotificationService = notificationService;
            PlayersViewModel = playersViewModel;
            DialogService = dialogService;
            NavigationService = navigationService;
            ServersManagement = serversManagement;
            MapsService = mapsService;
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

        CancellationTokenSource BackgroundToken;
        Task BackgroundTask;

        private void SetTimeUpdater()
        {
            if (BackgroundToken is not null) return;
            BackgroundToken = new();
            BackgroundTask = Task.Run(async () =>
            {
                while (!BackgroundToken.IsCancellationRequested)
                {
                    var games = Games.Where(g =>
                    g.State is GameState.Playing &&
                    g.RatingType == SelectedRatingType);
                    if (games is null || !games.Any())
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }
                    foreach (var game in games)
                    {
                        if (game.LaunchedAt.HasValue)
                        {
                            game.OnPropertyChanged(nameof(game.LaunchedAtTimeSpan));
                        }
                        game.OnPropertyChanged(nameof(game.AfterCreationTimeSpan));
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                BackgroundTask = null;
                BackgroundToken = null;
            });
        }

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
                    if (value is not RatingType.global)
                    {
                        SetTimeUpdater();
                    }
                    else
                    {
                        BackgroundToken?.Cancel();
                    }
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
                    if (value)
                    {
                        SetTimeUpdater();
                    }
                    else
                    {
                        BackgroundToken?.Cancel();
                    }
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

        #region Compact
        private bool _Compact;
        public bool Compact
        {
            get => _Compact;
            set
            {
                if (Set(ref _Compact, value))
                {

                    GameTemplateSelector.LadderCompact = value;
                    if (SelectedRatingType is RatingType.ladder_1v1)
                    {
                        GamesView?.Refresh();
                    }
                }
            }
        }
        #endregion

        public Visibility HostGameButtonVisibility =>
            (SelectedGameType is GameType.Custom or GameType.Coop &&
            MatchmakingViewModel.State is MatchmakingState.Idle &&
            MatchmakingViewModel.PartyViewModel.IsOwner &&
            !MatchmakingViewModel.PartyViewModel.HasMembers) || true
            ? Visibility.Visible : Visibility.Collapsed;

        private readonly ConcurrentObservableCollection<Game> Games;
        public CollectionViewSource GamesSource { get; private set; }
        public ICollectionView GamesView => GamesSource.View;

        #region SelectedGame
        private Game _SelectedGame;
        public Game SelectedGame
        {
            get => _SelectedGame;
            set
            {
                if (Set(ref _SelectedGame, value))
                {

                }
            }
        }
        #endregion

        public Game GetGame(long id) => Games?.FirstOrDefault(g => g.Uid == id);
        public bool TryGetGame(long id, out Game game, out int index)
        {
            var games = Games;
            game = null;
            index = 0;
            for (int i = 0; i < games.Count; i++)
            {
                var g = games[i];
                if (g.State is GameState.Closed)
                {
                    games.Remove(g);
                    continue;
                }
                if (g.Uid == id)
                {
                    game = g;
                    index = i;
                    break;
                }
            }
            return game is not null;
        }

        private void SetMapScenario(Game game)
        {
            if (game.MapScenario is not null) return;
            var scenario = Configuration.GetMapFile(game.Mapname, "_scenario.lua");
            if (File.Exists(scenario))
            {
                game.MapScenario = FA.Vault.MapScenario.FromFile(scenario);
            }
        }
        private void GamesSource_Filter(object sender, FilterEventArgs e)
        {
            var game = (Game)e.Item;
            e.Accepted = false;
            game.IsPassedFilter = false;
            
            // tab filter
            if (game.GameType != SelectedGameType) return;
            if (game.RatingType != SelectedRatingType) return;
            if (game.RatingType is RatingType.global) 
            {
                if (IsLive && !game.LaunchedAt.HasValue) return;
                else if (!IsLive && game.LaunchedAt.HasValue) return;
            }

            // user filters
            if (OnlyGeneratedMaps && !game.IsMapgen) return;
            if (OnlyLobbiesWithFriends) { }
            if (HideFoesLobbies) { }
            if (HidePrivateLobbies && game.PasswordProtected) return;
            if (EnableMapsBlacklist && MapsBlacklist.Any(map => game.Mapname.Split('.')[0].Contains(map, StringComparison.OrdinalIgnoreCase)))
                return;

            SetMapScenario(game);

            e.Accepted = true;
            game.IsPassedFilter = true;
        }

        public bool IsBadGame(Game game)
        {
            if (game.State is GameState.Closed) return true;
            if (!game.Teams.Any(t => t.Value.Any()) && game.State is GameState.Open)
            {
                return true;
            }
            if (!game.TeamsIds.Any(t => t.PlayerIds.Any()) && game.State is GameState.Open)
            {
                return true;
            }
            return false;
        }

        private void Lobby_GamesReceived(object sender, Game[] e)
        {
            var badGames = new List<Game>();
            foreach (var g in e)
            {
                //if (IsBadGame(g))
                //{
                //    badGames.Add(g);
                //    continue;
                //}
                if (!g.IsMapgen) g.SmallMapPreview = $"https://content.faforever.com/maps/previews/small/{g.Mapname}.png";
                g.UpdateTeams();
                FillPlayers(g);
                //if (g.HostPlayer?.Player is null)
                //{
                //    badGames.Add(g);
                //}
            }
            //using var defer = GamesView.DeferRefresh();
            Games.AddRange(e);
        }
        private void Lobby_GameReceived(object sender, Game e) => ProceedGame(e);
        private void ProceedGame(Game e)
        {
            var games = Games;
            if (games is null) return;
            if (!TryGetGame(e.Uid, out var cached, out var index))
            {
                //Logger.LogWarning($"Unique game received [{e.Uid}] [{e.NumPlayers}] [{e.MaxPlayers}] [{e.State}]");
                if (e.State is GameState.Closed) 
                {
                    return;
                }
                if (e.State is GameState.Playing && e.NumPlayers == 0)
                {
                    return;
                }
                //if (e.State is GameState.Closed)
                //{
                //    // received new closed game
                //    //Logger.LogWarning("Received game [{uid}] that was [GameState.Closed] and not founded in cache", e.Uid);
                //    Logger.LogWarning($"Received [{e.Uid}] [{e.NumPlayers}] [{e.MaxPlayers}] [{e.State}]");
                //    return;
                //}
                //if (IsBadGame(e))
                //{
                //    if (e.State is GameState.Closed)
                //    {
                //        // received new closed game
                //        Logger.LogWarning("Received game [{uid}] that was [GameState.Closed] and not founded in cache", e.Uid);
                //        return;
                //    }
                //    return;
                //}
                //newGame.Host = PlayersService.GetPlayer(newGame.host);
                //HandleTeams(newGame);
                e.UpdateTeams();
                FillPlayers(e);
                games.Add(e);
                //Logger.LogInformation(e.Title);
                //OnNewGameReceived(newGame);
                return;
            }

            var leftPlayers = cached.PlayersIds
                .Except(e.PlayersIds)
                .ToArray();

            if (e.State is GameState.Closed)
            {
                if (cached.State is GameState.Playing)
                {
                    // game is end
                }
                else
                {
                    // host closed game
                }
                //ProcessLeftPlayers(cached, leftPlayers);
                games.Remove(cached);
            }

            if (e.State is GameState.Playing)
            {
                if (cached.State is GameState.Playing)
                {
                    if (e.NumPlayers < cached.NumPlayers && e.NumPlayers == 0)
                    {
                        games.Remove(cached);
                        return;
                    }
                }
            }

            var joinedPlayers = e.PlayersIds
                .Except(cached.PlayersIds)
                .ToArray();

            // lets update cached game
            cached.Title = e.Title;
            cached.Mapname = e.Mapname;
            cached.MaxPlayers = e.MaxPlayers;
            cached.NumPlayers = e.NumPlayers;
            cached.LaunchedAt = e.LaunchedAt;
            OnPropertyChanged(nameof(cached.LaunchedAtTimeSpan));


            if (e.State is GameState.Playing && cached.State is GameState.Playing)
            {
                // update playing game
                ProcessDiedPlayers(cached, leftPlayers);
            }
            else
            {
                // game updated
                ProcessLeftPlayers(cached, leftPlayers);
                UpdateTeams(cached, e);
                FillPlayers(cached);
                ProcessJoinedPlayers(cached, joinedPlayers);
            }

            if (e.State is GameState.Playing)
            {
                if (cached.State is GameState.Open)
                {
                    //Logger.LogTrace("Game [{title}] [{mod}] [{rating}] launched, updating list on index",
                    //    e.Title, e.FeaturedMod, e.RatingType);
                    e.GameTeams = cached.GameTeams;
                    if (cached.MapGeneratorState is MapGeneratorState.Generated)
                    {
                        e.SmallMapPreview = cached.SmallMapPreview;
                        e.MapGeneratorState = cached.MapGeneratorState;
                    }
                    e.HostPlayer = cached.HostPlayer;
                    Games[index] = e;
                }
            }
            cached.State = e.State;
        }
        private void UpdateTeams(Game oldGame, Game newGame)
        {
            oldGame.Teams = newGame.Teams;
            oldGame.TeamsIds = newGame.TeamsIds;
            oldGame.UpdateTeams();
        }
        private void FillPlayers(Game game)
        {
            foreach (var player in game.Players)
            {
                if (!PlayersViewModel.TryGetPlayer(player.Id, game.ServerManager, out var cache))
                {
                    continue;
                }
                if (game.HostPlayer is null)
                {

                }
                if (player.Login == game.Host)
                {
                    game.HostPlayer = cache;
                    player.IsHost = true;
                }
                player.Player = cache;
                cache.Game = game;
            }
        }
        private void ProcessJoinedPlayers(Game game, params long[] joinedPlayers)
        {
            foreach (var joined in joinedPlayers)
            {
                if (!game.TryGetPlayer(joined, out var player))
                {
                    continue;
                }
            }
        }
        private void ProcessLeftPlayers(Game game, params long[] leftPlayers)
        {
            foreach (var left in leftPlayers)
            {
                Player cached;
                if (!game.TryGetPlayer(left, out var player))
                {
                    if (!PlayersViewModel.TryGetPlayer(left, game.ServerManager, out cached))
                    {
                        // welp... GG
                    }
                }
                else
                {
                    cached = player.Player;
                }
                if (cached is not null)
                    cached.Game = null;
            }
        }
        private void ProcessDiedPlayers(Game game, params long[] diedPlayers)
        {
            foreach (var died in diedPlayers)
            {
                if (!game.TryGetPlayer(died, out var player))
                {
                    continue;
                }
                player.IsConnected = false;
            }
        }

        public ICommand ChangeLiveButton { get; }
        private bool CanChangeButton(object arg) => IsLiveInputEnabled;
        private void OnChangeButton(object obj) => IsLive = !IsLive;

        #region HostGame
        public ICommand HostGameCommand { get; }
        private bool CanHostGameCommand(object arg) =>
            GameLauncher.State is GameLauncherState.Idle && MatchmakingViewModel.State is MatchmakingState.Idle;
        private void OnHostGameCommand(object arg)
        {
            NavigationService.GetNavigationControl().SelectedPageIndex = 0;
            //NavigationService.Navigate(-1);
            NavigationService.Navigate(typeof(HostGameView));
        }
        #endregion

        #region WatchGameCommand
        public ICommand WatchGameCommand { get; }
        private void OnWatchGameCommand(object arg)
        {
            if (arg is not Game game) return;
            if (game.State is not GameState.Playing)
            {
                NotificationService.Notify("Warning", $"Cant watch [{game.State}] game", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            if (game.SimMods is not null && game.SimMods.Count > 0)
            {
                NotificationService.Notify("Warning", $"Sim mods not supported", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            Task.Run(async () => await MapsService
                .EnsureMapExistAsync(game.Mapname, game.ServerManager.GetContentClient())
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Logger.LogError(t.Exception.ToString());
                        return;
                    }
                    t.ContinueWith(t => game.ServerManager
                    .GetPatchClient()
                    .ConfirmPatchAsync(game.FeaturedMod)
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            Logger.LogError(t.Exception.ToString());
                            return;
                        }
                        t.ContinueWith(t => GameLauncher.WatchGame(game));
                    }));
                }));
        }
        #endregion

        #region JoinGameCommand
        public ICommand JoinGameCommand { get; }
        private CancellationTokenSource CancellationTokenSource;
        private Game LastGame;
        private async void OnJoinGameCommand(object arg)
        {
            if (arg is not Game game) return;
            if (game.FeaturedMod is not FeaturedMod.coop or FeaturedMod.FAF or FeaturedMod.FAFBeta or FeaturedMod.FAFDevelop)
            {
                NotificationService.Notify("Warning", $"Featured mod [{game.FeaturedMod}] not supported");
                return;
            }
            if (game.LaunchedAt.HasValue)
            {
                WatchGameCommand.Execute(arg);
                return;
            }
            if (GameLauncher.State is not GameLauncherState.Idle)
            {
                NotificationService.Notify("Warning", $"Cant join game while launcher in state [{GameLauncher.State}]", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            if (game.GameType is GameType.MatchMaker) return;
            if (game.NumPlayers == 0)
            {
                NotificationService.Notify("Warning", "Game is empty, possibly broken", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            if (game.PasswordProtected)
            {
                var textbox = new System.Windows.Controls.PasswordBox
                {
                    Margin = new Thickness(10, 20, 10, 0),
                    Padding = new Thickness(10, 4, 10, 4)
                };
                var dialog = DialogService.GetDialogControl();
                dialog.Content = textbox;
                dialog.Title = "Game password protected";
                dialog.ButtonLeftName = "Enter password";
                dialog.ButtonRightName = "Cancel";
                var result = await dialog.ShowAndWaitAsync(true);
                if (result is not Wpf.Ui.Controls.Interfaces.IDialogControl.ButtonPressed.Left) return;
                dialog.Hide();
                var password = textbox.Password;
                //NotificationService.Notify("Warning", "Game is password protected", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            if (game.SimMods is not null && game.SimMods.Count > 0)
            {
                NotificationService.Notify("Warning", "SIM mods not supported", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            CancellationTokenSource = new CancellationTokenSource();

            var dialogg = DialogService.GetDialogControl();
            dialogg.Title = "Joining game";
            dialogg.Content = null;
            dialogg.ButtonLeftName = "Cancel";
            dialogg.ButtonRightName = "Cancel";
            dialogg.Closed += Dialog_Closed;
            dialogg.Show();

            await Task.Run(async () =>
            {
                // todo
                IProgress<string> progress = new Progress<string>(d => dialogg.Content = d);

                // TODO reverse MapsService injection from game
                await MapsService
                    .EnsureMapExistAsync(game.Mapname, game.ServerManager.GetContentClient())
                    .ContinueWith(async t =>
                    {
                        if (t.IsFaulted)
                        {
                            Logger.LogError(t.Exception.ToString());
                            return;
                        }
                        await game.ServerManager
                        .GetPatchClient()
                        .ConfirmPatchAsync(game.FeaturedMod)
                        .ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                Logger.LogError(t.Exception.ToString());
                                return;
                            }
                            game.ServerManager.GetLobbyClient().JoinGame(game.Uid);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                dialogg.Closed -= Dialog_Closed;
                                dialogg.Hide();
                            });
                        });
                    });
            });


            //try
            //{
            //    await MapsService.EnsureMapExistAsync(game.Mapname, game.ServerManager.GetContentClient(), CancellationTokenSource.Token);
            //    await GameLauncher.JoinGame(game, progress, CancellationTokenSource.Token);
            //    LastGame = null;
            //}
            //catch (TaskCanceledException canceled)
            //{
            //    LastGame = null;
            //    NotificationService.Notify("Exception", canceled.Message, Wpf.Ui.Common.SymbolRegular.Warning24);
            //    GameLauncher.State = GameLauncherState.Idle;
            //}
            //catch (Exception ex)
            //{
            //    LastGame = null;
            //    NotificationService.Notify("Exception", ex.Message, Wpf.Ui.Common.SymbolRegular.Warning24);
            //    GameLauncher.State = GameLauncherState.Idle;
            //}
            //finally
            //{
            //    dialogg.Closed -= Dialog_Closed;
            //    dialogg.Hide();
            //}
        }

        private void Dialog_Closed([System.Diagnostics.CodeAnalysis.NotNull] Wpf.Ui.Controls.Dialog sender, RoutedEventArgs e)
        {
            CancellationTokenSource.Cancel();
        }
        #endregion

        #region Filters

        #region OnlyGeneratedMapsd
        private bool _OnlyGeneratedMaps;
        public bool OnlyGeneratedMaps
        {
            get => _OnlyGeneratedMaps;
            set
            {
                if (Set(ref _OnlyGeneratedMaps, value, "Lobby:Games:OnlyGeneratedMaps"))
                    GamesView.Refresh();
            }
        }
        #endregion

        #region OnlyLobbiesWithFriends
        private bool _OnlyLobbiesWithFriends;
        public bool OnlyLobbiesWithFriends
        {
            get => _OnlyLobbiesWithFriends;
            set
            {
                if (Set(ref _OnlyLobbiesWithFriends, value, "Lobby:Games:OnlyLobbiesWithFriends"))
                    GamesView.Refresh();
            }
        }

        #endregion

        #region HidePrivateLobbies
        private bool _HidePrivateLobbies;
        public bool HidePrivateLobbies
        {
            get => _HidePrivateLobbies;
            set
            {
                if (Set(ref _HidePrivateLobbies, value, "Lobby:Games:HidePrivateLobbies"))
                    GamesView.Refresh();
            }
        }
        #endregion

        #region HideFoesLobbies
        private bool _HideFoesLobbies;
        public bool HideFoesLobbies
        {
            get => _HideFoesLobbies;
            set
            {
                if (Set(ref _HideFoesLobbies, value, "Lobby:Games:HideFoesLobbies"))
                    GamesView.Refresh();
            }
        }
        #endregion

        #region EnableMapsBlacklist
        private bool _EnableMapsBlacklist;
        public bool EnableMapsBlacklist
        {
            get => _EnableMapsBlacklist;
            set
            {
                if (Set(ref _EnableMapsBlacklist, value, "Lobby:Games:EnableMapsBlacklist"))
                {
                    GamesView.Refresh();
                    OnPropertyChanged(nameof(MapsBlacklistVisibility));
                }
            }
        }
        #endregion

        public Visibility MapsBlacklistVisibility => EnableMapsBlacklist ? Visibility.Visible : Visibility.Collapsed;

        private readonly ConcurrentObservableCollection<string> MapsBlacklist;
        private readonly CollectionViewSource MapsBlacklistSource;
        public ICollectionView MapsBlacklistView => MapsBlacklistSource.View;


        #region MapsBlacklistFilter
        private string _MapsBlacklistFilter;
        public string MapsBlacklistFilter
        {
            get => _MapsBlacklistFilter;
            set
            {
                if (Set(ref _MapsBlacklistFilter, value))
                {
                    MapsBlacklistView.Refresh();
                }
            }
        }
        #endregion

        private void MapsBlacklistSource_Filter(object sender, FilterEventArgs e)
        {
            var map = (string)e.Item;
            e.Accepted = false;
            if (!string.IsNullOrWhiteSpace(MapsBlacklistFilter) &&
                MapsBlacklistFilter.Split().Any(c => map.Contains(c, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }
            e.Accepted = true;
        }
        public ICommand AddMapToBlacklistCommand { get; }
        private void OnAddMapToBlacklistCommand(object arg)
        {
            if (arg is not string map) return;
            var name = map.Split('.')[0];
            MapsBlacklist.Add(name);
            UserSettings.Update("Lobby:Games:MapsBlacklist", MapsBlacklist.ToArray());
            GamesView.Refresh();
        }
        public ICommand RemoveMapFromBlacklistCommand { get; }
        private void OnRemoveMapFromBlacklistCommand(object arg)
        {
            if (arg is not string map) return;
            MapsBlacklist.Remove(map);
            UserSettings.Update("Lobby:Games:MapsBlacklist", MapsBlacklist.ToArray());
            GamesView.Refresh();
        }

        #endregion
    }
}
    