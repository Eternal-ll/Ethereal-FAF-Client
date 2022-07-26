using beta.Infrastructure.Commands;
using beta.Infrastructure.Converters;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Enums;
using beta.Models.IRC;
using beta.Models.IRC.Base;
using beta.Models.Server;
using beta.Resources.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace beta.ViewModels
{
    public class ChatViewModel : Base.ViewModel
    {
        private readonly IIrcService IrcService;
        private readonly IPlayersService PlayersService;

        private readonly object _lock = new();

        public ObservableCollection<IrcChannelVM> Channels { get; } = new();

        private TestControl _TestInputControl;
        public TestControl TestInputControl
        {
            get => _TestInputControl;
            set
            {
                _TestInputControl = value;
                if (value is not null)
                {
                    value.LeaveRequired += (s, e) =>
                    {
                        if (SelectedChannel is null) return;
                        OnLeaveFromChannelCommand(SelectedChannel.Name);
                    };
                }
            }
        }

        #region SelectedChannel
        private IrcChannelVM _SelectedChannel;
        public IrcChannelVM SelectedChannel
        {
            get => _SelectedChannel;
            set
            {
                if (_SelectedChannel is not null)
                {
                    _SelectedChannel.IsSelected = false;

                    //App.Current.Dispatcher.Invoke(() =>
                    //{
                    //    SelectedChannelPlayersViewSource = null;
                    OnPropertyChanged(nameof(SelectedChannelPlayersView));
                    //    //BindingOperations.DisableCollectionSynchronization(_SelectedChannel.History);
                    //    //BindingOperations.DisableCollectionSynchronization(_SelectedChannel.Players);
                    //});
                }
                if (Set(ref _SelectedChannel, value))
                {
                    FilterText = string.Empty;
                    if (value is not null)
                    {
                        //if (value.Users is not null)
                        //    Task.Run(() =>
                        //    {
                        //        UpdateSelectedChannelUsers(value.Users.ToArray());
                        //    });
                        
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            SelectedChannelPlayersViewSource.Source = value.Players;
                            BindingOperations.EnableCollectionSynchronization(value.Players, _lock);
                            OnPropertyChanged(nameof(SelectedChannelPlayersView));
                        });
                        value.IsSelected = true;
                    }
                    else
                    {
                        SelectedChannelPlayers?.Clear();
                    }

                    if (TestInputControl is null) return;
                    TestInputControl.SelectedChannel = value;
                }
            }
        }
        #endregion
        private ObservableCollection<IPlayer> _SelectedChannelPlayers;
        public ObservableCollection<IPlayer> SelectedChannelPlayers
        {
            get => _SelectedChannelPlayers;
            set
            {
                if (Set(ref _SelectedChannelPlayers, value))
                {
                    BindingOperations.EnableCollectionSynchronization(SelectedChannelPlayers, _lock);
                    SelectedChannelPlayersViewSource.Source = value;
                    OnPropertyChanged(nameof(SelectedChannelPlayersView));
                }
            }
        }
        private CollectionViewSource SelectedChannelPlayersViewSource = new();
        public ICollectionView SelectedChannelPlayersView => SelectedChannelPlayersViewSource?.View;

        private ObservableCollection<IrcMessage> _SelectedChannelHistory;
        public ObservableCollection<IrcMessage> SelectedChannelHistory
        {
            get => _SelectedChannelHistory;
            set
            {
                if (value is null && _SelectedChannel is not null)
                {
                    BindingOperations.DisableCollectionSynchronization(value);
                }
                if (Set(ref _SelectedChannelHistory, value))
                {
                    if (value is not null)
                    {
                        BindingOperations.EnableCollectionSynchronization(value, _lock);
                    }
                }
            }
        }

        #region SelectedGrouping
        private PropertyGroupDescription _SelectedGrouping;
        public PropertyGroupDescription SelectedGrouping
        {
            get => _SelectedGrouping;
            set
            {
                if (Set(ref _SelectedGrouping, value))
                {
                    SelectedChannelPlayersViewSource.GroupDescriptions.Clear();
                    if (value is not null) SelectedChannelPlayersViewSource.GroupDescriptions.Add(value);
                }
            }
        } 
        #endregion
        public PropertyGroupDescription[] PropertyGroupDescriptions { get; set; } = new PropertyGroupDescription[2];
        private void InitializeGroupDescriptions()
        {
            var roleGroupDescription = new PropertyGroupDescription(null, new ChatUserGroupConverter());
            roleGroupDescription.GroupNames.Add("Me");
            roleGroupDescription.GroupNames.Add("Moderators");
            roleGroupDescription.GroupNames.Add("Friends");
            roleGroupDescription.GroupNames.Add("Clan");
            roleGroupDescription.GroupNames.Add("Players");
            roleGroupDescription.GroupNames.Add("IRC users");
            roleGroupDescription.GroupNames.Add("Foes");
            PropertyGroupDescriptions[0] = roleGroupDescription;
            PropertyGroupDescriptions[1] = new PropertyGroupDescription(null, new UserCountryGroupConverter());
            SelectedGrouping = roleGroupDescription;
        }

        public ChatViewModel()
        {
            var ircService = App.Services.GetService<IIrcService>();
            var playersService = App.Services.GetService<IPlayersService>();

            IrcService = ircService;
            PlayersService = playersService;

            BindingOperations.EnableCollectionSynchronization(Channels, _lock);
            InitializeGroupDescriptions();
            SelectedChannelPlayersViewSource.GroupDescriptions.Add(SelectedGrouping);
            SelectedChannelPlayersViewSource.SortDescriptions.Add(new(nameof(IPlayer.login), ListSortDirection.Ascending));
            SelectedChannelPlayersViewSource.Filter += PlayersFilter;

            playersService.PlayerUpdated += PlayersService_PlayerUpdated;

            #region IrcService event listeners
            ircService.StateChanged += OnStateChanged;
            ircService.NotificationMessageReceived += IrcService_NotificationMessageReceived;
            ircService.ChannelUsersReceived += OnChannelUsersReceived;
            ircService.ChannelTopicUpdated += OnChannelTopicUpdated;
            ircService.ChannelTopicChangedBy += OnChannelTopicChangedBy;
            ircService.UserJoined += OnChannelUserJoin;
            ircService.UserLeft += OnChannelUserLeft;
            ircService.UserDisconnected += IrcService_UserDisconnected;
            ircService.UserChangedName += OnUserChangedName;
            ircService.ChannedMessageReceived += OnChannelMessageReceived;
            #endregion

            //if (ircService.State == IrcState.Authorized)
            //    ircService.Channels.ForEach(channel => ircService.Join(channel));
            if (Properties.Settings.Default.ConnectIRC)
            {

            }
        }
        #region ChatVisibility
        private Visibility _ChatVisibility = Visibility.Collapsed;
        public Visibility ChatVisibility
        {
            get => _ChatVisibility;
            set => Set(ref _ChatVisibility, value);
        }
        #endregion

        #region PreviewVisibility
        private Visibility _PreviewVisibility = Visibility.Visible;
        public Visibility PreviewVisibility
        {
            get => _PreviewVisibility;
            set => Set(ref _PreviewVisibility, value);
        }
        #endregion

        #region FormVisibility
        private Visibility _FormVisibility = Visibility.Visible;
        public Visibility FormVisibility
        {
            get => _FormVisibility;
            set => Set(ref _FormVisibility, value);
        }
        #endregion

        #region LoadingVisibility
        private Visibility _LoadingVisibility;
        public Visibility LoadingVisibility
        {
            get => _LoadingVisibility;
            set => Set(ref _LoadingVisibility, value);
        }
        #endregion


        #region FilterText
        private string _FilterText = string.Empty;
        public string FilterText
        {
            get => _FilterText;
            set
            {
                if (Set(ref _FilterText, value))
                {
                    SelectedChannelPlayersView.Refresh();
                }
            }
        }
        #endregion



        #region NewChannelText
        private string _NewChannelText = string.Empty;
        public string NewChannelText
        {
            get => _NewChannelText;
            set => Set(ref _NewChannelText, value);
        }
        #endregion

        #region Commands

        #region LeaveFromChannelCommand
        private ICommand _LeaveFromChannelCommand;
        public ICommand LeaveFromChannelCommand => _LeaveFromChannelCommand ??= new LambdaCommand(OnLeaveFromChannelCommand);
        private void OnLeaveFromChannelCommand(object parameter)
        {
            var channel = parameter.ToString();
            if (string.IsNullOrWhiteSpace(channel)) return;
            IrcService.Leave(channel);
            for (int i = 0; i < Channels.Count; i++)
            {
                if (Channels[i].Name == channel)
                {
                    if (i > 0 && Channels[i].IsSelected) SelectedChannel = Channels[i - 1];
                    Channels.RemoveAt(i);
                }
            }
        }

        #endregion

        #region UpdateUsersCommand
        private ICommand _RefreshUserListCommand;
        public ICommand RefreshUserListCommand => _RefreshUserListCommand ??= new LambdaCommand(OnRefreshUserListCommand);
        private void OnRefreshUserListCommand(object parameter)
        {
            if (SelectedChannel is null) return;
            Task.Run(() => OnChannelUsersReceived(this, new(SelectedChannel.Name, SelectedChannel.Users.ToArray())));
        }
        #endregion

        #region JoinChannelCommand
        private ICommand _JoinChannelCommand;
        public ICommand JoinChannelCommand => _JoinChannelCommand ??= new LambdaCommand(OnJoinChannelCommand, CanJoinChannelCommand);
        private bool CanJoinChannelCommand(object parameter) => parameter is not null && parameter is string channel && !string.IsNullOrWhiteSpace(channel);
        private void OnJoinChannelCommand(object parameter)
        {
            if (parameter is not string channel) return;
            if (string.IsNullOrWhiteSpace(channel)) return;
            if (!channel.StartsWith('#')) channel = "#" + channel;
            var channels = Channels;
            for (int i = 0; i < channels.Count; i++)
            {
                if (channels[i].Name == channel)
                {
                    SelectedChannel = channels[i];
                    NewChannelText = string.Empty;
                    return;
                }
            }
            IrcService.Join(channel);
            NewChannelText = string.Empty;
        }
        #endregion

        #endregion

        private void PlayersService_PlayerUpdated(object sender, PlayerInfoMessage e)
        {
            var players = SelectedChannelPlayers;

            if (players is null) return;

            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].id == e.id)
                {
                    players[i] = e;
                }
            }
        }

        private void IrcService_NotificationMessageReceived(object sender, IrcNotificationMessage e) => SelectedChannel?.History.Add(e);

        private IPlayer GetChatPlayer(string login)
        {
            bool isChatMod = login.StartsWith('@');

            if (isChatMod) login = login[1..];

            string ircName = login;

            login = login.Replace('`', ' ');

            if (PlayersService.TryGetPlayer(login, out var player))
            {
                player.IrcName = ircName;
                player.IsChatModerator = isChatMod;
                return player;
            }
            return new UnknownPlayer()
            {
                IsChatModerator = isChatMod,
                login = ircName
            };
        }

        private void OnChannelMessageReceived(object sender, IrcChannelMessage e)
        {
            if (TryGetChannel(e.Channel, out var channel))
                channel.AddMessage(e);
        }

        private void OnUserChangedName(object sender, IrcUserChangedName e)
        {
            var channels = Channels;
            for (int i = 0; i < channels.Count; i++)
            {
                var channel = channels[i];
                if (channel.UpdateUser(e.From, e.To))
                {
                }
                channel.UpdatePlayer(GetChatPlayer(e.To), e.From);
            }
        }

        private void OnChannelUserLeft(object sender, IrcUserLeft e)
        {
            if (TryGetChannel(e.Channel, out var channel))
            {
                if (channel.RemoveUser(e.User))
                {

                }
            }
        }
        private void IrcService_UserDisconnected(object sender, string e)
        {
            var channels = Channels;
            foreach (var channel in channels)
            {
                if (channel.RemoveUser(e))
                {

                }
                else
                {

                }
            }
        }

        bool TryGetChannel(string name, out IrcChannelVM channel)
        {
            var channels = Channels;
            for (int i = 0; i < channels.Count; i++)
            {
                channel = channels[i];
                if (channel.Name == name)
                {
                    return true;
                }
            }
            channel = null;
            return false;
        }

        private void OnChannelUserJoin(object sender, IrcUserJoin e)
        {
            if (TryGetChannel(e.Channel, out var channel))
            {
                if (channel.AddUser(e.User) && channel.IsSelected)
                {
                    SelectedChannelPlayers?.Add(GetChatPlayer(e.User));
                }
            }
        }
        private void OnChannelTopicChangedBy(object sender, IrcChannelTopicChangedBy e)
        {
            if (TryGetChannel(e.Channel, out var channel))
            {
                channel.TopicChangedBy = e;
                channel.AddMessage(new IrcNotificationMessage(e.ToString()));
            }
        }

        private IrcChannelTopicUpdated LastTopicUpdated;
        private void OnChannelTopicUpdated(object sender, IrcChannelTopicUpdated e)
        {
            if (TryGetChannel(e.Channel, out var channel))
            {
                channel.Topic = e.Topic;
                channel.AddMessage(new IrcNotificationMessage(e.ToString()));
            }
            else
            {
                LastTopicUpdated = e;
            }
        }
        private void OnChannelUsersReceived(object sender, IrcChannelUsers e)
        {
            IrcChannelVM channel;
            var found = TryGetChannel(e.Channel, out channel);
            if (!found)
            {
                channel = new(e.Channel);
                App.Current.Dispatcher.Invoke(() =>
                {
                    BindingOperations.EnableCollectionSynchronization(channel.History, _lock);
                });
            }
            if (LastTopicUpdated is not null)
            {
                channel.Topic = LastTopicUpdated.Topic;
                channel.AddMessage(new IrcNotificationMessage(LastTopicUpdated.ToString()));
                LastTopicUpdated = null;
            }
            channel.Users = new(e.Users);
            ObservableCollection<IPlayer> players = new();
            for (int i = 0; i < e.Users.Length; i++)
            {
                players.Add(GetChatPlayer(e.Users[i]));
            }
            if (channel.IsSelected)
            {
                SelectedChannel = null;
                channel.Players = players;
            }
            else
            {
                channel.Players = players;
            }
            if (!found)
            {
                Channels.Add(channel);
            }
            SelectedChannel = channel;
        }

        private void OnStateChanged(object sender, IrcState e)
        {
            if (e == IrcState.Authorized)
            {
                for (int i = 0; i < Channels.Count; i++)
                {
                    IrcService.Join(Channels[i].Name);
                }
            }
            else if (e != IrcState.Connected)
            {
                for (int i = 0; i < Channels.Count; i++)
                {
                    Channels[i].Users.Clear();
                }
            }
            else if (e == IrcState.Disconnected)
            {
                SelectedChannel = null;
                Channels.Clear();
            }
        }

        private void PlayersFilter(object sender, FilterEventArgs e)
        {
            var player = (IPlayer)e.Item;

            if (FilterText.Length == 0) return;

            e.Accepted = player.login.StartsWith(FilterText, System.StringComparison.OrdinalIgnoreCase);

            if (player.RelationShip.ToString().Equals(FilterText, System.StringComparison.OrdinalIgnoreCase))
            {
                e.Accepted = true;
                return;
            }
        }
    }
}
