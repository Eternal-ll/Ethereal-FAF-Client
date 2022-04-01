using beta.Infrastructure;
using beta.Infrastructure.Commands;
using beta.Infrastructure.Converters;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Enums;
using beta.Models.IRC;
using beta.Properties;
using beta.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl, INotifyPropertyChanged
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

        private readonly IPlayersService PlayersService;
        private readonly IIrcService IrcService;

        #region IsChatMapPreviewEnabled
        private bool _IsChatMapPreviewEnabled;
        public bool IsChatMapPreviewEnabled
        {
            get => _IsChatMapPreviewEnabled;
            set
            {
                if (_IsChatMapPreviewEnabled != value)
                {
                    _IsChatMapPreviewEnabled = value;
                    OnPropertyChanged(nameof(IsChatMapPreviewEnabled));
                }
            }
        }
        #endregion

        public ObservableCollection<IrcChannelVM> Channels { get; } = new();

        #region SelectedChannel
        private IrcChannelVM _SelectedChannel;
        public IrcChannelVM SelectedChannel
        {
            get => _SelectedChannel;
            set
            {
                if (value != _SelectedChannel)
                {
                    _SelectedChannel = value;
                    OnPropertyChanged(nameof(SelectedChannel));
                    UpdateSelectedChannelUsers();

                    //if (value is not null)
                    //    TestInputControl.Users = value.Users;
                    TestInputControl.SelectedChannel = value;
                }
            }
        }
        #endregion

        private readonly ObservableCollection<IPlayer> SelectedChannelPlayers = new();

        private readonly CollectionViewSource SelectedChannelPlayersViewSource = new();
        public ICollectionView SelectedChannelPlayersView => SelectedChannelPlayersViewSource.View;

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

        #region SelectedUser
        private IPlayer _SelectedUser;
        public IPlayer SelectedUser
        {
            get => _SelectedUser;
            set => Set(ref _SelectedUser, value);
        }
        #endregion

        private readonly object _lock = new();

        #endregion

        public ChatView()
        {
            InitializeComponent();

            DataContext = this;

            PlayersService = App.Services.GetService<IPlayersService>();
            IrcService = App.Services.GetService<IIrcService>();

            SelectedChannelPlayersViewSource.Source = SelectedChannelPlayers;
            BindingOperations.EnableCollectionSynchronization(SelectedChannelPlayers, _lock);
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

            #region IrcService event listeners
            IrcService.StateChanged += OnStateChanged;

            IrcService.ChannelUsersReceived += OnChannelUsersReceived;
            IrcService.ChannelTopicUpdated += OnChannelTopicUpdated;
            IrcService.ChannelTopicChangedBy += OnChannelTopicChangedBy;
            IrcService.UserJoined += OnChannelUserJoin;
            IrcService.UserLeft += OnChannelUserLeft;
            IrcService.UserChangedName += OnUserChangedName;

            IrcService.ChannedMessageReceived += OnChannelMessageReceived;
            #endregion

            TestInputControl.LeaveRequired += (s, e) => OnLeaveFromChannelCommand(SelectedChannel.Name);

            App.Current.MainWindow.Closing += MainWindow_Closing;

            _LeaveFromChannelCommand = new LambdaCommand(OnLeaveFromChannelCommand);
            _RefreshUserListCommand = new LambdaCommand(OnRefreshUserListCommand);
            _HideSelectedUserCommand = new LambdaCommand(OnHideSelectedUserCommand);

            GlobalGrid.Resources.Add("LeaveFromChannelCommand", _LeaveFromChannelCommand);
        }

        #region Commands

        #region LeaveFromChannelCommand
        private ICommand _LeaveFromChannelCommand;
        public ICommand LeaveFromChannelCommand => _LeaveFromChannelCommand;
        private void OnLeaveFromChannelCommand(object parameter)
        {
            var channel = parameter.ToString();
            IrcService.Leave(channel);
            int i = 0;

            while (i < Channels.Count)
            {
                if (Channels[i].Name == channel)
                {
                    Channels.RemoveAt(i);
                }

                i++;
            }

            if (SelectedChannel is null)
            {
                if (Channels.Count > 0) SelectedChannel = Channels[0];
                //i--;
                //if ()
                //{
                //    SelectedChannel = Channels[i - 2];
                //}
                //else if (i < Channels.Count - 1)
                //{
                //    SelectedChannel = Channels[i + 1];
                //}
            }
        }

        #endregion

        #region UpdateUsersCommand
        private ICommand _RefreshUserListCommand;
        public ICommand RefreshUserListCommand => _RefreshUserListCommand;
        private void OnRefreshUserListCommand(object parameter) => UpdateSelectedChannelUsers();
        #endregion

        #region HideSelectedUserCommand
        private ICommand _HideSelectedUserCommand;
        public ICommand HideSelectedUserCommand => _HideSelectedUserCommand;
        private void OnHideSelectedUserCommand(object parameter) => SelectedUser = null;
        #endregion

        #endregion

        private void OnUserChangedName(object sender, IrcUserChangedName e)
        {
            for (int i = 0; i < Channels.Count; i++)
            {
                var channel = Channels[i];

                for (int j = 0; j < channel.Users.Count; j++)
                {
                    var user = channel.Users[j];
                    if (user == e.From)
                    {
                        channel.Users[j] = e.To;

                        if (SelectedChannel.Name == channel.Name)
                        {
                            SelectedChannelPlayers.Remove(GetChatPlayer(user));
                            SelectedChannelPlayers.Add(GetChatPlayer(e.To));
                        }

                        break;
                    }
                }
            }
        }

        private void OnShowJoinToChannelClick(object sender, RoutedEventArgs e) => JoinChannelInput.Focus();
        private void MainWindow_Closing(object sender, CancelEventArgs e) => IrcService.Quit();

        private void JoinToChannelOnEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var channel = JoinChannelInput.Text;
                if (string.IsNullOrWhiteSpace(channel)) return;
                if (channel[0] != '#')
                    channel = "#" + channel;

                IrcService.Join(channel);

                JoinChannelInput.Text = string.Empty;
            }
        }


        private void PlayersFilter(object sender, FilterEventArgs e)
        {
            var player = (IPlayer)e.Item;

            if (FilterText.Length == 0) return;

            e.Accepted = player.login.StartsWith(FilterText, System.StringComparison.OrdinalIgnoreCase);
        }

        private IPlayer GetChatPlayer(string user)
        {
            bool isChatMod = user.StartsWith('@');

            if (isChatMod) user = user.Substring(1);

            var player = PlayersService.GetPlayer(user);
            if (player is not null)
            {
                player.IsChatModerator = isChatMod;
                return player;
            }
            else
            {
                return new UnknownPlayer()
                {
                    IsChatModerator = isChatMod,
                    login = user
                };
            }
        }
        private void UpdateSelectedChannelUsers()
        {
            var players = SelectedChannelPlayers;

            // TODO Check for duplicates someday... I saw two players on visual users list
            players.Clear();

            if (SelectedChannel is null) return;

            var users = SelectedChannel.Users;

            for (int i = 0; i < users.Count; i++)
            {
                players.Add(GetChatPlayer(users[i]));
            }

            //OnPropertyChanged(nameof(SelectedChannelPlayersView));
            //SelectedChannelPlayersView.Refresh();
            //using var defer = View.DeferRefresh();
        }
        private bool TryGetChannel(string channelName, out IrcChannelVM channel)
        {
            var channels = Channels;
            for (int i = 0; i < channels.Count; i++)
            {
                if (channels[i].Name == channelName)
                {
                    channel =  channels[i];
                    return true;
                }
            }
            channel = null;
            return false;
        }

        #region Events listeners
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
        }

        private void OnChannelMessageReceived(object sender, IrcChannelMessage e)
        {
            var channel = GetChannel(e.Channel);
            channel.History.Add(e);
        }

        private void OnChannelTopicChangedBy(object sender, IrcChannelTopicChangedBy e)
        {
            var channel = GetChannel(e.Channel);

            channel.TopicChangedBy = e;
        }

        private void OnChannelTopicUpdated(object sender, IrcChannelTopicUpdated e)
        {
            var channel = GetChannel(e.Channel);

            channel.Topic = e.Topic;
        }

        private void OnChannelUserLeft(object sender, IrcUserLeft e)
        {
            if (TryGetChannel(e.Channel, out IrcChannelVM channel))
            {
                channel.Users.Remove(e.User);

                if (channel.Name.Equals(SelectedChannel.Name))
                {
                    SelectedChannelPlayers.Remove(GetChatPlayer(e.User));
                }
            }
            else
            {
                // todo
            }
        }
        private void OnChannelUserJoin(object sender, IrcUserJoin e)
        {
            if (TryGetChannel(e.Channel, out IrcChannelVM channel))
            {
                if (!channel.Users.Contains(e.User))
                {
                    channel.Users.Add(e.User);

                    if (channel.Name.Equals(SelectedChannel?.Name))
                    {
                        SelectedChannelPlayers.Add(GetChatPlayer(e.User));
                    }
                }
            }
            else
            {
                // todo
            }
        }
        private void OnChannelUsersReceived(object sender, IrcChannelUsers e)
        {
            var channel = GetChannel(e.Channel);

            // User join event working earlier and we getting double current authorized player
            if (channel.Users.Count == 1)
            {
                channel.Users.Clear();
            }

            for (int i = 0; i < e.Users.Length; i++)
            {
                channel.Users.Add(e.Users[i]);
            }
            if (SelectedChannel is null)
            {
                SelectedChannel = channel;
            }
            else if (SelectedChannel.Name == channel.Name)
            {
                UpdateSelectedChannelUsers();
            }
        }

        private IrcChannelVM GetChannel(string name)
        {
            if (!TryGetChannel(name, out IrcChannelVM channel))
            {
                channel = new(name);
                Channels.Add(channel);
                SelectedChannel = channel;
                //BindingOperations.EnableCollectionSynchronization(channel.Users, _lock);
                BindingOperations.EnableCollectionSynchronization(channel.History, _lock);
            }
            return channel;
        }
        #endregion
    }
}
