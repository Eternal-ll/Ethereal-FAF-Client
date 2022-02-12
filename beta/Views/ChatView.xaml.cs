using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Properties;
using beta.ViewModels.Base;
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
        #region Properties

        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        private readonly IrcClient IrcClient;
        private readonly IPlayersService PlayersService;
        
        private readonly ObservableCollection<Channel> _Channels = new();
        public ObservableCollection<Channel> Channels => _Channels;

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

            DataContext = this;

            Channels.Add(new() { Name = "#server" });
            SelectedChannel = Channels[0];

            App.Current.MainWindow.Closing += MainWindow_Closing;

            MessageInput.TextChanged += OnMessageInputTextChanged;
            MessageInput.PreviewKeyDown += OnMessageInputPreviewKeyDown;

            return;

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

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (IrcClient.Connected)
            IrcClient.Disconnect();
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
            if (e.Key == System.Windows.Input.Key.Up)
            {
                string previousItem = (string)SuggestionListBox.SelectedItem;
                if (selectedIndex > 0)
                    listbox.SelectedIndex--;
                else listbox.SelectedIndex = len;

                var item = (string)listbox.Items[listbox.SelectedIndex];
                item = item.Substring(keyWordLen);

                var start = text.Substring(0, _foundedIndex);

                input.Text = start + item;
                if (previousItem != null)
                {
                    var t = _foundedIndex + previousItem.Length - keyWordLen;
                    if (t < input.Text.Length)
                        input.Text += text.Substring(t);
                }
                else input.Text += text.Substring(_foundedIndex);
                input.CaretIndex = start.Length + item.Length;
            }
        }
        private bool ChangedByKey = false;

        private int _foundedIndex;
        private int _keyWordLen;
        private IEnumerable<string> Test = new List<string>()
        {
            "Test",
            "Tes32t",
            "Tesert",
            "Testre",
            "Tesgrt",
            "Tefst",
        };
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
            //return;

            //bool foundD = false;
            //for (int i = 0; i < len; i++)
            //{
            //    var letter = text[i];

            //    if (foundD)
            //    {
            //        if (letter != ' ' && i != len - 1)
            //        {
            //            sb.Append(letter);
            //            continue;
            //        }
            //        foundD = false;

            //        if (letter != ' ' && i == len - 1)
            //        {
            //            sb.Append(letter);
            //        }

            //        var keyWord = sb.ToString();

            //        if (keyWord.Length == 0)
            //        {
            //            continue;
            //        }

            //        if (input.CaretIndex > i + 1)
            //        {
            //            sb.Clear();
            //            foundD = false;
            //            continue;
            //        }

            //        var suggestions = Test.Where(x => x.StartsWith(keyWord, StringComparison.OrdinalIgnoreCase)).ToArray();

            //        if (suggestions.Length == 1 && suggestions[0] == keyWord)
            //        {
            //            SuggestionListBox.ItemsSource = null;
            //            VisibilityTest = Visibility.Collapsed;

            //            sb.Clear();
            //            continue;
            //        }

            //        if (suggestions.Length > 0)
            //        {

            //            VisibilityTest = Visibility.Visible;
            //            var width = MessageInput.ActualWidth;
            //            var height = MessageInput.ActualHeight;
            //            for (int j = 0; j < width; j++)
            //            {
            //                var func = MessageInput.GetCharacterIndexFromPoint(new(j, height / 2), false);
            //                if (func == i - keyWord.Length)
            //                {
            //                    Thickness = new Thickness(j, 0, 0, 10);
            //                    break;
            //                }
            //            }

            //            SuggestionListBox.ItemsSource = suggestions;
            //            SuggestionListBox.SelectedIndex = 0;

            //            _foundedIndex = i;
            //            _keyWordLen = keyWord.Length;
            //            return;
            //        }
            //        else
            //        {
            //            VisibilityTest = Visibility.Collapsed;
            //            SuggestionListBox.ItemsSource = null;
            //            sb.Clear();
            //            continue;
            //        }


            //        sb.Clear();
            //        foundD = false;
            //    }

            //    // return if there is nothing interesting
            //    if (i == len - 1)
            //    {
            //        SuggestionListBox.ItemsSource = null;
            //        VisibilityTest = Visibility.Collapsed;
            //        return;
            //    }

            //    if (letter == '@')
            //    {
            //        foundD = true;
            //    }
            //}
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
                var playerInfo = PlayersService.GetPlayer(e.UserList[i]);
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

            var login = "@" + Settings.Default.PlayerNick;

            for (int i = 0; i < Channels.Count; i++)
            {
                var channel = Channels[i];
                if (channel.Name == e.Channel)
                {
                    var len = channel.History.Count - 1;

                    if (len >= 0)
                    {
                        var previousNick = channel.History[len].From;
                        while (previousNick == null && len > 0)
                        {
                            len--;
                            previousNick = channel.History[len].From;
                        }

                        if (previousNick == e.From)
                            e.From = null;
                    }
                    channel.History.Add(new()
                    {
                        From = e.From,
                        Message = e.Message,
                        HasMention = e.Message.Contains(login, StringComparison.OrdinalIgnoreCase)
                    }) ;
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
        private void JoinChannelInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var channel = JoinChannelInput.Text;
                if (string.IsNullOrWhiteSpace(channel)) return;
                if (channel[0] != '#')
                    channel = "#" + channel;

                if (!IrcClient.Connected)
                {
                    return;
                }

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
}
