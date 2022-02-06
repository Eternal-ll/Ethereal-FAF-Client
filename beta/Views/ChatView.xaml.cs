using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Properties;
using beta.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace beta.Views
{
    public struct ChannelMessage
    {
        public string From { get; set; }
        public string Message { get; set; }
        public DateTime Created => DateTime.Now;
    }
    public class Channel : ViewModel
    {
        #region Name
        private string _Name;
        public string Name
        {
            get => _Name;
            set
            {
                if (Set(ref _Name, value))
                    OnPropertyChanged(nameof(FormattedName));
            }
        }
        public string FormattedName => _Name.Substring(1);
        #endregion

        #region Title
        private string _Title;
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }
        #endregion

        #region History
        private readonly ObservableCollection<ChannelMessage> _History = new();
        public ObservableCollection<ChannelMessage> History => _History;
        #endregion

        #region Users
        private readonly ObservableCollection<PlayerInfoMessage> _Users = new();
        public ObservableCollection<PlayerInfoMessage> Users => _Users;
        #endregion

        #region UsersView
        public readonly CollectionViewSource UsersViewSource = new();
        public ICollectionView UsersView => UsersViewSource.View;
        #endregion

        #region FilterText
        private string _FilterText = "";
        public string FilterText
        {
            get => _FilterText;
            set
            {
                if (Set(ref _FilterText, value))
                {
                    UsersView.Refresh();
                }
            }
        }
        #endregion

        private void OnFilterText(object sender, FilterEventArgs e)
        {
            var filter = _FilterText;
            if (filter.Length == 0) return;
            var player = (PlayerInfoMessage)e.Item;
            e.Accepted = player.login.Contains(filter, StringComparison.OrdinalIgnoreCase) || (player.clan != null && player.clan.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        public Channel()
        {
            UsersViewSource.Source = Users;
            UsersViewSource.Filter += OnFilterText;
        }
    }
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl, INotifyPropertyChanged
    {
        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        private readonly IrcClient IrcClient;
        private readonly ILobbySessionService LobbySessionService;
        //private readonly object _lock = new();

        #region Properties
        public ObservableCollection<Channel> Channels { get; } = new();

        #region SelectedChannel
        private Channel _SelectedChannel;
        public Channel SelectedChannel
        {
            get => _SelectedChannel;
            set
            {
                _SelectedChannel = value;
                OnPropertyChanged(nameof(SelectedChannel));
            }
        }
        #endregion

        #endregion
        public string GGG => "gerge :test: tests :gweg : egweg:  : ewgew :eteg:";
        public IList Inlines => new List<Inline>()
        {
            new Run() { Text = "Test" },
            new Run() { Text = "Test" },
            new Run() { Text = "Test" },
            new InlineUIContainer()
            {
                Child =  new Image()
                {
                    Source = App.Current.Resources["QuestionIcon"] as ImageSource
                }
            },
            new InlineUIContainer()
            {
                Child =  new Image(){Source = App.Current.Resources["QuestionIcon"] as ImageSource}
            },
            new InlineUIContainer()
            {
                Child =  new Image(){Source = App.Current.Resources["QuestionIcon"] as ImageSource}
            },
            new Run() { Text = "Test" },
            new Run() { Text = "Test" },
            new Run() { Text = "Test" },
        };
        public ChatView()
        {
            InitializeComponent();
            DataContext = this;
            Channels.Add(new() { Name = "#server" });
            SelectedChannel = Channels[0];
            TextBox.KeyDown += TextBox_KeyDown;
            return;


            LobbySessionService = App.Services.GetService<ILobbySessionService>();

            //BindingOperations.EnableCollectionSynchronization(LobbySessionService.Players, _lock);

            IrcClient = new IrcClient("116.202.155.226", 6697);
            
            string nick = Settings.Default.PlayerNick;
            string id = Settings.Default.PlayerId.ToString();
            string password = Settings.Default.irc_password;
            IrcClient.Nick = nick;
            IrcClient.ServerPass = password;
            IrcClient.Connect();
            IrcClient.ServerMessage += IrcClient_ServerMessage;
            IrcClient.ChannelMessage += IrcClient_ChannelMessage;
            IrcClient.UpdateUsers += IrcClient_UpdateUsers;
            IrcClient.OnConnect += IrcClient_OnConnect;
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) 
            {
                SelectedChannel.History.Add(new() { Message = TextBox.Text });
            }
        }

        private void IrcClient_UpdateUsers(object sender, UpdateUsersEventArgs e)
        {
            ObservableCollection<PlayerInfoMessage> users = null;
            for (int i = 0; i < Channels.Count; i++)
            {
                var channel = Channels[i];
                if (channel.Name == e.Channel)
                {
                    users = channel.Users;
                    break;
                }
            }

            for (int i = 0; i < e.UserList.Length; i++)
            {
                var playerInfo = LobbySessionService.GetPlayerInfo(e.UserList[i]);
                if (playerInfo != null)
                    users.Add(playerInfo);
            }
        }
        private void IrcClient_OnConnect(object sender, EventArgs e)
        {
            IrcClient.JoinChannel("#aeolus");
            Channels.Add(new() { Name = "#aeolus" });
        }

        private void IrcClient_ChannelMessage(object sender, ChannelMessageEventArgs e)
        {
            e.Message = e.Message.Replace('\n', ' ');
            e.Message = e.Message.Replace('\r', ' ');
            e.From = e.From.Replace('\n', ' ');
            e.From = e.From.Replace('\r', ' ');

            for (int i = 0; i < Channels.Count; i++)
            {
                var channel = Channels[i];
                if (channel.Name == e.Channel)
                {
                    channel.History.Add(new()
                    {
                        From = e.From,
                        Message = e.Message
                    });
                    break;
                }
            }
        }

        private void IrcClient_ServerMessage(object sender, StringEventArgs e)
        {
            e.Result = e.Result.Replace('\n', ' ');
            e.Result = e.Result.Replace('\r', ' ');
            Channels[0].History.Add(new() { Message = e.Result });
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBox.Text)) return;

            string nick = Settings.Default.PlayerNick;
            var channel = SelectedChannel.Name;
            var msg = TextBox.Text;
            IrcClient.SendMessage(channel, msg);

            SelectedChannel.History.Add(new()
            {
                From = nick,
                Message = msg
            });
            TextBox.Clear();
        }

        private void JoinChannel(object sender, RoutedEventArgs e)
        {
            var channel = JoinChannelInput.Text;
            if (channel[0] != '#')
                channel = "#" + channel;

            for (int i = 0; i < Channels.Count; i++)
            {
                if (Channels[i].Name == channel)
                {
                    // raise something
                    return;
                }
            }

            IrcClient.JoinChannel(channel);
            Channels.Add(new() { Name = channel });

            JoinChannelInput.Text = string.Empty;
        }
    }
}
