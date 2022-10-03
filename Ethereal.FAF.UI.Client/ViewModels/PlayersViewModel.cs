using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using Meziantou.Framework.WPF.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class Welcome : WelcomeData
    {
        public new Player me { get; set; }
    }
    public class Player : PlayerInfoMessage
    {
        private string _FlagUri;
        public string FlagUri => _FlagUri ??= "/Resources/Images/Flags/" + Country + ".png";

        [JsonPropertyName("players")]
        public new Player[] Players { get; set; }


        public int Global => Ratings.Global is null ? 0 : Ratings.Global.DisplayedRating;
        public int Ladder1v1 => Ratings.Ladder1V1 is null ? 0 : Ratings.Ladder1V1.DisplayedRating;
        public int Tmm2v2 => Ratings.Tmm2V2 is null ? 0 : Ratings.Tmm2V2.DisplayedRating;
        public int Tmm4v4 => Ratings.Tmm4V4FullShare is null ? 0 : Ratings.Tmm4V4FullShare.DisplayedRating;

        public RatingType DisplayRatingType { get; set; }
        public int UniversalRatingDisplay => DisplayRatingType switch
        {
            RatingType.global => Global,
            RatingType.ladder_1v1 => Ladder1v1,
            RatingType.tmm_4v4_full_share => Tmm4v4,
            RatingType.tmm_4v4_share_until_death => Tmm4v4,
            RatingType.tmm_2v2 => Tmm2v2,
        };
    }
    public class PlayersSort
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
    public class PlayersGroup
    {
        public string Name { get; set; }
        public PropertyGroupDescription PropertyGroupDescription { get; set; }
    }
    public class RatingsRangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int rating)
            {
                var ceilings = new[] { 0, 300, 600, 900, 1200, 1500, 1700, 1900, 2100, 2400, 2800 };
                var group = ceilings.First(ceiling => ceiling >= rating);
                ceilings = new[] { -1000, -500, -200, 0, 300, 600, 900, 1200, 1500, 1700, 1900, 2100, 2400, 2800 };
                group = 0;
                foreach (var ceil in ceilings)
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


        private readonly LobbyClient Lobby;

        public PlayersViewModel(LobbyClient lobby)
        {
            Lobby = lobby;
            lobby.PlayersReceived += Lobby_PlayersReceived;
            lobby.PlayerReceived += Lobby_PlayerReceived;
            lobby.WelcomeDataReceived += Lobby_WelcomeDataReceived;
            GroupBy = PlayersGroupSource[0];
        }

        public Player Self;

        private void Lobby_WelcomeDataReceived(object sender, Welcome e)
        {
            Self = e.me;
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

        public PlayersSort[] PlayersSortSource { get; set; } = new PlayersSort[]
        {
            new PlayersSort("Sort by", null, ListSortDirection.Ascending),
            new PlayersSort("Sort by Id", nameof(Player.Id), ListSortDirection.Ascending),
            new PlayersSort("Sort by Login", nameof(Player.Login), ListSortDirection.Ascending),
            new PlayersSort("Sort by Clan", nameof(Player.Clan), ListSortDirection.Ascending),
            new PlayersSort("Sort by Country", nameof(Player.Country), ListSortDirection.Ascending),
            new PlayersSort("Sort by Rating Global", nameof(Player.Global), ListSortDirection.Ascending, true),
            new PlayersSort("Sort by Rating 1 vs 1", nameof(Player.Ladder1v1), ListSortDirection.Ascending, true),
            new PlayersSort("Sort by Rating 2 vs 2", nameof(Player.Tmm2v2), ListSortDirection.Ascending, true),
            new PlayersSort("Sort by Rating 4 vs 4", nameof(Player.Tmm4v4), ListSortDirection.Ascending, true),
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
                        PlayersSource?.SortDescriptions.Add(new SortDescription(value.Property, value.ListSortDirection));
                        if (GroupIndex == 2)
                        {
                            GroupBy.PropertyGroupDescription.PropertyName = value.IsRating ? value.Property : nameof(Player.Ladder1v1);
                            RatingType rating = value.Property switch
                            {
                                nameof(Player.Global) => RatingType.global,
                                nameof(Player.Ladder1v1) => RatingType.ladder_1v1,
                                nameof(Player.Tmm2v2) => RatingType.tmm_2v2,
                                nameof(Player.Tmm4v4) => RatingType.tmm_4v4_full_share,
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

        public PlayersGroup[] PlayersGroupSource { get; set; } = new PlayersGroup[]
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
                PropertyGroupDescription = new PropertyGroupDescription(nameof(Player.Global), new RatingsRangeConverter())
            }
        };

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
                    PlayersSource = new()
                    {
                        Source = value.AsObservable
                    };
                    PlayersSource.Filter += PlayersSource_Filter;
                    OnPropertyChanged(nameof(PlayersView));
                }
            }
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
            if (!found) players.Add(e);
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

        private void Lobby_PlayersReceived(object sender, Player[] e) => App.Current.Dispatcher.Invoke(() =>
        {
            PrepareRatings(e);
            var obs = new ConcurrentObservableCollection<Player>();
            obs.AddRange(e);
            Players = obs;
        }, System.Windows.Threading.DispatcherPriority.Background);
    }
}
    