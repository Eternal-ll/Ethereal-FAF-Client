﻿using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.DataTemplateSelectors;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.Views.Hosting;
using FAF.Domain.LobbyServer.Enums;
using Meziantou.Framework.WPF.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class GamesViewModel : JsonSettingsViewModel
    {

        private readonly NotificationService NotificationService;
        private readonly INavigationService NavigationService;
        //private readonly DialogService DialogService;
        private readonly IFafPlayersService _fafPlayersService;
        private readonly IFafGamesService _fafGamesService;
        private readonly IFafGamesEventsService _fafGamesEventsService;

        private readonly ILogger Logger;
        private readonly IConfiguration Configuration;

        private MatchmakingViewModel _MatchmakingViewModel;
        public MatchmakingViewModel MatchmakingViewModel
        {
            get => _MatchmakingViewModel;
            set => Set(ref _MatchmakingViewModel, value);
        }

        public GamesViewModel(
            NotificationService notificationService,
            ILogger<GamesViewModel> logger,
            IConfiguration configuration,
            INavigationService navigationService,
            IServiceProvider serviceProvider,
            MatchmakingViewModel matchmakingViewModel,
            IFafPlayersService fafPlayersService,
            IFafGamesService fafGamesService,
            IFafGamesEventsService fafGamesEventsService)
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

            fafGamesEventsService.GameAdded += FafGamesService_GamesAdded;
            fafGamesEventsService.GameRemoved += FafGamesService_GamesRemoved;
            fafGamesEventsService.GameStateChanged += FafGamesEventsService_GameStateChanged;

            Games = new();
            Games.AddRange(fafGamesService.GetGames());
            //Games.SupportRangeNotifications = true;
            GamesSource = new()
            {
                Source = Games.AsObservable
            };
            GamesSource.Filter += GamesSource_Filter;
            SelectedGameMode = "Custom";
            Configuration = configuration;
            Logger = logger;
            MatchmakingViewModel = matchmakingViewModel;
            NotificationService = notificationService;
            //DialogService = dialogService;
            NavigationService = navigationService;
            _fafPlayersService = fafPlayersService;
            _fafGamesService = fafGamesService;
            _fafGamesEventsService = fafGamesEventsService;
        }

        private void FafGamesEventsService_GameStateChanged(object sender, (Game game, GameState newest, GameState latest) e)
        {

            var refreshView = false;
            if (e.game.GameType == GameType.Custom
                && SelectedGameMode == "Custom"
                && e.latest == GameState.Playing
                && IsLive)
            {
                refreshView = true;
            }
            if (e.game.GameType == GameType.Custom
                && SelectedGameMode == "Custom"
                && e.latest == GameState.Open
                && !IsLive)
            {
                refreshView = true;
            }
            if (refreshView)
            {
                Application.Current.Dispatcher.BeginInvoke(() => GamesView.Refresh());
            }
        }

        private void FafGamesService_GamesRemoved(object sender, Game e)
        {
            Games.Remove(e);
            if (SelectedGameMode == "Custom"
                && e.State == GameState.Closed)
            {
                //Application.Current.Dispatcher.BeginInvoke(() => GamesView.Refresh());
            }
        }

        private void FafGamesService_GamesAdded(object sender, Game e)
        {
            Games.Add(e);
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
                            //game.OnPropertyChanged(nameof(game.LaunchedAtTimeSpan));
                        }
                        //game.OnPropertyChanged(nameof(game.AfterCreationTimeSpan));
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
            //var scenario = Configuration.GetMapFile(game.Mapname, "_scenario.lua");
            //if (File.Exists(scenario))
            //{
            //    game.MapScenario = FA.Vault.MapScenario.FromFile(scenario);
            //}
        }
        private void GamesSource_Filter(object sender, FilterEventArgs e)
        {
            var game = (Game)e.Item;
            e.Accepted = false;
            
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
            //if (EnableMapsBlacklist && MapsBlacklist.Any(map => game.Mapname.Split('.')[0].Contains(map, StringComparison.OrdinalIgnoreCase)))
            //    return;

            SetMapScenario(game);

            e.Accepted = true;
        }

        public ICommand ChangeLiveButton { get; }
        private bool CanChangeButton(object arg) => IsLiveInputEnabled;
        private void OnChangeButton(object obj) => IsLive = !IsLive;

        #region HostGame
        public ICommand HostGameCommand { get; }
        private bool CanHostGameCommand(object arg) => false;
        private void OnHostGameCommand(object arg)
        {
            //NavigationService.GetNavigationControl().SelectedPageIndex = 0;
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
                //NotificationService.Notify("Warning", $"Cant watch [{game.State}] game", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
            if (game.SimMods is not null && game.SimMods.Count > 0)
            {
                //NotificationService.Notify("Warning", $"Sim mods not supported", Wpf.Ui.Common.SymbolRegular.Warning24);
                return;
            }
        }
        #endregion

        #region JoinGameCommand
        public ICommand JoinGameCommand { get; }
        private CancellationTokenSource CancellationTokenSource;
        private Game LastGame;
        private async void OnJoinGameCommand(object arg)
        {
            if (arg is not Game game) return;
            var allowed = new[] { FeaturedMod.coop, FeaturedMod.FAF, FeaturedMod.FAFBeta, FeaturedMod.FAFDevelop };
            if (!allowed.Contains(game.FeaturedMod))
            {
                NotificationService.Notify("Warning", $"Featured mod [{game.FeaturedMod}] not supported");
                return;
            }
            if (game.LaunchedAt.HasValue)
            {
                WatchGameCommand.Execute(arg);
                return;
            }
            if (game.GameType is GameType.MatchMaker) return;
            if (game.NumPlayers == 0)
            {
                NotificationService.Notify("Warning", "Game is empty, possibly broken", SymbolRegular.Warning24);
                return;
            }
            var password = string.Empty;
            //if (game.PasswordProtected)
            //{
            //    var textbox = new System.Windows.Controls.PasswordBox
            //    {
            //        Margin = new Thickness(10, 20, 10, 0),
            //        Padding = new Thickness(10, 4, 10, 4)
            //    };
            //    var dialog = DialogService.GetDialogControl();
            //    dialog.Content = textbox;
            //    dialog.Title = "Game password protected";
            //    dialog.ButtonLeftName = "Enter password";
            //    dialog.ButtonRightName = "Cancel";
            //    var result = await dialog.ShowAndWaitAsync(true);
            //    if (result is not Wpf.Ui.Controls.Interfaces.IDialogControl.ButtonPressed.Left) return;
            //    dialog.Hide();
            //    password = textbox.Password;
            //}
            //if (game.SimMods is not null && game.SimMods.Count > 0)
            //{
            //    NotificationService.Notify("Warning", "SIM mods not supported", Wpf.Ui.Common.SymbolRegular.Warning24);
            //    return;
            //}
            CancellationTokenSource = new CancellationTokenSource();

            //var dialogg = DialogService.GetDialogControl();
            //dialogg.Title = "Joining game";
            //dialogg.Content = null;
            //dialogg.ButtonLeftName = "Cancel";
            //dialogg.ButtonRightName = "Cancel";
            //dialogg.Closed += Dialog_Closed;
            //dialogg.Show();

            //await Task.Run(async () =>
            //{
            //    // todo
            //    IProgress<string> progress = new Progress<string>(d => dialogg.Content = d);

            //    // TODO reverse MapsService injection from game
            //    await MapsService
            //        .EnsureMapExistAsync(game.Mapname)
            //        .ContinueWith(async t =>
            //        {
            //            if (t.IsFaulted)
            //            {
            //                Logger.LogError(t.Exception.ToString());
            //                NotificationService.Notify("Exception", t.Exception.ToString());
            //                Application.Current.Dispatcher.BeginInvoke(dialogg.Hide, System.Windows.Threading.DispatcherPriority.Background);
            //                return;
            //            }
            //            await ServerManager
            //            .GetPatchClient()
            //            .ConfirmPatchAsync(game.FeaturedMod)
            //            .ContinueWith(t =>
            //            {
            //                if (t.IsFaulted)
            //                {
            //                    Logger.LogError(t.Exception.ToString());
            //                    NotificationService.Notify("Exception", t.Exception.ToString());
            //                    Application.Current.Dispatcher.BeginInvoke(dialogg.Hide, System.Windows.Threading.DispatcherPriority.Background);
            //                    return;
            //                }
            //                var lobby = ServerManager.GetLobbyClient();
            //                if (game.PasswordProtected) lobby.JoinGame(game.Uid, password);
            //                else lobby.JoinGame(game.Uid);
            //                Application.Current.Dispatcher.Invoke(() =>
            //                {
            //                    dialogg.Closed -= Dialog_Closed;
            //                    dialogg.Hide();
            //                });
            //            });
            //        });
            //});


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

        //private void Dialog_Closed([System.Diagnostics.CodeAnalysis.NotNull] Wpf.Ui.Controls.Dialog sender, RoutedEventArgs e)
        //{
        //    CancellationTokenSource.Cancel();
        //}
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
    