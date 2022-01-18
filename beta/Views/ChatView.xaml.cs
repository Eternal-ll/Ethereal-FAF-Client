using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace beta.Views
{
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


        public ChatView()
        {
            InitializeComponent();

            LobbySessionService = App.Services.GetService<ILobbySessionService>();

            DataContext = this;

            IrcClient = new IrcClient("116.202.155.226", 6697);
            
            string nick = Properties.Settings.Default.PlayerNick;
            string id = Properties.Settings.Default.PlayerId.ToString();
            string password = Properties.Settings.Default.irc_password;
            IrcClient.Nick = nick;
            IrcClient.ServerPass = password;
            IrcClient.Connect();
            IrcClient.ServerMessage += IrcClient_ServerMessage;
            IrcClient.ChannelMessage += IrcClient_ChannelMessage;
            IrcClient.UpdateUsers += IrcClient_UpdateUsers;
            IrcClient.OnConnect += IrcClient_OnConnect;
        }

        //public PlayerInfoMessage GetPlayerInfo => LobbySessionService.Players[LobbySessionService.PlayerNameToId[""]];


        private void IrcClient_UpdateUsers(object sender, UpdateUsersEventArgs e)
        {
            if (_ChannelUsers.TryGetValue(e.Channel, out var channel))
                for (int i = 0; i < e.UserList.Length; i++)
                    channel.Add(e.UserList[i]);
            else
            {
                _ChannelUsers.Add(e.Channel, new HashSet<string>(e.UserList));
                //for (int i = 0; i < e.UserList.Length; i++)
                //    _ChannelUsers[e.Channel].Add(e.UserList[i]);
            }
            if (SelectedChannelName == e.Channel)
                SelectedChannelName = SelectedChannelName;
                //OnPropertyChanged(nameof(SelectedChannelUsers));
        }

        private readonly Dictionary<string, IList<string>> _Channels = new()
        {
            { "#server", new List<string>() }
        };

        private readonly Dictionary<string, HashSet<string>> _ChannelUsers = new();

        public Dictionary<string, IList<string>> Channels => _Channels;

        public HashSet<string> SelectedChannelUsers => SelectedChannelName != null ? _ChannelUsers.ContainsKey(SelectedChannelName) ? _ChannelUsers[SelectedChannelName] : null : null;

        public IList<string> SelectedChannelHistory => SelectedChannelName != null ? _Channels[SelectedChannelName] : null;

        private string _SelectedChannelName;
        public string SelectedChannelName
        {
            get => _SelectedChannelName;
            set
            {
                _SelectedChannelName = value;
                OnPropertyChanged(nameof(SelectedChannelUsers));
                OnPropertyChanged(nameof(SelectedChannelHistory));
            }
        }
        private void IrcClient_OnConnect(object sender, System.EventArgs e)
        {
            IrcClient.JoinChannel("#aeolus");
            _Channels.Add("#aeolus", new List<string>());
            OnPropertyChanged(nameof(Channels));
        }

        private void IrcClient_ChannelMessage(object sender, ChannelMessageEventArgs e)
        {
            e.Message = e.Message.Replace('\n', ' ');
            e.From = e.From.Replace('\n', ' ');
            var msg = e.From + ": " + e.Message;
            if (_Channels.TryGetValue(e.Channel, out var channel)) 
                channel.Add(msg);
            else
            {
                _Channels.Add(e.Channel, new List<string>() { msg });
                OnPropertyChanged(nameof(Channels));
            }
            if (SelectedChannelName == e.Channel)
                SelectedChannelName = SelectedChannelName;
                //OnPropertyChanged(nameof(SelectedChannelHistory));
        }

        private void IrcClient_ServerMessage(object sender, StringEventArgs e)
        {
            e.Result = e.Result.Replace('\n', ' ');
            _Channels["#server"].Add(e.Result);
            if (SelectedChannelName == "#server")
                SelectedChannelName = SelectedChannelName;
        }

        private bool s = false;
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBox.Text)) return;
            
            string nick = Properties.Settings.Default.PlayerNick;
            var channel = SelectedChannelName;
            var msg = TextBox.Text;
            IrcClient.SendMessage(channel, msg);

            msg = nick + ": " + msg;

            _Channels[channel].Add(msg);

            if (SelectedChannelName == channel)
                SelectedChannelName = SelectedChannelName;
                //OnPropertyChanged(nameof(SelectedChannelHistory));

            TextBox.Clear();
        }
    }
}
