﻿using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using FAF.Domain.LobbyServer.Enums;
using Meziantou.Framework.WPF.Collections;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

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
        private readonly IServiceProvider ServiceProvider;
        private readonly IFafPlayersService _fafPlayersService;
        private readonly IFafPlayersEventsService _fafPlayersEventsService;

        public PlayersViewModel(IServiceProvider serviceProvider, IConfiguration configuration, IFafPlayersService fafPlayersService, IFafPlayersEventsService fafPlayersEventsService)
        {
            Players = new(Application.Current.Dispatcher);
            OpenPrivateCommand = new LambdaCommand(OnOpenPrivateCommand);
            //var lobby = server.GetLobbyClient();
            //var ircClient = server.GetIrcClient();
            //lobby.StateChanged += (s, state) =>
            //{
            //    if (state is not LobbyState.Disconnected) return;
            //    lobby.WelcomeDataReceived -= server.WelcomeDataReceived;
            //    lobby.PlayerReceived -= server.PlayerReceived;
            //    lobby.PlayersReceived -= server.PlayersReceived;
            //    ircClient.UserDisconnected -= server.IrcUserDisconnected;
            //};
            //ircClient.UserDisconnected += (s, data) =>
            //{
            //    if (!long.TryParse(data.id, out var id)) return;
            //    var player = Players.FirstOrDefault(p => p.Id == id);
            //    Players.Remove(player);
            //};
            //lobby.WelcomeDataReceived += (s, self) => Self = self.me;
            //lobby.PlayerReceived += Lobby_PlayerReceived;
            //lobby.PlayersReceived += Lobby_PlayersReceived;
            ServiceProvider = serviceProvider;

            PlayersGroupSource = new PlayersGroup[]
            {
                new PlayersGroup()
                {
                    Name = "None"
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
                },
                new PlayersGroup()
                {
                    Name = "Group by clan",
                    PropertyGroupDescription = new PropertyGroupDescription(nameof(Player.Clan))
                }
            };
            GroupBy = PlayersGroupSource[0];
            _fafPlayersService = fafPlayersService;
            _fafPlayersEventsService = fafPlayersEventsService;
        }

        public override void OnLoaded()
        {

            _fafPlayersEventsService.PlayersAdded += _fafPlayersService_PlayersAdded;
            _fafPlayersEventsService.PlayersRemoved += _fafPlayersService_PlayersRemoved;
            Players.AddRange(_fafPlayersService.GetPlayers());
            base.OnLoaded();
        }

        public override void OnUnloaded()
        {

            _fafPlayersEventsService.PlayersAdded -= _fafPlayersService_PlayersAdded;
            _fafPlayersEventsService.PlayersRemoved -= _fafPlayersService_PlayersRemoved;
            Players.Clear();
            base.OnUnloaded();
        }

        private void _fafPlayersService_PlayersRemoved(object sender, Player[] e)
        {
            foreach (var player in e)
            {
                Players.Remove(player);
            }
        }
        private void _fafPlayersService_PlayersAdded(object sender, Player[] e) => Players.AddRange(e);

        public Player Self { get; set; }

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
                if (Set(ref _FilterText, value) &&
                    (string.IsNullOrEmpty(value) || value?.Length > 3))
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
            new PlayersSort("None", null, ListSortDirection.Descending),
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
            if (_Players == null) return;
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

        public ICommand OpenPrivateCommand { get; }
        private void OnOpenPrivateCommand(object arg)
        {
            //if (arg is not string user) return;
            //var navigation = ServiceProvider.GetService<INavigationService>();
            //var chat = ServiceProvider.GetService<ChatViewModel>();
            //chat.OpenPrivateCommand.Execute(user);
            //navigation.Navigate(typeof(ChatView));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PlayersSource.Filter -= PlayersSource_Filter;
                PlayersSource = null;
                Players.Clear();
                Players = null;

                _fafPlayersEventsService.PlayersAdded -= _fafPlayersService_PlayersAdded;
                _fafPlayersEventsService.PlayersRemoved -= _fafPlayersService_PlayersRemoved;

                PlayersGroupSource = null;
                SelectedGroup = null;
                SelectedPlayer = null;
                SelectedPlayersSort = null;
            }
            base.Dispose(disposing);
        }
    }
}