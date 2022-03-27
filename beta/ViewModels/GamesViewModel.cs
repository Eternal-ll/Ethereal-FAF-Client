using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.Models.Server.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace beta.ViewModels
{
    public abstract class GamesViewModel : Base.ViewModel
    {
        public abstract GameType GameType { get; }

        private readonly ISocialService SocialService;
        private readonly IGamesService GamesService;
        public GamesViewModel()
        {
            SocialService = App.Services.GetService<ISocialService>();
            GamesService = App.Services.GetService<IGamesService>();
            GamesService.NewGame += OnNewGame;
            GamesService.GameUpdated += OnGameUpdated;
            GamesService.GameRemoved += OnGameRemoved;

            IdleGamesViewSource.Filter += OnGameFilter;
            LiveGamesViewSource.Filter += OnGameFilter;
        }

        public ObservableCollection<GameInfoMessage> Games { get; } = new();

        public ICollectionView IdleGames => IdleGamesViewSource.View;
        public ICollectionView LiveGames => LiveGamesViewSource.View;

        private readonly CollectionViewSource IdleGamesViewSource = new();
        private readonly CollectionViewSource LiveGamesViewSource = new();

        #region FilterText
        private string _FilterText;
        public string FilterText
        {
            get => _FilterText;
            set
            {
                if (Set(ref _FilterText, value))
                {
                    RefreshGames();
                }
            }
        }
        #endregion

        #region IsLiveGamesOnView
        private bool _IsLiveGamesOnView;
        public bool IsLiveGamesOnView
        {
            get => _IsLiveGamesOnView;
            set
            {
                if (Set(ref _IsLiveGamesOnView, value))
                {
                    if (!string.IsNullOrWhiteSpace(FilterText))
                    {
                        FilterText = null;
                    }
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
                    RefreshGames();
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
                if (_IsSortEnabled != value)
                {
                    _IsSortEnabled = value;

                    //View.Refresh();
                    OnPropertyChanged(nameof(IsSortEnabled));

                    if (!value)
                    {
                        ClearSort();
                    }
                    //if (value && View.SortDescriptions.Count == 0)
                    //{
                    //    SelectedSort = SortDescriptions[0];
                    //}
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
                if (value != _SelectedSort)
                {
                    _SelectedSort = value;
                    OnPropertyChanged(nameof(SelectedSort));
                    OnPropertyChanged(nameof(SortDirection));

                    SetSort(value);
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

        private void ClearSort()
        {
            if (IsLiveGamesOnView)
            {
                LiveGamesViewSource.LiveSortingProperties.Clear();
            }
            else
            {
                IdleGamesViewSource.LiveSortingProperties.Clear();
            }
        }

        private void SetSort(SortDescription sort)
        {
            ClearSort();
            if (IsLiveGamesOnView)
            {
                LiveGamesViewSource.LiveSortingProperties.Add(sort.PropertyName);
            }
            else
            {
                IdleGamesViewSource.LiveSortingProperties.Add(sort.PropertyName);
            }
        }


        protected abstract bool FilterGame(GameInfoMessage game);
        private void OnGameFilter(object sender, FilterEventArgs e)
        {
            var game = (GameInfoMessage)e.Item;
            if (IsFoesGamesHidden)
            {
                var foes = SocialService.GetFoes();
                for (int i = 0; i < foes.Count; i++)
                {
                    if (foes[i].login.Equals(game.host, System.StringComparison.OrdinalIgnoreCase))
                    {
                        e.Accepted = false;
                        return;
                    }
                }
            }
            e.Accepted = FilterGame(game);
        }

        private void RefreshGames()
        {
            if (IsLiveGamesOnView) LiveGames.Refresh(); else IdleGames.Refresh();
        }

        private bool UpdateGame(GameInfoMessage updatedGame)
        {
            var games = Games;
            for (int i = 0; i < games.Count; i++)
            {
                var game = games[i];
                if (game.uid == updatedGame.uid)
                {
                    game.Update(updatedGame);
                    return true;
                }
            }
            return false;
        }

        private void OnGameRemoved(object sender, Infrastructure.EventArgs<GameInfoMessage> e)
        {
            var game = e.Arg;
            if (game.GameType != GameType) return;

            if (Games.Remove(game))
            {

            }
        }

        private void OnGameUpdated(object sender, Infrastructure.EventArgs<GameInfoMessage> e)
        {
            var game = e.Arg;
            if (game.GameType != GameType) return;

            if (UpdateGame(game))
            {

            }
        }

        private void OnNewGame(object sender, Infrastructure.EventArgs<GameInfoMessage> e)
        {
            var game = e.Arg;
            if (game.GameType != GameType) return;
            Games.Add(game);
        }
    }
    public class CustomGamesViewModel : GamesViewModel
    {
        public override GameType GameType { get; } = GameType.Custom;

        protected override bool FilterGame(GameInfoMessage game)
        {
            return true;
        }
    }
    public class CoopGamesViewModel : GamesViewModel
    {
        public override GameType GameType { get; } = GameType.Coop;

        protected override bool FilterGame(GameInfoMessage game)
        {
            return true;
        }
    }
    public class MatchMakerGamesViewModel : GamesViewModel
    {
        public override GameType GameType { get; } = GameType.MatchMaker;

        protected override bool FilterGame(GameInfoMessage game)
        {
            return true;
        }
    }
}
