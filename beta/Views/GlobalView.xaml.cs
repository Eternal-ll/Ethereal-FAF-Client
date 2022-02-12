using beta.Infrastructure.Services.Interfaces;
using beta.Infrastructure.Utils;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
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
                _IsPrivateGamesHidden = value;

                View.Refresh();
                OnPropertyChanged(nameof(IsPrivateGamesHidden));
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

            //var mapName = lobby.MapName;
            //if (mapName.Contains("gap") || mapName.Contains("crater") || mapName.Contains("astro") ||
            //    mapName.Contains("pass"))
            //    return;

            if (lobby.game_type != "custom" || lobby.featured_mod != "faf")
                return;

            //if (lobby.num_players == 0)
            //    return;


            if (_IsPrivateGamesHidden)
            {
                e.Accepted = !lobby.password_protected;
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
