using beta.Infrastructure.Converters;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Properties;
using beta.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
        #endregion

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

        private readonly IPlayersService PlayersService;
        private readonly IIrcService IrcService;

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
                    if (value != null)
                    {
                        _SelectedChannel.Users.CollectionChanged -= Users_CollectionChanged;
                        value.Users.CollectionChanged += Users_CollectionChanged;
                    }
                }
            }
        }

        private void Users_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (sender is ObservableCollection<string> users)
            {
                if (users != _SelectedChannel.Users)
                {
                    users.CollectionChanged -= Users_CollectionChanged;
                    return;
                }

                var players = SelectedChatPlayers;
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            players.Add(GetChatPlayer(e.NewItems[i].ToString()));
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            players.Remove(GetChatPlayer(e.OldItems[i].ToString()));
                        }
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        break;
                }
                //View.Refresh();
            }
        }
        #endregion

        private ObservableCollection<IPlayer> SelectedChatPlayers = new();

        private readonly CollectionViewSource SelectedChannelUsersViewSource = new();
        public ICollectionView View => SelectedChannelUsersViewSource.View;

        #region Thickness
        // for suggestion box above the input box
        // offset on X from left
        private Thickness _Thickness;
        public Thickness Thickness
        {
            get => _Thickness;
            set
            {
                _Thickness = value;
                OnPropertyChanged(nameof(Thickness));
            }
        }
        #endregion

        #region VisibilityTest
        // visibility of suggestionBox
        private Visibility _Visibility = Visibility.Hidden;
        public Visibility VisibilityTest
        {
            get => _Visibility;
            set
            {
                _Visibility = value;
                OnPropertyChanged(nameof(VisibilityTest));
            }
        }
        #endregion

        #endregion

        public ChatView()
        {
            InitializeComponent();

            PlayersService = App.Services.GetService<IPlayersService>();
            IrcService = App.Services.GetService<IIrcService>();
            
            string nick = Settings.Default.PlayerNick;
            string id = Settings.Default.PlayerId.ToString();
            string password = Settings.Default.irc_password;

            //IrcService.Authorize(nick, password);

            PropertyGroupDescription groupDescription = new("", new ChatUserGroupConverter());
            groupDescription.GroupNames.Add("Me");
            groupDescription.GroupNames.Add("Moderators");
            groupDescription.GroupNames.Add("Friends");
            groupDescription.GroupNames.Add("Clan");
            groupDescription.GroupNames.Add("Players");
            groupDescription.GroupNames.Add("IRC users");
            groupDescription.GroupNames.Add("Foes");
            SelectedChannelUsersViewSource.GroupDescriptions.Add(groupDescription);

            DataContext = this;

            App.Current.MainWindow.Closing += MainWindow_Closing;

            MessageInput.TextChanged += OnMessageInputTextChanged;
            MessageInput.PreviewKeyDown += OnMessageInputPreviewKeyDown;
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
            ObservableCollection<IPlayer> players = SelectedChatPlayers;
            players.Clear();
            var users = _SelectedChannel.Users;
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
        private void IrcClient_UserLeft(object sender, UserLeftEventArgs e)
        {
            if (TryGetChannel(e.Channel, out IrcChannelVM channel))
            {
                channel.Users.Remove(e.User);
            }
            else
            {
                var channels = Channels;
                for (int i = 0; i < channels.Count; i++)
                {
                    channels[i].Users.Remove(e.User);
                }
            }
        }
        private void IrcClient_UserJoined(object sender, UserJoinedEventArgs e)
        {
            if (TryGetChannel(e.Channel, out IrcChannelVM channel))
            {
                channel.Users.Add(e.User);
            }
            else
            {

            }
        }
        private void IrcClient_UpdateUsers(object sender, UpdateUsersEventArgs e)
        {
            if (TryGetChannel(e.Channel, out var channel))
            {
                ObservableCollection<string> users = channel.Users;

                for (int i = 0; i < e.UserList.Length; i++)
                {
                    //var playerInfo = PlayersService.GetPlayer(e.UserList[i]);
                    //if (playerInfo != null)
                    //    users.Add(playerInfo);
                    //else users.Add(new UnknownPlayer()
                    //{
                    //    login = e.UserList[i]
                    //});
                    users.Add(e.UserList[i]);
                }
            }
        }
        private void IrcClient_OnConnect(object sender, EventArgs e)
        {
            //IrcClient.JoinChannel("#aeolus");
            Channels.Add(new() { Name = "#aeolus" });
        }
        private void IrcClient_ChannelMessage(object sender, ChannelMessageEventArgs e)
        {
            e.Message = e.Message.Replace('\n', ' ');
            e.Message = e.Message.Replace('\r', ' ');
            e.From = e.From.Replace('\n', ' ');
            e.From = e.From.Replace('\r', ' ');

            var login = "@" + Settings.Default.PlayerNick;

            for (int i = 0; i < Channels.Count; i++)
            {
                var channel = Channels[i];
                if (channel.Name == e.Channel)
                {
                    var len = channel.History.Count - 1;

                    if (len >= 0)
                    {
                        var previousMsg = channel.History[len];
                        while (previousMsg.From == null && len > 0)
                        {
                            len--;
                            previousMsg = channel.History[len];
                        }

                        if (previousMsg.From == e.From)
                            e.From = null;
                    }
                    channel.History.Add(new()
                    {
                        From = e.From,
                        Message = e.Message,
                        HasMention = e.Message.Contains(login, StringComparison.OrdinalIgnoreCase)
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

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            IrcService.Quit();
        }

        private void OnMessageInputPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ChangedByKey = true;

            var listbox = SuggestionListBox;
            var input = MessageInput;

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (VisibilityTest != Visibility.Visible)
                {
                    string nick = Settings.Default.PlayerNick;
                    var mention = input.Text.Contains(nick, StringComparison.OrdinalIgnoreCase);

                    var lent = SelectedChannel.History.Count - 1;
                    if (lent >= 0)
                    {
                        var previousNick = SelectedChannel.History[lent].From;
                        while (previousNick == null && lent > 0)
                        {
                            lent--;
                            previousNick = SelectedChannel.History[lent].From;
                        }

                        if (previousNick == nick)
                            nick = null;
                    }

                    SelectedChannel.History.Add(new()
                    {
                        From = nick,
                        Message = input.Text,
                        HasMention = mention
                    }) ;
                    MessageInput.Text = string.Empty;
                    return;
                }
                if(input.Text[^1] != ' ')
                    input.Text += " ";
                input.Select(input.Text.Length, 0);
                _foundedIndex = -1;

                SuggestionListBox.ItemsSource = null;
                VisibilityTest = Visibility.Collapsed;

                return;
            }

            if (listbox.ItemsSource == null) return;

            var selectedIndex = listbox.SelectedIndex;
            var len = listbox.Items.Count - 1;

            var originalIndex = _foundedIndex;
            var keyWordLen = _keyWordLen;

            var text = input.Text;
            //text = text.Substring(0, )

            if (e.Key == System.Windows.Input.Key.Down)
            {
                var previousItem = (PlayerInfoMessage)SuggestionListBox.SelectedItem;

                if (selectedIndex < len)
                    listbox.SelectedIndex++;
                else listbox.SelectedIndex = 0;

                var itemg = (PlayerInfoMessage)listbox.Items[listbox.SelectedIndex];
                var item = itemg.login.Substring(keyWordLen);

                var start = text.Substring(0, _foundedIndex);

                input.Text = start + item;
                if (previousItem != null)
                {
                    var t = _foundedIndex + previousItem.login.Length - keyWordLen;

                    // TODO FIX ME
                    // !!!!!!!!!!!!!!!!!!!!!!
                    try
                    {
                        if (t < input.Text.Length)
                            input.Text += text.Substring(t);
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.ToString());
                    }
                }
                else input.Text += text.Substring(_foundedIndex);
                input.CaretIndex = start.Length + item.Length;
            }
            if (e.Key == System.Windows.Input.Key.Up)
            {
                var previousItem = (PlayerInfoMessage)SuggestionListBox.SelectedItem;
                if (selectedIndex > 0)
                    listbox.SelectedIndex--;
                else listbox.SelectedIndex = len;

                var itemg = (PlayerInfoMessage)listbox.Items[listbox.SelectedIndex];
                var item = itemg.login.Substring(keyWordLen);

                var start = text.Substring(0, _foundedIndex);

                input.Text = start + item;
                if (previousItem != null)
                {
                    var t = _foundedIndex + previousItem.login.Length - keyWordLen;

                    // TODO FIX ME
                    try
                    {
                        if (t < input.Text.Length)
                            input.Text += text.Substring(t);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                else input.Text += text.Substring(_foundedIndex);
                input.CaretIndex = start.Length + item.Length;
            }
        }
        private bool ChangedByKey = false;

        private int _foundedIndex;
        private int _keyWordLen;
        private string _previousText = "";
        private void OnMessageInputTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!ChangedByKey) return;
            ChangedByKey = false;

            var input = MessageInput;

            var text = input.Text;
            var len = text.Length;

            var isDeletingAction = _previousText.Length > text.Length;
            if (isDeletingAction)
                if (input.CaretIndex > 0)
                    _foundedIndex--;

            if (len == 0)
            {
                SuggestionListBox.ItemsSource = null;
                VisibilityTest = Visibility.Collapsed;
                return;
            }

            StringBuilder sb = new();

            for (int i = 0; i < len; i++)
            {
                var letter = text[i];

                if (letter == '@')
                {

                    sb.Append(letter);
                    Char innerL = '0';
                    bool gg = false;
                    while (innerL != ' ' && i < len - 1)
                    {
                        i++;
                        innerL = text[i];
                        if (innerL == '@' || innerL == ' ')
                        {
                            if (input.CaretIndex != i)
                                sb.Clear();
                            else gg = true;
                            break;
                        }
                        sb.Append(innerL);
                    }

                    if (sb.Length <= 2)
                    {
                        sb.Clear();
                    }

                    var currentIndex = i + 1;

                    bool test = sb.Length > 2 && input.CaretIndex == currentIndex;

                    if (gg)
                        test = sb.Length > 2;

                    if (test)
                    {
                        var word = sb.ToString().Trim().Substring(1);
                        var suggestions = PlayersService.GetPlayers(word).ToArray();

                        if (suggestions.Length > 0)
                        {
                            if (suggestions.Length == 1)
                            {
                                if (suggestions[0].login.Equals(word, StringComparison.OrdinalIgnoreCase))
                                {
                                    SuggestionListBox.ItemsSource = null;
                                    VisibilityTest = Visibility.Collapsed;
                                    return;
                                }
                            }

                            SuggestionListBox.ItemsSource = suggestions;
                            SuggestionListBox.SelectedIndex = 0;
                            VisibilityTest = Visibility.Visible;

                            var width = MessageInput.ActualWidth;
                            var height = MessageInput.ActualHeight;
                            for (int j = 0; j < width; j++)
                            {
                                var func = MessageInput.GetCharacterIndexFromPoint(new(j, height / 2), false);
                                if (func == i - word.Length)
                                {
                                    Thickness = new Thickness(j, 0, 0, 10);
                                    break;
                                }
                            }

                            var player = suggestions[0];
                            //input.Text = text + player;
                            _previousText = text + player;
                            if (gg) currentIndex--;
                            _foundedIndex = currentIndex;
                            _keyWordLen = word.Length;
                            //input.Select(currentIndex, currentIndex + player.Length);
                            return;
                        }
                        sb.Clear();
                    }

                    //SuggestionListBox.ItemsSource = null;
                    //VisibilityTest = Visibility.Collapsed;
                }
            }
            _previousText = input.Text;
        }
        private void JoinChannelInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var channel = JoinChannelInput.Text;
                if (string.IsNullOrWhiteSpace(channel)) return;
                if (channel[0] != '#')
                    channel = "#" + channel;

                //if (!IrcClient.Connected)
                //{
                //    return;
                //}

                for (int i = 0; i < Channels.Count; i++)
                {
                    if (Channels[i].Name == channel)
                    {
                        // raise something
                        return;
                    }
                }

                //IrcClient.JoinChannel(channel);
                Channels.Add(new() { Name = channel });

                JoinChannelInput.Text = string.Empty;
            }
        }
    }
}
