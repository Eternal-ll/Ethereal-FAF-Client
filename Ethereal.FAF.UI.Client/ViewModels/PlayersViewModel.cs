using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.IRC;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.Views;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using Meziantou.Framework.WPF.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui.Mvvm.Contracts;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class PlayersSort
    {
        public PlayersSort()
        {

        }
        public PlayersSort(string name, string property, ListSortDirection listSortDirection, bool isRating = false)
        {
            Name = name;
            Property = property;
            ListSortDirection = listSortDirection;
            IsRating = isRating;
        }

        public string Name { get; set; }
        public string Property { get; set; }
        public bool IsRating { get; set; }
        public ListSortDirection ListSortDirection { get; set; }
    }
    public sealed class PlayersGroup
    {
        public string Name { get; set; }
        public PropertyGroupDescription PropertyGroupDescription { get; set; }
    }
    public sealed class RatingsRangeConverter : IValueConverter
    {
        private readonly IConfiguration Configuration;

        public RatingsRangeConverter(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int rating)
            {
                var ranges = Configuration.GetPlayersRatingRanges();
                var group = 0;
                foreach (var ceil in ranges)
                {
                    if (ceil <= rating) group = ceil;
                }
                return group;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PlayersViewModel : Base.ViewModel
    {
        public event EventHandler<Game> NewGameReceived;
        public event EventHandler<Game> GameUpdated;
        //public event EventHandler<Game> GameRemoved;

        public event EventHandler<Game> GameLaunched;
        public event EventHandler<Game> GameEnd;
        public event EventHandler<Game> GameClosed;

        public event EventHandler<string[]> PlayersLeftFromGame;
        public event EventHandler<KeyValuePair<Game, string[]>> PlayersJoinedToGame;

        public event EventHandler<KeyValuePair<Game, Player[]>> PlayersJoinedGame;
        public event EventHandler<KeyValuePair<Game, Player[]>> PlayersLeftGame;
        public event EventHandler<KeyValuePair<Game, Player[]>> PlayersFinishedGame;

        private readonly IServiceProvider ServiceProvider;
        private readonly IConfiguration Configuration;
        private readonly LobbyClient Lobby;
        private readonly IrcClient IrcClient;

        public PlayersViewModel(LobbyClient lobby, IServiceProvider serviceProvider, IrcClient ircClient, IConfiguration configuration)
        {
            OpenPrivateCommand = new LambdaCommand(OnOpenPrivateCommand);

            lobby.PlayersReceived += Lobby_PlayersReceived;
            lobby.PlayerReceived += Lobby_PlayerReceived;
            lobby.WelcomeDataReceived += Lobby_WelcomeDataReceived;

            ircClient.UserDisconnected += IrcClient_UserDisconnected;


            Lobby = lobby;
            ServiceProvider = serviceProvider;
            IrcClient = ircClient;
            Configuration = configuration;

            PlayersGroupSource = new PlayersGroup[]
            {
                new PlayersGroup()
                {
                    Name = "Group by"
                },
                new PlayersGroup()
                {
                    Name = "Group by countries",
                    PropertyGroupDescription = new PropertyGroupDescription(nameof(Player.Country))
                },
                new PlayersGroup()
                {
                    Name = "Group by ratings ranges",
                    PropertyGroupDescription = new PropertyGroupDescription(nameof(Player.Global), new RatingsRangeConverter(configuration))
                }
            };
            GroupBy = PlayersGroupSource[0];
        }

        public Player Self { get; set; }

        private void Lobby_WelcomeDataReceived(object sender, Welcome e)
        {
            Self = e.me;
        }

        private void IrcClient_UserDisconnected(object sender, (string user, string id) e)
        {
            if (!long.TryParse(e.id, out var id)) return;
            Players.Remove(GetPlayer(id));
        }

        #region SelectedPlayer
        private Player _SelectedPlayer;
        public Player SelectedPlayer
        {
            get => _SelectedPlayer;
            set
            {
                if (Set(ref _SelectedPlayer, value))
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
                    PlayersView.Refresh();
                }
            }
        }
        #endregion

        public Visibility ListSortDirectionVisibility => SelectedPlayersSort?.Property is not null ? Visibility.Visible : Visibility.Collapsed;
        public ListSortDirection[] ListSortDirectionSource { get; set; } = new ListSortDirection[]
        {
            ListSortDirection.Descending,
            ListSortDirection.Ascending
        };
        #region ListSortDirection
        private ListSortDirection _ListSortDirection;
        public ListSortDirection ListSortDirection
        {
            get => _ListSortDirection;
            set
            {
                if (Set(ref _ListSortDirection, value))
                {
                    var oldsort = _SelectedPlayersSort;
                    oldsort.ListSortDirection = value;
                    _SelectedPlayersSort = null;
                    SelectedPlayersSort = oldsort;
                }
            }
        }

        #endregion

        public PlayersSort[] PlayersSortSource { get; set; } = new PlayersSort[]
        {
            new PlayersSort("Sort by", null, ListSortDirection.Descending),
            new PlayersSort("Sort by Id", nameof(Player.Id), ListSortDirection.Descending),
            new PlayersSort("Sort by Login", nameof(Player.Login), ListSortDirection.Descending),
            new PlayersSort("Sort by Clan", nameof(Player.Clan), ListSortDirection.Descending),
            new PlayersSort("Sort by Country", nameof(Player.Country), ListSortDirection.Descending),
            new PlayersSort("Sort by Rating Global", nameof(Player.Global), ListSortDirection.Descending, true),
            new PlayersSort("Sort by Rating 1 vs 1", nameof(Player.Ladder1v1), ListSortDirection.Descending, true),
            new PlayersSort("Sort by Rating 2 vs 2", nameof(Player.Tmm2v2), ListSortDirection.Descending, true),
            new PlayersSort("Sort by Rating 4 vs 4", nameof(Player.Tmm4v4), ListSortDirection.Descending, true),
        };

        #region SelectedPlayersSort
        private PlayersSort _SelectedPlayersSort;
        public PlayersSort SelectedPlayersSort
        {
            get => _SelectedPlayersSort;
            set
            {
                if (Set(ref _SelectedPlayersSort, value))
                {
                    PlayersSource?.SortDescriptions.Clear();
                    if (value?.Property is not null)
                    {
                        _ListSortDirection = value.ListSortDirection;
                        OnPropertyChanged(nameof(ListSortDirection));
                        PlayersSource?.SortDescriptions.Add(new SortDescription(value.Property, value.ListSortDirection));
                        if (GroupIndex == 2)
                        {
                            GroupBy.PropertyGroupDescription.PropertyName = value.IsRating ? value.Property : nameof(Player.Ladder1v1);
                            RatingType rating = GroupBy.PropertyGroupDescription.PropertyName switch
                            {
                                nameof(Player.Global) => RatingType.global,
                                nameof(Player.Ladder1v1) => RatingType.ladder_1v1,
                                nameof(Player.Tmm2v2) => RatingType.tmm_2v2,
                                nameof(Player.Tmm4v4) => RatingType.tmm_4v4_full_share,
                                _=> RatingType.ladder_1v1
                            };
                            Task.Run(() =>
                            {
                                foreach (var player in Players)
                                {
                                    player.DisplayRatingType = rating;
                                    OnPropertyChanged(nameof(player.UniversalRatingDisplay));
                                }
                            });
                        }
                    }
                    OnPropertyChanged(nameof(ListSortDirectionVisibility));
                    PlayersView?.Refresh();
                }
            }
        }
        #endregion

        public ReadOnlyObservableCollection<object> Groups => PlayersView.Groups;

        #region SelectedGroup
        private CollectionViewGroup _SelectedGroup;
        public CollectionViewGroup SelectedGroup
        {
            get => _SelectedGroup;
            set => Set(ref _SelectedGroup, value);
        }
        #endregion

        #region GroupIndex
        private int _GroupIndex = 0;
        public int GroupIndex
        {
            get => _GroupIndex;
            set => Set(ref _GroupIndex, value);
        }
        #endregion
        #region GroupBy
        private PlayersGroup _GroupBy;
        public PlayersGroup GroupBy
        {
            get => _GroupBy;
            set
            {
                if (Set(ref _GroupBy, value))
                {
                    PlayersSource?.GroupDescriptions.Clear();
                    if (value?.PropertyGroupDescription is not null)
                    {
                        PlayersSource?.GroupDescriptions.Add(value.PropertyGroupDescription);
                    }
                    OnPropertyChanged(nameof(Groups));
                    PlayersView?.Refresh();
                }
            }
        }
        #endregion

        public PlayersGroup[] PlayersGroupSource { get; set; }

        public System.Collections.Concurrent.ConcurrentDictionary<int, PlayerInfoMessage> PlayersDic { get; } = new();
        public PlayerInfoMessage[] PlayersArray => PlayersDic.Select(x => x.Value).ToArray();

        #region Players
        public CollectionViewSource PlayersSource { get; private set; }
        public ICollectionView PlayersView => PlayersSource?.View;
        private ConcurrentObservableCollection<Player> _Players;
        public ConcurrentObservableCollection<Player> Players
        {
            get => _Players;
            set
            {
                if (Set(ref _Players, value))
                {
                    App.Current.Dispatcher.Invoke(UpdatePlayersSource);
                }
            }
        }
        private void UpdatePlayersSource()
        {
            PlayersSource = new()
            {
                Source = Players.AsObservable
            };
            PlayersSource.Filter += PlayersSource_Filter;
            OnPropertyChanged(nameof(PlayersView));
        }

        private void PlayersSource_Filter(object sender, FilterEventArgs e)
        {
            var player = (Player)e.Item;
            e.Accepted = false;
            if (!string.IsNullOrWhiteSpace(FilterText))
            {
                e.Accepted = player.Login.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
                return;
            }
            e.Accepted = true;
        }
        #endregion

        public Player GetPlayer(long id) => Players.FirstOrDefault(p => p.Id == id);
        public Player GetPlayer(string login) => Players.FirstOrDefault(p => p.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
        public bool TryGetPlayer(long id, out Player player)
        {
            player = GetPlayer(id);
            return player is not null;
        }
        public bool TryGetPlayer(string login, out Player player)
        {
            player = GetPlayer(login);
            return player is not null;
        }

        private void Lobby_PlayerReceived(object sender, Player e)
        {
            if (e.Id == Self.Id)
            {
                Self = e;
            }
            
            PrepareRatings(e);

            var players = Players;
            var found = false;
            for (int i = 0; i < players.Count; i++)
            {
                var old = players[i];
                if (old.Id == e.Id)
                {
                    found = true;
                    players[i] = e;
                    break;
                }
            }
            if (!found)
            {
                players.Add(e);
            }
        }

        private void PrepareRatings(params Player[] players)
        {
            foreach (var player in players)
            {
                if (player.Ratings.Global is not null) player.Ratings.Global.name = "Global";
                if (player.Ratings.Ladder1V1 is not null) player.Ratings.Ladder1V1.name = "1 vs 1";
                if (player.Ratings.Tmm2V2 is not null) player.Ratings.Tmm2V2.name = "2 vs 2";
                if (player.Ratings.Tmm4V4FullShare is not null) player.Ratings.Tmm4V4FullShare.name = "4 vs 4";
            }
        }

        private void Lobby_PlayersReceived(object sender, Player[] e)
        {
            if (Players is not null)
            {
                foreach (var player in e)
                {
                    Lobby_PlayerReceived(sender, player);
                }
                return;
            }

            PrepareRatings(e);
            var obs = new ConcurrentObservableCollection<Player>();
            obs.AddRange(e);
            Players = obs;
        }

        public ICommand OpenPrivateCommand { get; }
        private void OnOpenPrivateCommand(object arg)
        {
            if (arg is not string user) return;
            var navigation = ServiceProvider.GetService<INavigationService>();
            var chat = ServiceProvider.GetService<ChatViewModel>();
            chat.OpenPrivateCommand.Execute(user);
            navigation.Navigate(typeof(ChatView));
        }
    }
}
    