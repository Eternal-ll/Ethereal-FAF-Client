using beta.Infrastructure;
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

        #region FilterText
        private string _FilterText = string.Empty;
        public string FilterText
        {
            get => _FilterText;
            set
            {
                if (Set(ref _FilterText, value))
                    SelectedChannelPlayersView.Refresh();
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
                SelectedChannelPlayersViewSource.Source = SelectedChannelPlayers;
            }
            else
            {
                WelcomeGridVisibility = Visibility.Visible;
                BindingOperations.EnableCollectionSynchronization(PlayersService.Players, _lock);
                SelectedChannelPlayersViewSource.Source = PlayersService.Players;
            }

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
        }

        private void OnIrcConnected(object sender, bool e)
        {
            if (e)
            {
                WelcomeGridVisibility = Visibility.Collapsed;
                SelectedChannelPlayersViewSource.Source = SelectedChannelPlayers;
                BindingOperations.DisableCollectionSynchronization(PlayersService.Players);
            }
            else
            {
                WelcomeGridVisibility = Visibility.Visible;
                BindingOperations.EnableCollectionSynchronization(PlayersService.Players, _lock);
                SelectedChannelPlayersViewSource.Source = PlayersService.Players;
            }
        }

        private void PlayersFilter(object sender, FilterEventArgs e)
        {
            var player = (IPlayer)e.Item;

            if (FilterText.Length == 0) return;

            e.Accepted = player.login.StartsWith(FilterText);
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
            var users = SelectedChannel.Users;

            players.Clear();
            for (int i = 0; i < users.Count; i++)
            {
                players.Add(GetChatPlayer(users[i]));
            }

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
            }

            for (int i = 0; i < e.Arg.Users.Length; i++)
            {
                channel.Users.Add(e.Arg.Users[i]);
            }

            Channels.Add(channel);
        } 

        #endregion

        private void JoinToChannelOnEnter(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var channel = JoinChannelInput.Text;
                if (string.IsNullOrWhiteSpace(channel)) return;
                if (channel[0] != '#')
                    channel = "#" + channel;

                IrcService.Join(channel);

                JoinChannelInput.Text = string.Empty;
            }
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e) => IrcService.Quit();
    }
}
