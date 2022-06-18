using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Enums;
using beta.Models.Server;
using beta.Models.Server.Enums;
using beta.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace beta.ViewModels
{
    public abstract class GamesViewModel : Base.ViewModel
    {
        public abstract GameType GameType { get; }
        public abstract GameState GameState { get; }

        private readonly ISocialService SocialService;
        private readonly ISessionService SessionService;
        protected readonly IGamesService GamesService;
        private readonly IPlayersService PlayersService;
        private readonly IMapsService MapsService;

        private readonly object _lock = new();
        private Dispatcher Dispatcher => GamesViewSource.Dispatcher;
        public GamesViewModel()
        {
            SocialService = App.Services.GetService<ISocialService>();
            SessionService = App.Services.GetService<ISessionService>();
            GamesService = App.Services.GetService<IGamesService>();
            PlayersService = App.Services.GetService<IPlayersService>();
            MapsService = App.Services.GetService<IMapsService>();

            Games = new();
            GamesWithBlockedMap = new();
            GamesViewSource = new();
            BindingOperations.EnableCollectionSynchronization(Games, _lock);
            GamesViewSource.Source = Games;
            GamesViewSource.IsLiveSortingRequested = true;
            GamesViewSource.Filter += OnGameFilter;

            GamesService.NewGameReceived += OnNewGame;
            //GamesService.GameUpdated += OnGameUpdated;
            GamesService.GameEnd += OnGameEnd;
            GamesService.GameLaunched += OnGameLaunched;
            GamesService.GameClosed += GamesService_GameClosed;
        }

        public ObservableCollection<GameInfoMessage> Games { get; private set; }
        public ObservableCollection<GameInfoMessage> GamesWithBlockedMap { get; private set; }

        public ICollectionView GamesView => GamesViewSource.View;

        protected CollectionViewSource GamesViewSource;

        #region SelectedGame
        private GameInfoMessage _SelectedGame;
        public GameInfoMessage SelectedGame
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

        #region FilterText
        private string _FilterText;
        public string FilterText
        {
            get => _FilterText;
            set
            {
                if (Set(ref _FilterText, value))
                {
                    switch (SelectedFilterDescription.Property)
                    {
                        case nameof(GameInfoMessage.max_players):
                        case nameof(GameInfoMessage.num_players):
                        case nameof(GameInfoMessage.rating_max):
                        case nameof(GameInfoMessage.rating_min):
                        case nameof(GameInfoMessage.AverageRating):
                            if (!int.TryParse(value, out _) && Games.Count == ((ListCollectionView)GamesView).Count)
                            {
                                return;
                            }
                            break;
                    }

                    RefreshGameView();
                }
            }
        }
        #endregion

        #region FilterDescription
        public static PropertyFilterDescription[] FilterDescriptions => new PropertyFilterDescription[]
        {
            //new("None", null),
            new("Title",(nameof(GameInfoMessage.title))),
            new("Host name",(nameof(GameInfoMessage.host))),
            new("Map name",(nameof(GameInfoMessage.mapname))),
            new("Max players",(nameof(GameInfoMessage.max_players))),
            new("Num players",(nameof(GameInfoMessage.num_players))),
            new("Max rating",(nameof(GameInfoMessage.rating_max))),
            new("Min rating",(nameof(GameInfoMessage.rating_min))),
            //new("",(nameof(GameInfoMessage.AverageRating)))
        };

        #endregion

        #region SelectedFilterDescription
        private PropertyFilterDescription _SelectedFilterDescription;
        public PropertyFilterDescription SelectedFilterDescription
        {
            get => _SelectedFilterDescription ??= FilterDescriptions[0];
            set
            {
                if (Set(ref _SelectedFilterDescription, value))
                {
                    if (!string.IsNullOrWhiteSpace(FilterText)) RefreshGameView();
                }
            }
        }

        #endregion

        #region IsFoesGamesHidden
        private bool _IsFoesGamesHidden;
        public bool IsFoesGamesHidden
        {
            get => _IsFoesGamesHidden;
            set
            {
                if (Set(ref _IsFoesGamesHidden, value))
                {
                    if (value)
                    {
                        _IsOnlyFriendsGamesVisible = false;
                        OnPropertyChanged(nameof(IsOnlyFriendsGamesVisible));
                    }
                    RefreshGameView();
                }
            }
        }
        #endregion

        #region IsOnlyFriendsGamesVisible
        private bool _IsOnlyFriendsGamesVisible;
        public bool IsOnlyFriendsGamesVisible
        {
            get => _IsOnlyFriendsGamesVisible;
            set
            {
                if (Set(ref _IsOnlyFriendsGamesVisible, value))
                {
                    if (value)
                    {
                        _IsFoesGamesHidden = false;
                        OnPropertyChanged(nameof(IsFoesGamesHidden));
                    }
                    RefreshGameView();
                }
            }
        }
        #endregion
            
        #region IsGamesWithEnforcedRatingVisible
        private bool _IsOnlyGamesWithEnforcedRatingVisible;
        public bool IsOnlyGamesWithEnforcedRatingVisible
        {
            get => _IsOnlyGamesWithEnforcedRatingVisible;
            set
            {
                if (Set(ref _IsOnlyGamesWithEnforcedRatingVisible, value))
                {
                    RefreshGameView();
                }
            }
        }
        #endregion

        #region IsPrivateGamesHidden
        private bool _IsPrivateGamesHidden;
        public bool IsPrivateGamesHidden
        {
            get => _IsPrivateGamesHidden;
            set
            {
                if (Set(ref _IsPrivateGamesHidden, value))
                {
                    RefreshGameView();
                }
            }
        }
        #endregion

        #region IsSortEnabled
        private bool _IsSortEnabled;
        public bool IsSortEnabled
        {
            get => _IsSortEnabled;
            set
            {
                if (Set(ref _IsSortEnabled, value))
                {
                    if (value)
                    {
                        SelectedSort = SortDescriptions[0];
                    }
                    else
                    {
                        GamesViewSource.LiveSortingProperties.Clear();
                        GamesViewSource.SortDescriptions.Clear();
                    }
                }
            }
        }
        #endregion

        #region SortDescriptions
        public static SortDescription[] SortDescriptions => new SortDescription[]
        {
            new SortDescription(nameof(GameInfoMessage.title), ListSortDirection.Ascending),
            // Requires IComaprable TODO
            //new SortDescription(nameof(GameInfoMessage.Host), ListSortDirection.Ascending),
            new SortDescription(nameof(GameInfoMessage.mapname), ListSortDirection.Ascending),
            new SortDescription(nameof(GameInfoMessage.max_players), ListSortDirection.Ascending),
            new SortDescription(nameof(GameInfoMessage.num_players), ListSortDirection.Ascending),
            new SortDescription(nameof(GameInfoMessage.rating_max), ListSortDirection.Ascending),
            new SortDescription(nameof(GameInfoMessage.rating_min), ListSortDirection.Ascending),
            new SortDescription(nameof(GameInfoMessage.AverageRating), ListSortDirection.Ascending),
            new SortDescription(nameof(GameInfoMessage.Duration), ListSortDirection.Ascending),
            new SortDescription(nameof(GameInfoMessage.password_protected), ListSortDirection.Ascending),
        }; 
        #endregion

        #region SelectedSort
        private SortDescription _SelectedSort;
        public SortDescription SelectedSort
        {
            get => _SelectedSort;
            set
            {
                if (Set(ref _SelectedSort, value))
                {
                    OnPropertyChanged(nameof(SortDirection));
                    
                    GamesViewSource.SortDescriptions.Clear();
                    GamesViewSource.LiveSortingProperties.Add(value.PropertyName);
                    GamesViewSource.SortDescriptions.Add(value);
                }
            }
        }
        #endregion

        #region SortDirection
        public ListSortDirection SortDirection
        {
            get => SelectedSort.Direction;
            set
            {
                if (SelectedSort.Direction == ListSortDirection.Ascending)
                {
                    SelectedSort = new(SelectedSort.PropertyName, ListSortDirection.Descending);
                }
                else SelectedSort = new(SelectedSort.PropertyName, ListSortDirection.Ascending);
            }
        }
        #endregion

        #region ChangeSortDirectionCommmand
        private ICommand _ChangeSortDirectionCommmand;
        public ICommand ChangeSortDirectionCommmand => _ChangeSortDirectionCommmand ??= new LambdaCommand(OnChangeSortDirectionCommmand);
        public void OnChangeSortDirectionCommmand(object parameter) => SortDirection = ListSortDirection.Ascending;
        #endregion

        #region Maps black list area

        #region IsMapsBlacklistEnabled
        private bool _IsMapsBlacklistEnabled = true; // TODO default value
        public bool IsMapsBlacklistEnabled
        {
            get => _IsMapsBlacklistEnabled;
            set
            {
                if (Set(ref _IsMapsBlacklistEnabled, value))
                {
                    if (MapsBlackList.Count > 0)
                        RefreshGameView();
                }
            }
        }
        #endregion

        public ObservableCollection<BlockedMapDescription> MapsBlackList { get; } = new();

        public static FilterDescription[] MapFilterDescriptions => new FilterDescription[]
        {
            FilterDescription.Contains,
            FilterDescription.StartsWith,
            FilterDescription.EndsWith,
        };

        #region SelectedMapFilterDescription
        private FilterDescription _SelectedMapFilterDescription = MapFilterDescriptions[0];
        public FilterDescription SelectedMapFilterDescription
        {
            get => _SelectedMapFilterDescription;
            set => Set(ref _SelectedMapFilterDescription, value);
        }

        #endregion

        #region InputKeyWord
        private string _InputKeyWord = string.Empty;
        public string InputKeyWord
        {
            get => _InputKeyWord;
            set => Set(ref _InputKeyWord, value);
        }
        #endregion

        #region AddKeyWordCommand
        private ICommand _AddKeyWordCommand;
        public ICommand AddKeyWordCommand => _AddKeyWordCommand ?? new LambdaCommand(OnAddKeyWordCommand, CanAddKeyWordCommand);
        private bool CanAddKeyWordCommand(object parameter) => !string.IsNullOrWhiteSpace(_InputKeyWord);
        public void OnAddKeyWordCommand(object parameter)
        {
            if (parameter is null) return;
            if (string.IsNullOrWhiteSpace(parameter.ToString())) return;

            BlockedMapDescription filter = new(parameter.ToString(), SelectedMapFilterDescription);

            var blocked = MapsBlackList;

            foreach (var mapFilter in blocked)
            {
                if (mapFilter.Name.Equals(filter.Name, StringComparison.OrdinalIgnoreCase) && mapFilter.Filter == filter.Filter) return;
            }
            blocked.Add(filter);
            InputKeyWord = string.Empty;

            if (IsMapsBlacklistEnabled) RefreshGameView();
        }
        #endregion

        #region RemoveKeyWordCommand
        private ICommand _RemoveKeyWordCommand;
        public ICommand RemoveKeyWordCommand => _RemoveKeyWordCommand ??= new LambdaCommand(OnRemoveKeyWordCommand);
        public void OnRemoveKeyWordCommand(object parameter)
        {
            if (parameter is BlockedMapDescription filter)
                if (MapsBlackList.Remove(filter) && IsMapsBlacklistEnabled) RefreshGameView();
        }
        #endregion

        #endregion

        #region View toggles

        #region IsDataGridView
        private bool _IsDataGridView;
        public bool IsDataGridView
        {
            get => _IsDataGridView;
            set
            {
                if (Set(ref _IsDataGridView, value))
                {

                }
            }
        }
        #endregion

        #region IsGridView
        private bool _IsGridView = false;
        public bool IsGridView
        {
            get => _IsGridView;
            set
            {
                if (Set(ref _IsGridView, value))
                {

                }
            }
        }
        #endregion

        #region IsExtendedViewEnabled
        private bool _IsExtendedViewEnabled;
        public bool IsExtendedViewEnabled
        {
            get => _IsExtendedViewEnabled;
            set => Set(ref _IsExtendedViewEnabled, value);
        }
        #endregion

        #endregion

        private void RefreshGameView()
        {
            GamesWithBlockedMap.Clear();
            GamesView.Refresh();
        }

        protected abstract bool FilterGame(GameInfoMessage game);
        private bool CommonFilter(GameInfoMessage game)
        {
            if (IsPrivateGamesHidden && game.password_protected) return false;

            if(IsOnlyGamesWithEnforcedRatingVisible && !game.enforce_rating_range) return false;

            if (IsMapsBlacklistEnabled)
            {
                var blocked = MapsBlackList;
                if (blocked.Count > 0)
                for (int i = 0; i < blocked.Count; i++)
                    {
                        var filterDesc = blocked[i];
                        bool isBlocked = false;
                        switch (filterDesc.Filter)
                        {
                            case FilterDescription.Contains:
                                isBlocked = game.MapName.Contains(filterDesc.Name, StringComparison.OrdinalIgnoreCase);
                                break;
                            case FilterDescription.StartsWith:
                                isBlocked = game.MapName.StartsWith(filterDesc.Name, StringComparison.OrdinalIgnoreCase);
                                break;
                            case FilterDescription.EndsWith:
                                isBlocked = game.MapName.EndsWith(filterDesc.Name, StringComparison.OrdinalIgnoreCase);
                                break;
                        }
                        if (isBlocked)
                        {
                            GamesWithBlockedMap.Add(game);
                            return false;
                        }
                    }
            }

            if (game.Host is PlayerInfoMessage player)
            {
                if (IsOnlyFriendsGamesVisible && player.RelationShip != PlayerRelationShip.Friend)
                {
                    return false;
                }

                if (IsFoesGamesHidden && player.RelationShip == PlayerRelationShip.Foe)
                {
                    return false;
                }
            }

            var filter = FilterText;

            if (!string.IsNullOrWhiteSpace(FilterText))
            {
                var selectedFilter = SelectedFilterDescription;
                switch (selectedFilter.Property)
                {
                    case nameof(GameInfoMessage.title):
                        if (!game.title.Contains(filter)) return false;
                        break;
                    case nameof(GameInfoMessage.host):
                        if (!game.host.Contains(filter)) return false;
                        break;
                    case nameof(GameInfoMessage.mapname):
                        if (!game.mapname.Contains(filter)) return false;
                        break;
                }
                if (int.TryParse(FilterText, out var number))
                    switch (selectedFilter.Property)
                    {
                        case nameof(GameInfoMessage.max_players):
                            if (game.max_players != number) return false;
                            break;
                        case nameof(GameInfoMessage.num_players):
                            if (game.num_players != number) return false;
                            break;
                        case nameof(GameInfoMessage.rating_max):
                            if (game.rating_max.HasValue)
                            {
                                if (game.rating_max < number) return false;
                            }
                            else return false;
                            break;
                        case nameof(GameInfoMessage.rating_min):
                            if (game.rating_min.HasValue)
                            {
                                if (game.rating_min > number) return false;
                            }
                            else return false;
                            break;
                        case nameof(GameInfoMessage.AverageRating):
                            if (game.AverageRating < number) return false;
                            break;
                    }
            }

            return FilterGame(game);
        }
        private void OnGameFilter(object sender, FilterEventArgs e) => 
            e.Accepted = CommonFilter((GameInfoMessage)e.Item);

        #region Commands

        #region RefreshCommand
        private ICommand _RefreshCommand;
        public ICommand RefreshCommand => _RefreshCommand ??= new LambdaCommand(OnRefreshCommand);
        private void OnRefreshCommand(object parameter)
        {
            //BindingOperations.EnableCollectionSynchronization(Games, _lock);
            GamesViewSource.Source = null;
            Games.Clear();
            HandleGames(GamesService.Games.ToArray());
            GamesViewSource.Source = Games;
            OnPropertyChanged(nameof(GamesView));
        }
        #endregion

        #endregion

        private bool TryGetIndexOfGame(long uid, out int id)
        {
            var games = Games;
            for (int i = 0; i < games.Count; i++)
            {
                if (games[i] is null)
                {
                    games.RemoveAt(i);
                    continue;
                }   
                if (games[i].uid == uid)
                {
                    id = i;
                    return true;
                }
            }
            id = -1;
            return false;
        }

        private bool IsNotRequiredGame(GameInfoMessage game) => (game.State == GameState || GameState == GameState.None) && game.GameType == GameType;

        private void OnGameLaunched(object sender, GameInfoMessage e)
        {
            if (e.GameType != GameType && GameState == GameState.Open) return;

            e.State = GameState.Launched;
            Task.Run(() =>
            {
                Thread.Sleep(5000);
                e.State = GameState.Playing;
                if (GameType == GameType.MatchMaker)
                {
                    Thread.Sleep(5000);
                    e.OldState = GameState.Playing;
                    e.State = GameState.None;
                    e.State = GameState.Playing;
                    return;
                }
                Thread.Sleep(7000);
                Games.Remove(e);
            });
        }


        private void GamesService_GameClosed(object sender, GameInfoMessage e)
        {
            if (e.GameType != GameType) return;

            e.State = GameState.Closed;
            Task.Run(() =>
            {
                Thread.Sleep(7000);
                Games.Remove(e);
            });
        }
        private void OnGameEnd(object sender, GameInfoMessage game)
        {
            if (game.GameType != GameType) return;

            for (int i = 0; i < Games.Count; i++)
            {
                var cgame = Games[i];
                if (cgame.uid == game.uid)
                {
                    Games.RemoveAt(i);
                }
            }
        }

        protected void HandleGameData(GameInfoMessage game)
        {
            // TODO
            // 1. Optimize filling of in game teams
            // 2. Fill host once, or re-fill if host instance is UnknownPlayer

            //AddInGameTeams(game);
            //var host = GetPlayer(game.host);
            //if (host is PlayerInfoMessage)
            //{
            //    game.Host = host;
            //}

            if (TryGetIndexOfGame(game.uid, out var id))
            {
                Games[id] = game;
            }
            else
            {
                //game.Map = await MapsService.GetGameMap(game.mapname);
                Games.Add(game);
            }
        }

        private void OnGameUpdated(object sender, GameInfoMessage game)
        {
            if (!IsNotRequiredGame(game)) return;
            HandleGameData(game);
        }

        private void OnNewGame(object sender, GameInfoMessage game)
        {
            if (!IsNotRequiredGame(game)) return;
            HandleGameData(game);
        }
        private void HandleGames(GameInfoMessage[] e)
        {
            var games = Games;
            foreach (var game in e)
            {
                if (!IsNotRequiredGame(game)) continue;

                games.Add(game);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GamesService.NewGameReceived -= OnNewGame;
                GamesService.GameUpdated -= OnGameUpdated;
                GamesService.GameEnd -= OnGameEnd;
                GamesViewSource.Filter -= OnGameFilter;
                GamesViewSource.Source = null;
                BindingOperations.DisableCollectionSynchronization(Games);
                Games.Clear();
            }
            base.Dispose(disposing);
        }
    }
    /// <summary>
    /// Custom open games
    /// </summary>
    public class CustomGamesViewModel : GamesViewModel
    {
        private readonly NavigationService NavigationService;

        public override GameType GameType { get; } = GameType.Custom;
        public override GameState GameState => GameState.Open;
        public CustomLiveGamesViewModel LiveGamesViewModel { get; }
        private CustomGamesView LiveGamesView { get; }

        public CustomGamesViewModel()
        {
            IsGridView = true;
        }

        public CustomGamesViewModel(NavigationService nav) : this()
        {
            NavigationService = nav;
            LiveGamesViewModel = new(nav, this);
            LiveGamesView = new()
            {
                DataContext = LiveGamesViewModel
            };
            _ShowLiveGamesCommand = new LambdaCommand(OnShowLiveGamesCommand);
        }

        protected override bool FilterGame(GameInfoMessage game)
        {
            return true;
        }

        #region ShowLiveGamesCommand
        private ICommand _ShowLiveGamesCommand;
        public ICommand ShowLiveGamesCommand => _ShowLiveGamesCommand;
        private void OnShowLiveGamesCommand(object parameter) => NavigationService.Navigate(LiveGamesView);
        #endregion
    }
    /// <summary>
    /// Custom live games
    /// </summary>
    public class CustomLiveGamesViewModel : GamesViewModel
    {
        private readonly NavigationService NavigationService;

        public override GameType GameType => GameType.Custom;
        public override GameState GameState => GameState.Playing;
        public CustomGamesViewModel OpenGamesViewModel { get; }
        public CustomLiveGamesViewModel()
        {
            GamesService.GameLaunched += GamesService_GameLaunched;
        }
        public CustomLiveGamesViewModel(NavigationService nav, CustomGamesViewModel openGamesViewModel) : this()
        {
            NavigationService = nav;
            OpenGamesViewModel = openGamesViewModel;
            _ShowOpenGamesCommand = new LambdaCommand(OnShowOpenGamesCommand);
        }

        private void GamesService_GameLaunched(object sender, GameInfoMessage e)
        {
            HandleGameData(e);
        }

        protected override bool FilterGame(GameInfoMessage game)
        {
            return true;
        }

        #region ShowOpenGamesCommand
        private ICommand _ShowOpenGamesCommand;
        public ICommand ShowOpenGamesCommand => _ShowOpenGamesCommand;
        private void OnShowOpenGamesCommand(object parameter) => NavigationService.GoBack();
        #endregion
    }
    /// <summary>
    /// Coop open games
    /// </summary>
    public class CoopGamesViewModel : GamesViewModel
    {
        public override GameType GameType => GameType.Coop;
        public override GameState GameState => GameState.Open;

        protected override bool FilterGame(GameInfoMessage game)
        {
            return true;
        }
    }
    /// <summary>
    /// Matchmaker games
    /// </summary>
    public class MatchMakerGamesViewModel : GamesViewModel
    {
        public override GameType GameType => GameType.MatchMaker;

        public override GameState GameState => GameState.None; // Open + Playing

        #region IsFilterByRatingTypeEnabled
        private bool _IsFilterByRatingTypeEnabled;
        public bool IsFilterByRatingTypeEnabled
        {
            get => _IsFilterByRatingTypeEnabled;
            set
            {
                if (Set(ref _IsFilterByRatingTypeEnabled, value))
                {
                    GamesView.Refresh();
                }
            }
        }
        #endregion

        #region SelectedRatingType
        private RatingType _SelectedRatingType;
        public RatingType SelectedRatingType
        {
            get => _SelectedRatingType;
            set
            {
                if (Set(ref _SelectedRatingType, value) && IsFilterByRatingTypeEnabled)
                {
                    GamesView.Refresh();
                }
            }
        }
        #endregion

        protected override bool FilterGame(GameInfoMessage game)
        {
            if (IsFilterByRatingTypeEnabled)
            {
                return game.RatingType == SelectedRatingType;
            }
            return true;
        }

        public MatchMakerGamesViewModel()
        {
            IsGridView = true;
            GamesViewSource.SortDescriptions.Add(new SortDescription(nameof(GameInfoMessage.State), ListSortDirection.Ascending));
        }
    }
}
