using beta.Infrastructure.Commands;
using beta.Infrastructure.Services.Interfaces;
using beta.Infrastructure.Utils;
using beta.Models.Server.Enums;
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
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }
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

            if (lobby.game_type != "custom" || lobby.FeaturedMod != FeaturedMod.FAF || lobby.sim_mods != null)
                return;

            if (_IsMapsBlacklistEnabled && MapsBlackList.Count > 0)
            {
                var items = MapsBlackList;
                for (int i = 0; i < items.Count; i++)
                    if (lobby.mapname.Contains(items[i], StringComparison.OrdinalIgnoreCase))
                        return;
            }

            if (_IsFoesGamesHidden && lobby.Host?.RelationShip == PlayerRelationShip.Foe)
                return;

            if (_IsPrivateGamesHidden && lobby.password_protected)
                return;
            

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

                    CollectionViewSource.LiveSortingProperties.Clear();
                    CollectionViewSource.LiveSortingProperties.Add(value.PropertyName);
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

        #region View toggles

        #region IsGridView
        private bool _IsGridView = true;
        public bool IsGridView
        {
            get => _IsGridView;
            set
            {
                if (Set(ref _IsGridView, value))
                {
                    IsListView = !value;
                }
            }
        }
        #endregion

        #region IsListView
        private bool _IsListView;
        public bool IsListView
        {
            get => _IsListView;
            set
            {
                if (Set(ref _IsListView, value))
                {
                    IsGridView = !value;
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

        #region IsPrivateGamesHidden

        private bool _IsPrivateGamesHidden = Settings.Default.IsPrivateGamesHidden;

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

        #region IsSortEnabled
        private bool _IsSortEnabled = Settings.Default.IsGamesSortEnabled;
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
                        CollectionViewSource.LiveSortingProperties.Clear();
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

        #region IsFoesGamesHidden
        private bool _IsFoesGamesHidden = Settings.Default.IsFoesGamesHidden;
        public bool IsFoesGamesHidden
        {
            get => _IsFoesGamesHidden;
            set
            {
                if (Set(ref _IsFoesGamesHidden, value))
                {
                    View.Refresh();
                }
            }
        }
        #endregion

        #region IsMapsBlacklistEnabled
        private bool _IsMapsBlacklistEnabled = Settings.Default.IsMapsBlacklistEnabled;
        public bool IsMapsBlacklistEnabled
        {
            get => _IsMapsBlacklistEnabled;
            set
            {
                if (Set(ref _IsMapsBlacklistEnabled, value))
                {
                    if (MapsBlackList.Count > 0)
                        View.Refresh();
                }
            }
        }
        #endregion

        #region MapsBlackList

        public ObservableCollection<string> MapsBlackList { get; set; } = new();

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
        public ICommand AddKeyWordCommand => _AddKeyWordCommand;
        private bool CanAddKeyWordCommand(object parameter) => !string.IsNullOrWhiteSpace(_InputKeyWord);
        public void OnAddKeyWordCommand(object parameter)
        {
            if (MapsBlackList.Contains(_InputKeyWord.Trim())) return;
            MapsBlackList.Add(_InputKeyWord.Trim());
            InputKeyWord = string.Empty;

            if (_IsMapsBlacklistEnabled)
                View.Refresh();
        }
        #endregion

        #region RemoveKeyWordCommand
        private ICommand _RemoveKeyWordCommand;
        public ICommand RemoveKeyWordCommand => _RemoveKeyWordCommand;
        public void OnRemoveKeyWordCommand(object parameter)
        {
            if (parameter == null) return;
            MapsBlackList.Remove(parameter.ToString());

            if (_IsMapsBlacklistEnabled)
                View.Refresh();
        }
        #endregion

        #endregion

        public static object NewItemPlaceholder => CollectionView.NewItemPlaceholder;
        public ObservableCollection<GameInfoMessage> LiveGames => GamesServices.LiveGames;

        private readonly IGamesServices GamesServices;
        private readonly object _lock = new();

        #endregion

        #region CTOR
        public GlobalView()
        {
            InitializeComponent();

            _AddKeyWordCommand = new LambdaCommand(OnAddKeyWordCommand, CanAddKeyWordCommand);
            _RemoveKeyWordCommand = new LambdaCommand(OnRemoveKeyWordCommand);
            _OpenGameCreationWindowCommand = new LambdaCommand(OnOpenGameCreationWindowCommand, CanOpenGameCreationWindowCommand);
            
            if (!App.Current.Resources.Contains("OnOpenGameCreationWindowCommand"))
                App.Current.Resources.MergedDictionaries[2].Add("OnOpenGameCreationWindowCommand", _OpenGameCreationWindowCommand);

            IdleGamesScrollViewer = Tools.FindChild<ScrollViewer>(IdleGames);

            GamesServices = App.Services.GetRequiredService<IGamesServices>();

            //var grouping = new PropertyGroupDescription(nameof(GameInfoMessage.featured_mod));
            CollectionViewSource.Filter += LobbiesViewSourceFilter;
            //CollectionViewSource.GroupDescriptions.Add(grouping);
            CollectionViewSource.IsLiveSortingRequested = true;

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

        #region OpenGameCreationWindowCommand
        private ICommand _OpenGameCreationWindowCommand;
        public ICommand OpenGameCreationWindowCommand => _OpenGameCreationWindowCommand;
        private bool CanOpenGameCreationWindowCommand(object parameter) => !_IsExtendedViewEnabled;
        private void OnOpenGameCreationWindowCommand(object parameter)
        {
            TestDialog.ShowAsync();
        }
        #endregion
    }
}
