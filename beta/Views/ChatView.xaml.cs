using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
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
        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        private readonly IrcClient IrcClient;
        private readonly ILobbySessionService LobbySessionService;

        private readonly CollectionViewSource ChattersViewSource = new();
        public ICollectionView ChattersView => ChattersViewSource.View;
        private readonly object _lock = new();
        public ChatView()
        {
            InitializeComponent();
            DataContext = this;

            LobbySessionService = App.Services.GetService<ILobbySessionService>();

            BindingOperations.EnableCollectionSynchronization(LobbySessionService.Players, _lock);
            ChattersViewSource.Source = LobbySessionService.Players;
            ChattersViewSource.Filter += ChattersViewSource_Filter;

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
        private Dictionary<string, List<string>> Test = new();
        private void ChattersViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = false;
            if (SelectedChannelName == null || SelectedChannelName == "#server") return;
            var enumerator = Test[SelectedChannelName].GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current == ((PlayerInfoMessage)e.Item).login)
                {
                    e.Accepted = true;
                    return;
                }
            }
        }

        private readonly CollectionViewSource HistoryViewSource = new();
        public ICollectionView HistoryView => HistoryViewSource.View;

        private void IrcClient_UpdateUsers(object sender, UpdateUsersEventArgs e)
        {
            if (_ChannelUsers.TryGetValue(e.Channel, out var channel))
                for (int i = 0; i < e.UserList.Length; i++)
                    channel.Add(e.UserList[i]);
            if (Test.TryGetValue(e.Channel, out var channell))
                for (int i = 0; i < e.UserList.Length; i++)
                    channell.Add(e.UserList[i]);
            else
            {
                _ChannelUsers.Add(e.Channel, new HashSet<string>(e.UserList));
                Test.Add(e.Channel, new List<string>(e.UserList));
                //for (int i = 0; i < e.UserList.Length; i++)
                //    _ChannelUsers[e.Channel].Add(e.UserList[i]);
            }
            if (SelectedChannelName == e.Channel)
                SelectedChannelName = SelectedChannelName;
                //OnPropertyChanged(nameof(SelectedChannelUsers));
        }

        private readonly Dictionary<string, BindingList<string>> _Channels = new()
        {
            { "#server", new BindingList<string>() }
        };

        private readonly Dictionary<string, HashSet<string>> _ChannelUsers = new();

        public Dictionary<string, BindingList<string>> Channels => _Channels;

        public HashSet<string> SelectedChannelUsers => SelectedChannelName != null ? _ChannelUsers.ContainsKey(SelectedChannelName) ? _ChannelUsers[SelectedChannelName] : null : null;

        //public IList<string> SelectedChannelHistory => SelectedChannelName != null ? _Channels[SelectedChannelName] : null;
        private List<string> t = new();
        public IList<string> SelectedChannelHistory => t;
        private BindingList<string> _BindingList = new();
        public BindingList<string> BindingList
        {
            get => _BindingList;
            set
            {
                _BindingList = value;
                OnPropertyChanged();
            }
        }

        private string _SelectedChannelName;
        public string SelectedChannelName
        {
            get => _SelectedChannelName;
            set
            {
                _SelectedChannelName = value;
                OnPropertyChanged(nameof(SelectedChannelUsers));
                OnPropertyChanged(nameof(SelectedChannelHistory));
                ChattersView.Refresh();
            }
        }
        private void IrcClient_OnConnect(object sender, System.EventArgs e)
        {
            IrcClient.JoinChannel("#aeolus");
            _Channels.Add("#aeolus", new BindingList<string>());
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
                _Channels.Add(e.Channel, new BindingList<string>() { msg });
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
