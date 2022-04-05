using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.Models.Server.Enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace beta.ViewModels
{
    public class PropertyFilterDescription
    {
        public PropertyFilterDescription(string displayedProperty, string property)
        {
            DisplayedProperty = displayedProperty;
            Property = property;
        }

        public string DisplayedProperty { get; }
        public string Property { get; }
    }
    public enum FilterDescription
    {
        None,
        Contains,
        StartsWith,
        EndsWith,
    }
    public class BlockedMapDescription
    {
        public BlockedMapDescription(string name, FilterDescription filter)
        {
            Name = name;
            Filter = filter;
        }

        public string Name { get; }
        public FilterDescription Filter { get; }

    }
    public abstract class GamesViewModel : Base.ViewModel
    {
        public abstract GameType GameType { get; }
        public abstract GameState GameState { get; }

        private readonly ISocialService SocialService;
        private readonly IGamesService GamesService;

        private readonly object _lock = new();
        private Dispatcher Dispatcher => GamesViewSource.Dispatcher;
        public GamesViewModel()
        {
            SocialService = App.Services.GetService<ISocialService>();
            GamesService = App.Services.GetService<IGamesService>();

            Games = new();
            GamesWithBlockedMap = new();
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                GamesViewSource = new();
                BindingOperations.EnableCollectionSynchronization(Games, _lock);
                GamesViewSource.Source = Games;
                GamesViewSource.IsLiveSortingRequested = true;
                GamesViewSource.Filter += OnGameFilter;
            }));

            HandleGames(GamesService.Games.ToArray());

            GamesService.NewGameReceived += OnNewGame;
            GamesService.GamesReceived += OnGamesReceived;
            GamesService.GameUpdated += OnGameUpdated;
            GamesService.GameRemoved += OnGameRemoved;


            //Foes = SocialService.GetFoes;
            //Friends = SocialService.GetFriends;
        }

        public ObservableCollection<GameInfoMessage> Games { get; private set; }
        public ObservableCollection<GameInfoMessage> GamesWithBlockedMap { get; private set; }

        public ICollectionView GamesView => GamesViewSource.View;

        private CollectionViewSource GamesViewSource;

        #region Foes
        private string[] _Foes;
        public string[] Foes
        {
            get => _Foes;
            set
            {
                if (Set(ref _Foes, value) && IsFoesGamesHidden)
                {
                    RefreshGameView();
                }
            }
        }
        #endregion

        #region Friends
        private string[] _Friends;
        public string[] Friends
        {
            get => _Friends;
            set
            {
                if (Set(ref _Friends, value) && IsOnlyFriendsGamesVisible)
                {
                    RefreshGameView();
                }
            }
        }
        #endregion

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
                            if (!int.TryParse(value, out _))
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
                    if (Foes?.Length > 1) RefreshGameView();
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

                    if (Friends?.Length > 1) RefreshGameView();
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
            new SortDescription(nameof(GameInfoMessage.AverageRating), ListSortDirection.Ascending)
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

            if (IsOnlyFriendsGamesVisible)
            {
                var friends = Friends;
                bool passed = false;

                if (friends is not null)
                {
                    foreach (var foe in friends)
                        if (foe.Equals(game.host, StringComparison.OrdinalIgnoreCase))
                            passed = true;

                    if (!passed) return false;
                }
            }

            if (IsFoesGamesHidden)
            {
                var foes = Foes;

                if (foes is not null)
                    foreach (var foe in foes)
                        if (foe.Equals(game.host, StringComparison.OrdinalIgnoreCase))
                            return false;
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

        private void OnGameFilter(object sender, FilterEventArgs e)
        {
            var game = (GameInfoMessage)e.Item;
            //e.Accepted = false;
            //if (game.State != GameState.Open) return;
            e.Accepted = CommonFilter(game);
        }

        private bool TryGetIndexOfGame(long uid, out int id)
        {
            var games = Games;
            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].uid == uid)
                {
                    id = i;
                    return true;
                }
            }
            id = -1;
            return false;
        }

        private void OnGameRemoved(object sender, GameInfoMessage game)
        {
            if (game.GameType != GameType || game.State != GameState || game.FeaturedMod != FeaturedMod.FAF) return;

            //Dispatcher.BeginInvoke(() =>
            //{
            if (TryGetIndexOfGame(game.uid, out var id))
                {
                    Games.RemoveAt(id);
                }
            //});
        }

        private void OnGameUpdated(object sender, GameInfoMessage game)
        {
            if (game.GameType != GameType || game.State != GameState || game.FeaturedMod != FeaturedMod.FAF) return;

            //Dispatcher.BeginInvoke(() =>
            //{
                if (TryGetIndexOfGame(game.uid, out var id))
                {
                    Games[id] = game;
                }
            //});
        }

        private void OnNewGame(object sender, GameInfoMessage game)
        {
            if (game.GameType != GameType || game.State != GameState || game.FeaturedMod != FeaturedMod.FAF) return;
            //Dispatcher.BeginInvoke(() =>
            //{
                if (TryGetIndexOfGame(game.uid, out var id))
                {
                    Games[id] = game;
                }
                else
                {
                    Games.Add(game);
                }
            //});
        }

        private void OnGamesReceived(object sender, GameInfoMessage[] e) => HandleGames(e);
        private void HandleGames(GameInfoMessage[] e)
        {
            var games = Games;
            foreach (var game in e)
            {
                if (game.GameType != GameType || game.State != GameState || game.FeaturedMod != FeaturedMod.FAF) continue;

                games.Add(game);
            }
        }

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
        private bool _IsGridView = true;
        public bool IsGridView
        {
            get => _IsGridView;
            set
            {
                if (Set(ref _IsGridView, value))
                {
                    //IsListView = !value;
                }
            }
        }
        #endregion

        //#region IsListView
        //private bool _IsListView;
        //public bool IsListView
        //{
        //    get => _IsListView;
        //    set
        //    {
        //        if (Set(ref _IsListView, value))
        //        {
        //            IsGridView = !value;
        //        }
        //    }
        //}
        //#endregion

        #region IsExtendedViewEnabled
        private bool _IsExtendedViewEnabled;
        public bool IsExtendedViewEnabled
        {
            get => _IsExtendedViewEnabled;
            set => Set(ref _IsExtendedViewEnabled, value);
        }
        #endregion

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GamesService.NewGameReceived -= OnNewGame;
                GamesService.GamesReceived -= OnGamesReceived;
                GamesService.GameUpdated -= OnGameUpdated;
                GamesService.GameRemoved -= OnGameRemoved;
                GamesViewSource.Filter -= OnGameFilter;
                GamesViewSource.Source = null;
                BindingOperations.DisableCollectionSynchronization(Games);
                Games.Clear();
            }
            base.Dispose(disposing);
        }
    }
    public class CustomGamesViewModel : GamesViewModel
    {
        public override GameType GameType { get; } = GameType.Custom;

        public override GameState GameState => GameState.Open;

        protected override bool FilterGame(GameInfoMessage game)
        {
            return true;
        }
    }
    public class CustomLiveGamesViewModel : GamesViewModel
    {
        public override GameType GameType { get; } = GameType.Custom;

        public override GameState GameState => GameState.Playing;

        protected override bool FilterGame(GameInfoMessage game)
        {
            return true;
        }
    }
    public class CoopGamesViewModel : GamesViewModel
    {
        public override GameType GameType { get; } = GameType.Coop;
        public override GameState GameState => GameState.Open;

        protected override bool FilterGame(GameInfoMessage game)
        {
            return true;
        }
    }
    public class MatchMakerGamesViewModel : GamesViewModel
    {
        public override GameType GameType { get; } = GameType.MatchMaker;
        public override GameState GameState => GameState.Open;

        protected override bool FilterGame(GameInfoMessage game)
        {
            return true;
        }
    }
}
