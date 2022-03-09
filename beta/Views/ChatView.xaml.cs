using beta.Infrastructure;
using beta.Infrastructure.Commands;
using beta.Infrastructure.Converters;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
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
                }
            }
        }
        #endregion

        private readonly ObservableCollection<IPlayer> SelectedChannelPlayers = new();

        private readonly CollectionViewSource SelectedChannelPlayersViewSource = new();
        public ICollectionView SelectedChannelPlayersView => SelectedChannelPlayersViewSource.View;

        private readonly CollectionViewSource OnlinePlayersViewSource = new();
        public ICollectionView OnlinePlayersView => OnlinePlayersViewSource.View;

        #region FilterText
        private string _FilterText = string.Empty;
        public string FilterText
        {
            get => _FilterText;
            set
            {
                if (Set(ref _FilterText, value))
                {
                    if (SelectedChannelPlayersView != null)
                        SelectedChannelPlayersView.Refresh();
                    else OnlinePlayersView.Refresh();
                }
            }
        }
        #endregion

        #region WelcomeGridVisibility
        private Visibility _WelcomeGridVisibility = Visibility.Visible;
        public Visibility WelcomeGridVisibility
        {
            get => _WelcomeGridVisibility;
            set
            {
                if (Set(ref _WelcomeGridVisibility, value))
                {
                    ChatGridVisibility = value == Visibility.Visible ? Visibility.Collapsed : Visibility;
                }
            }
        } 
        #endregion

        #region ChatGridVisibility
        private Visibility _ChatGridVisibility = Visibility.Collapsed;
        public Visibility ChatGridVisibility
        {
            get => _ChatGridVisibility;
            set => Set(ref _ChatGridVisibility, value);
        }
        #endregion

        #region PendingConnectionToIRC
        private bool _PendingConnectionToIRC;
        public bool PendingConnectionToIRC
        {
            get => _PendingConnectionToIRC;
            set
            {
                if (Set(ref _PendingConnectionToIRC, value))
                {
                    OnPropertyChanged(nameof(IsRequestConnectBtnEnabled));
                }
            }
        }
        #endregion

        public bool IsRequestConnectBtnEnabled => !PendingConnectionToIRC;

        private readonly object _lock = new object();

        #endregion

        public ChatView()
        {
            InitializeComponent();

            DataContext = this;

            PlayersService = App.Services.GetService<IPlayersService>();
            IrcService = App.Services.GetService<IIrcService>();

            //string nick = Settings.Default.PlayerNick;
            //string id = Settings.Default.PlayerId.ToString();
            //string password = Settings.Default.irc_password;
            //IrcService.Authorize(nick, password);

            if (IrcService.IsIRCConnected)
            {
                WelcomeGridVisibility = Visibility.Collapsed;
            }

            BindingOperations.EnableCollectionSynchronization(PlayersService.Players, _lock);
            OnlinePlayersViewSource.Source = PlayersService.Players;

            SelectedChannelPlayersViewSource.Source = SelectedChannelPlayers;
            BindingOperations.EnableCollectionSynchronization(SelectedChannelPlayers, _lock);
            BindingOperations.EnableCollectionSynchronization(Channels, _lock);

            PropertyGroupDescription groupDescription = new("", new ChatUserGroupConverter());
            groupDescription.GroupNames.Add("Me");
            groupDescription.GroupNames.Add("Moderators");
            groupDescription.GroupNames.Add("Friends");
            groupDescription.GroupNames.Add("Clan");
            groupDescription.GroupNames.Add("Players");
            groupDescription.GroupNames.Add("IRC users");
            groupDescription.GroupNames.Add("Foes");
            SelectedChannelPlayersViewSource.GroupDescriptions.Add(groupDescription);

            SelectedChannelPlayersViewSource.Filter += PlayersFilter;
            OnlinePlayersViewSource.Filter += PlayersFilter;

            #region IrcService event listeners
            IrcService.IrcConnected += OnIrcConnected;

            IrcService.ChannelUsersReceived += OnChannelUsersReceived;
            IrcService.ChannelTopicUpdated += OnChannelTopicUpdated;
            IrcService.ChannelTopicChangedBy += OnChannelTopicChangedBy;
            IrcService.UserJoined += OnChannelUserJoin;
            IrcService.UserLeft += OnChannelUserLeft;

            IrcService.ChannedMessageReceived += OnChannelMessageReceived; 
            #endregion

            App.Current.MainWindow.Closing += MainWindow_Closing;

            _LeaveFromChannelCommand = new LambdaCommand(OnLeaveFromChannelCommand);
            var t = GlobalGrid.Resources;
            GlobalGrid.Resources.Add("LeaveFromChannelCommand", _LeaveFromChannelCommand);
        }

        private void OnConnectRequestBtnClick(object sender, RoutedEventArgs e) => ConnectToIRC();
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

        private void ConnectToIRC()
        {
            PendingConnectionToIRC = true;
            IrcService.Authorize(Settings.Default.PlayerNick, Settings.Default.irc_password);
        }

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

            if (i > 0)
            {
                SelectedChannel = Channels[i - 1];
            }
            else if (i < Channels.Count - 1)
            {
                SelectedChannel = Channels[i + 1];
            }
        }

        #endregion

        private void PlayersFilter(object sender, FilterEventArgs e)
        {
            var player = (IPlayer)e.Item;

            if (FilterText.Length == 0) return;

            e.Accepted = player.login.StartsWith(FilterText);
        }

        private IPlayer GetChatPlayer(string user)
        {
            bool isChatMod = user.StartsWith('@');

            if (isChatMod) user = user.Substring(1);

            var player = PlayersService.GetPlayer(user);
            if (player != null)
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

            if (SelectedChannel == null) return;

            var users = SelectedChannel.Users;

            for (int i = 0; i < users.Count; i++)
            {
                players.Add(GetChatPlayer(users[i]));
            }

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
        private void OnIrcConnected(object sender, bool e)
        {
            if (e)
            {
                WelcomeGridVisibility = Visibility.Collapsed;
                PendingConnectionToIRC = false;
                //BindingOperations.DisableCollectionSynchronization(PlayersService.Players);
                //OnlinePlayersViewSource.Source = null;

                for (int i = 0; i < Channels.Count; i++)
                {
                    IrcService.Join(Channels[i].Name);
                }
            }
            else
            {
                WelcomeGridVisibility = Visibility.Visible;
                for (int i = 0; i < Channels.Count; i++)
                {
                    Channels[i].Users.Clear();
                }
                //BindingOperations.EnableCollectionSynchronization(PlayersService.Players, _lock);
                //OnlinePlayersViewSource.Source = PlayersService.Players;
            }
            Dispatcher.Invoke(() => FilterText = string.Empty);
        }

        private void OnChannelMessageReceived(object sender, EventArgs<IrcChannelMessage> e)
        {
            if (TryGetChannel(e.Arg.Channel, out IrcChannelVM channel))
            {
                channel.History.Add(e.Arg);
            }
            else
            {
                // todo
            }
        }

        private void OnChannelTopicChangedBy(object sender, EventArgs<IrcChannelTopicChangedBy> e)
        {
            if (TryGetChannel(e.Arg.Channel, out IrcChannelVM channel))
            {
                channel.TopicChangedBy = e.Arg;
            }
            else
            {
                // todo
            }
        }

        private void OnChannelTopicUpdated(object sender, EventArgs<IrcChannelTopicUpdated> e)
        {
            if (TryGetChannel(e.Arg.Channel, out IrcChannelVM channel))
            {
                channel.Topic = e.Arg.Topic;
            }
            else
            {
                // todo
            }
        }

        private void OnChannelUserLeft(object sender, EventArgs<IrcUserLeft> e)
        {
            if (TryGetChannel(e.Arg.Channel, out IrcChannelVM channel))
            {
                channel.Users.Remove(e.Arg.User);

                if (channel.Name.Equals(SelectedChannel.Name))
                {
                    SelectedChannelPlayers.Add(GetChatPlayer(e.Arg.User));
                }
            }
            else
            {
                // todo
            }
        }
        private void OnChannelUserJoin(object sender, EventArgs<IrcUserJoin> e)
        {
            if (TryGetChannel(e.Arg.Channel, out IrcChannelVM channel))
            {
                //Dispatcher.Invoke(() => Channels.Add(channel));
                channel.Users.Add(e.Arg.User);

                if (channel.Name.Equals(SelectedChannel.Name))
                {
                    SelectedChannelPlayers.Add(GetChatPlayer(e.Arg.User));
                }
            }
            else
            {
                // todo
            }
        }
        private void OnChannelUsersReceived(object sender, EventArgs<IrcChannelUsers> e)
        {
            if (!TryGetChannel(e.Arg.Channel, out IrcChannelVM channel))
            {
                channel = new(e.Arg.Channel);
                Channels.Add(channel);
                //Dispatcher.Invoke(() => Channels.Add(channel));
                //BindingOperations.EnableCollectionSynchronization(channel.Users, _lock);
                BindingOperations.EnableCollectionSynchronization(channel.History, _lock);
            }

            for (int i = 0; i < e.Arg.Users.Length; i++)
            {
                channel.Users.Add(e.Arg.Users[i]);
            }

            if (SelectedChannel == null)
            {
                SelectedChannel = channel;
            }
            else if (SelectedChannel.Name == channel.Name)
            {
                UpdateSelectedChannelUsers();
            }
        }
        #endregion
    }
}
