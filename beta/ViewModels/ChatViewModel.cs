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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
                }
                if (Set(ref _SelectedChannel, value))
                {
                    FilterText = string.Empty;
                    if (value is not null)
                    {
                        UpdateSelectedChannelUsers();
                        UpdateSelectedChannelHistory();
                        value.IsSelected = true;
                    }
                    else
                    {
                        SelectedChannelPlayers.Clear();
                        SelectedChannelHistory.Clear();
                    }
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
        private readonly CollectionViewSource SelectedChannelPlayersViewSource = new();
        public ICollectionView SelectedChannelPlayersView => SelectedChannelPlayersViewSource.View;

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
        public ChatViewModel()
        {
            var ircService = App.Services.GetService<IIrcService>();
            var playersService = App.Services.GetService<IPlayersService>();

            IrcService = ircService;
            PlayersService = playersService;

            BindingOperations.EnableCollectionSynchronization(Channels, _lock);

            PropertyGroupDescription groupDescription = new(null, new ChatUserGroupConverter());
            groupDescription.GroupNames.Add("Me");
            groupDescription.GroupNames.Add("Moderators");
            groupDescription.GroupNames.Add("Friends");
            groupDescription.GroupNames.Add("Clan");
            groupDescription.GroupNames.Add("Players");
            groupDescription.GroupNames.Add("IRC users");
            groupDescription.GroupNames.Add("Foes");
            SelectedChannelPlayersViewSource.GroupDescriptions.Add(groupDescription);
            SelectedChannelPlayersViewSource.Filter += PlayersFilter;

            playersService.PlayerUpdated += PlayersService_PlayerUpdated;

            #region IrcService event listeners
            ircService.StateChanged += OnStateChanged;
            IrcService.NotificationMessageReceived += IrcService_NotificationMessageReceived;
            ircService.ChannelUsersReceived += OnChannelUsersReceived;
            ircService.ChannelTopicUpdated += OnChannelTopicUpdated;
            ircService.ChannelTopicChangedBy += OnChannelTopicChangedBy;
            ircService.UserJoined += OnChannelUserJoin;
            ircService.UserLeft += OnChannelUserLeft;
            ircService.UserChangedName += OnUserChangedName;
            ircService.ChannedMessageReceived += OnChannelMessageReceived;
            #endregion

            if (ircService.State == IrcState.Authorized)
                ircService.Channels.ForEach(channel => ircService.GetChannelUsers(channel));
        }


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

            if (SelectedChannel?.Name == channel)
            {
                Channels.Remove(SelectedChannel);
                SelectedChannel = null;
            }

            IrcService.Leave(channel);
            //var channels = Channels;

            //for (int i = 0; i < channels.Count; i++)
            //{
            //    if (channels[i].Name == channel)
            //    {
            //        Channels.Remove(channels[i]);
            //        if (SelectedChannel is not null && SelectedChannel.Name == channel)
            //        {
            //            SelectedChannel = null;
            //            if (channels.Count > 0)
            //            {
            //                SelectedChannel = channels[0];
            //            }
            //        }
            //    }
            //}
        }

        #endregion

        #region UpdateUsersCommand
        private ICommand _RefreshUserListCommand;
        public ICommand RefreshUserListCommand => _RefreshUserListCommand ??= new LambdaCommand(OnRefreshUserListCommand);
        private void OnRefreshUserListCommand(object parameter)
        {
            if (SelectedChannel is null) return;
            //SelectedChannel.Users.Clear();
            SelectedChannelPlayers.Clear();
            UpdateSelectedChannelUsers();
            //IrcService.GetChannelUsers(SelectedChannel.Name);
        }
        #endregion

        #region JoinChannelCommand
        private ICommand _JoinChannelCommand;
        public ICommand JoinChannelCommand => _JoinChannelCommand ??= new LambdaCommand(OnJoinChannelCommand, CanJoinChannelCommand);
        private bool CanJoinChannelCommand(object parameter) => !string.IsNullOrWhiteSpace(NewChannelText);
        private void OnJoinChannelCommand(object parameter)
        {
            var channel = NewChannelText;
            if (string.IsNullOrWhiteSpace(channel)) return;
            if (!channel.StartsWith('#')) channel = "#" + channel;
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

        private IrcChannelVM GetChannel(string name)
        {
            var channels = Channels;
            for (int i = 0; i < channels.Count; i++)
            {
                var fChannel = channels[i];
                if (fChannel.Name == name)
                {
                    return fChannel;
                }
            }
            IrcChannelVM channel = new(name);
            Channels.Add(channel);
            SelectedChannel = channel;
            BindingOperations.EnableCollectionSynchronization(channel.History, _lock);
            return channel;
        }

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
                login = login
            };
        }
        private void UpdateSelectedChannelHistory()
        {
            var history = new ObservableCollection<IrcMessage>();
            var msgs = SelectedChannel.History.ToArray();

            for (int i = 0; i < msgs.Length; i++)
            {
                history.Add(msgs[i]);
            }
            SelectedChannelPlayersViewSource.Dispatcher
                .Invoke(() => SelectedChannelHistory = history);
        }
        private void UpdateSelectedChannelUsers()
        {
            var players = new ObservableCollection<IPlayer>();
            var users = SelectedChannel.Users.ToArray();

            for (int i = 0; i < users.Length; i++)
            {
                players.Add(GetChatPlayer(users[i]));
            }
            SelectedChannelPlayersViewSource.Dispatcher
                .Invoke(() => SelectedChannelPlayers = players);

            // TODO
            SelectedChannelPlayersViewSource.Dispatcher.Invoke(() => TestInputControl.UpdateUsers(users));
        }

        private void OnChannelMessageReceived(object sender, IrcChannelMessage e)
        {
            var channel = GetChannel(e.Channel);
            var msg = channel.AddMessage(e);
            if (SelectedChannel?.Name == channel.Name)
            {
                SelectedChannelHistory.Add(msg);
            }
        }

        private bool GetIndexOfPlayer(string login, out int index)
        {
            var players = SelectedChannelPlayers;
            for (int j = 0; j < players.Count; j++)
            {
                if (players[j].login == login)
                {
                    index = j;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        private void OnUserChangedName(object sender, IrcUserChangedName e)
        {
            var channels = Channels;
            for (int i = 0; i < channels.Count; i++)
            {
                var channel = channels[i];
                if (channel.UpdateUser(e.From, e.To) && channel.IsSelected && GetIndexOfPlayer(e.From, out var index))
                {
                    SelectedChannelPlayers[index] = GetChatPlayer(e.To);
                }
            }
        }

        private void OnChannelUserLeft(object sender, IrcUserLeft e)
        {
            var channels = Channels;
            foreach (var channel in channels)
            {
                if (channel.Name == e.Channel)
                {
                    if (channel.RemoveUser(e.User) && channel.IsSelected && GetIndexOfPlayer(e.User, out var index))
                    {
                        SelectedChannelPlayers.RemoveAt(index);
                    }
                }
            }
        }

        private string SelfLogin = Properties.Settings.Default.PlayerNick;

        private void OnChannelUserJoin(object sender, IrcUserJoin e)
        {
            if (SelfLogin == e.User) return;
            var channel = GetChannel(e.Channel);

            if (channel.AddUser(e.User) && channel.IsSelected)
            {
                SelectedChannelPlayers.Add(GetChatPlayer(e.User));
            }
        }

        private void OnChannelTopicChangedBy(object sender, IrcChannelTopicChangedBy e) =>
            GetChannel(e.Channel).TopicChangedBy = e;

        private void OnChannelTopicUpdated(object sender, IrcChannelTopicUpdated e)
        {
            var channel = GetChannel(e.Channel);
            channel.Topic = e.Topic;
            var msg = channel.AddMessage(new IrcNotificationMessage(e.ToString()));
            if (channel.IsSelected)
            {
                SelectedChannelHistory.Add(msg);
            }
        }

        private readonly Dictionary<string, bool> ChannelUsersUpdates = new();
        private void OnChannelUsersReceived(object sender, IrcChannelUsers e)
        {
            var channel = GetChannel(e.Channel);
            channel.Users.AddRange(e.Users);
            if (ChannelUsersUpdates.ContainsKey(e.Channel))
            {
                ChannelUsersUpdates[e.Channel] = true;
            }
            else
            {
                ChannelUsersUpdates.Add(e.Channel, true);
            }
            if (SelectedChannel is not null && SelectedChannel.Name == e.Channel)
            {
                UpdateSelectedChannelUsers();
            }
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
                    SelectedChannelPlayers.Clear();
                }
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
