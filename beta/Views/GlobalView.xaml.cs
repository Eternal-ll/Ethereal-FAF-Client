using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Infrastructure.Utils;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GameInfoMessage = beta.Models.Server.GameInfoMessage;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for LobbiesView.xaml
    /// </summary>
    public partial class GlobalView : INotifyPropertyChanged
    {
        #region Properties

        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region SearchText

        private string _SearchText = string.Empty;

        public string SearchText
        {
            get => _SearchText;
            set
            {
                _SearchText = value;
                OnPropertyChanged(nameof(SearchText));

                View.Refresh();
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
                if (_IsPrivateGamesHidden != value)
                {
                    _IsPrivateGamesHidden = value;

                    View.Refresh();
                    OnPropertyChanged(nameof(IsPrivateGamesHidden));
                }
            }
        }
        #endregion

        #region CollectionViewSource / View
        private readonly CollectionViewSource CollectionViewSource = new();
        public ICollectionView View => CollectionViewSource.View;

        #endregion

        #region LobbiesViewSourceFilter
        private void LobbiesViewSourceFilter(object sender, FilterEventArgs e)
        {
            string searchText = _SearchText;

            var lobby = (GameInfoMessage)e.Item;
            e.Accepted = false;

            if (lobby.game_type != "custom" || lobby.featured_mod != "faf" || lobby.sim_mods.Count > 0)
                return;

            //var mapName = lobby.MapName;
            //if (mapName.Contains("gap") || mapName.Contains("crater") || mapName.Contains("astro") ||
            //    mapName.Contains("pass"))
            //    return;

            //if (lobby.num_players == 0)
            //    return;


            if (_IsPrivateGamesHidden && lobby.password_protected)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                e.Accepted = lobby.title.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) ||
                             lobby.host.Contains(searchText, StringComparison.CurrentCultureIgnoreCase);
                return;
            }
            e.Accepted = true;
        }
        #endregion

        #region IdleGames (ListBox) / IdleGamesColumns / ScrollViewer

        #region IdleGamesColumns
        private int _IdleGamesColumns = 1;
        public int IdleGamesColumns
        {
            get => _IdleGamesColumns;
            set
            {
                if (value != _IdleGamesColumns)
                {
                    _IdleGamesColumns = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IdleGamesColumns)));
                }
            }
        }
        #endregion


        public ScrollViewer IdleGamesScrollViewer { get; }
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
                        View.SortDescriptions.Clear();
                    }
                    if (value && View.SortDescriptions.Count == 0)
                    {
                        SelectedSort = SortDescriptions[0];
                    }
                }
            }
        }
        #endregion

        #region Sort properties

        public SortDescription[] SortDescriptions => new SortDescription[]
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
                    PropertyChanged?.Invoke(this, new(nameof(SelectedSort)));
                    PropertyChanged?.Invoke(this, new(nameof(SortDirection)));
                    //SortDirection = ListSortDirection.Ascending;
                    View.SortDescriptions.Clear();
                    View.SortDescriptions.Add(value);
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

        #endregion

        public ObservableCollection<GameInfoMessage> LiveGames => GamesServices.LiveGames;

        private readonly IGamesServices GamesServices;
        private readonly object _lock = new();

        #endregion

        #region CTOR
        public GlobalView()
        {
            InitializeComponent();

            IdleGamesScrollViewer = Tools.FindChild<ScrollViewer>(IdleGames);

            GamesServices = App.Services.GetRequiredService<IGamesServices>();

            var grouping = new PropertyGroupDescription(nameof(GameInfoMessage.featured_mod));
            CollectionViewSource.Filter += LobbiesViewSourceFilter;
            CollectionViewSource.GroupDescriptions.Add(grouping);

            BindingOperations.EnableCollectionSynchronization(GamesServices.IdleGames, _lock);
            BindingOperations.EnableCollectionSynchronization(GamesServices.LiveGames, _lock);

            CollectionViewSource.Source = GamesServices.IdleGames;

            DataContext = this;
        }
        #endregion

        private void IdleGames_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 600) IdleGamesColumns = 1;
            else IdleGamesColumns = Convert.ToInt32((e.NewSize.Width - 60) / (330));
        }
    }
}
