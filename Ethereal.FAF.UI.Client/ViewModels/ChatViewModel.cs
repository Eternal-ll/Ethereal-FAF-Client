using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.IRC;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.Models.IRC;
using Meziantou.Framework.WPF.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class ChatViewModel : Base.ViewModel
    {
        private readonly IFafPlayersService _fafPlayersService;

        private readonly IConfiguration Configuration;

        private readonly NotificationService NotificationService;
        private readonly SnackbarService SnackbarService;
        private readonly ContentDialogService DialogService;

        private readonly ConcurrentObservableCollection<string> UserChannels = new();
        private readonly HashSet<string> SelfLogins = new();

        private readonly IrcClient IrcClient;

        public ChatViewModel(NotificationService notificationService, IContentDialogService contentDialogService, IConfiguration configuration, ISnackbarService snackbarService,
            IServiceProvider serviceProvider, IFafPlayersService fafPlayersService)
        {
            JoinChannelCommand = new LambdaCommand(OnJoinChannelCommand, CanJoinChannelCommand);
            LeaveChannelCommand = new LambdaCommand(OnLeaveChannelCommand);
            SendMessageCommand = new LambdaCommand(OnSendMessageCommand);
            OpenPrivateCommand = new LambdaCommand(OnOpenPrivateCommand);
            ReconnectCommand = new LambdaCommand(OnReconnectCommand, CanReconnectCommand);
            RenameCommand = new LambdaCommand(OnRenameCommand, CanRenameCommand);

            Channels = new();
            ChannelsSource = new()
            {
                Source = Channels.AsObservable
            };

            Users = new();
            UsersSource = new()
            {
                Source = Users.AsObservable
            };
            UsersSource.Filter += UsersSource_Filter;

            History = new();
            HistorySource = new()
            {
                Source = History.AsObservable
            };

            var ircClient = serviceProvider.GetRequiredService<ServerManager>().GetIrcClient();
            IrcClient = ircClient;
            ircClient.Authorized += IrcClient_Authorized;
            ircClient.ChannelMessageReceived += IrcClient_ChannelMessageReceived;
            ircClient.ChannelUsersReceived += IrcClient_ChannelUsersReceived;
            ircClient.UserJoined += IrcClient_UserJoined;
            ircClient.UserLeft += IrcClient_UserLeft;
            ircClient.UserDisconnected += IrcClient_UserDisconnected;
            ircClient.UserChangedName += IrcClient_UserChangedName;
            ircClient.ChannelTopicUpdated += IrcClient_ChannelTopicUpdated;
            ircClient.ChannelTopicChangedBy += IrcClient_ChannelTopicChangedBy;
            ircClient.NotificationMessageReceived += IrcClient_NotificationMessageReceived;
            NotificationService = notificationService;
            DialogService = (ContentDialogService)contentDialogService;
            Configuration = configuration;
            SnackbarService = (SnackbarService)snackbarService;
            _fafPlayersService = fafPlayersService;
        }

        private void AddServerMessage(string text) => History.Add(new IrcUserMessage(text, new("irc.faforever.com")));
        private void IrcClient_NotificationMessageReceived(object sender, string e)
        {
            AddServerMessage(e);
        }

        #region UsersFilterText
        private string _UsersFilterText;
        public string UsersFilterText { get => _UsersFilterText; set
            {
                if (Set(ref _UsersFilterText, value))
                {
                    UsersView.Refresh();
                }
            } }
        #endregion

        static bool IsAccepted(IrcUser user, params string[] words)
        {
            var accepted = false;
            foreach (var word in words)
            {
                if (string.IsNullOrWhiteSpace(word)) continue;
                if (user.Player is null)
                {
                    if (user.Name.Contains(word, StringComparison.OrdinalIgnoreCase)) return true;
                }
                else
                {
                    var player = user.Player;
                    if (word.Length == 2) accepted = player.Country.Contains(word, StringComparison.OrdinalIgnoreCase);
                    else accepted = player.LoginWithClan.Contains(word, StringComparison.OrdinalIgnoreCase);
                }
            }
            return accepted;
        }

        private void UsersSource_Filter(object sender, FilterEventArgs e)
        {
            var user = (IrcUser)e.Item;
            e.Accepted = false;
            if (!string.IsNullOrWhiteSpace(UsersFilterText))
            {
                if (!IsAccepted(user, UsersFilterText.Split())) return;
            }
            e.Accepted = true;
        }

        public ConcurrentObservableCollection<string> SuggestList = new();

        #region ChannelText
        private string _ChannelText;
        public string ChannelText
        {
            get => _ChannelText;
            set => Set(ref _ChannelText, value);
        }
        #endregion

        private void IrcClient_Authorized(object sender, bool e)
        {
            if (e)
            {
                //foreach (var item in SocialData.autojoin)
                //{
                //    IrcClient.Join(item);
                //}
                //foreach (var item in SocialData.channels)
                //{
                //    IrcClient.Join(item);
                //}
                var autojoin = Configuration.GetSection($"Irc:UserChannels")
                    .Get<string[]>()?
                    .Where(c => !string.IsNullOrWhiteSpace(c));
                if (autojoin is null) return;
                foreach (var channel in autojoin)
                {
                    if (string.IsNullOrWhiteSpace(channel)) continue;
                    IrcClient.Join(channel.StartsWith('#') ? channel : '#' + channel);
                    UserChannels.Add(channel);
                }
                return;
            }
            if (SelectedChannel is not null)
            {
                SelectedChannel = null;
                Users.Clear();
                History.Clear();
                SuggestList.Clear();
            }
            foreach (var channel in Channels)
            {
                Channels.Remove(channel);
            }
            GC.Collect();
        }

        private void IrcClient_ChannelTopicUpdated(object sender, (string channel, string topic, string by) e)
        {
            if (GetChannel(e.channel) is GroupChannel group)
            {
                group.Title = e.topic;
                group.TopicChangedBy = e.by;
                group.TopicChangedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        private void IrcClient_ChannelTopicChangedBy(object sender, (string channel, string user, string at) e)
        {
            if (GetChannel(e.channel) is GroupChannel group)
            {
                group.TopicChangedBy = e.user;
                group.TopicChangedAt = long.Parse(e.at);
            }
        }

        private void IrcClient_UserChangedName(object sender, (string user, string to) e)
        {
            foreach (var channel in Channels)
            {
                if (channel is GroupChannel group)
                {
                    group.ReplaceUser(e.user, e.to);
                }
                if (IsChannelSelected(channel.Name))
                {
                    var user = Users.FirstOrDefault(u => u.Name == e.user);
                    if (user is not null) user.Name = e.to;
                }
                if (channel.Name == e.user)
                {
                    if (channel is DialogueChannel dialogue)
                    {
                        dialogue.Name = e.to;
                        dialogue.Receiver.Name = e.to;
                    }
                }
            }
            SuggestList.Remove(e.user);
            SuggestList.Add(e.to);
        }

        private void IrcClient_UserDisconnected(object sender, (string user, string id) e)
        {
            foreach (var channel in Channels)
            {
                if (channel is GroupChannel group)
                {
                    group.RemoveUser(e.user);
                }
                if (channel.IsSelected)
                {
                    if (_fafPlayersService.TryGetPlayer(e.user, out var player))
                    {
                        player.IrcUsername = null;
                    }
                    Users.Remove(Users.FirstOrDefault(u => u.Name == e.user));
                }
            }
            SuggestList.Remove(e.user);
        }

        private void IrcClient_UserLeft(object sender, (string channel, string user) e)
        {
            if (LeftChannel == e.channel) return;
            var channel = (GroupChannel)GetChannel(e.channel);
            if (e.user.Trim('@') == IrcClient.User)
            {
                Channels.Remove(channel);
            }
            channel.RemoveUser(e.user);
            if (!IsChannelSelected(e.channel)) return;
            Users.Remove(Users.FirstOrDefault(u => u.Name == e.user));
        }

        private void IrcClient_UserJoined(object sender, (string channel, string user) e)
        {
            var channel = (GroupChannel)GetChannel(e.channel);
            IrcUser user = new(e.user);
            if (_fafPlayersService.TryGetPlayer(e.user, out var player))
            {
                user.SetPlayer(player);
            }
            channel.AddUser(user);

            if (!SuggestList.Any(u => u == e.user)) SuggestList.Add(e.user);


            if (!IsChannelSelected(e.channel)) return;
            Users.Add(user);
        }

        private void IrcClient_ChannelUsersReceived(object sender, (string channel, string[] users) e)
        {
            if (string.IsNullOrWhiteSpace(e.channel)) return;
            var channel = (GroupChannel)GetChannel(e.channel);
            channel.Users.Clear();

            var users = e.users.Select(u => _fafPlayersService.TryGetPlayer(u.Trim('@').Trim('`'), out var player) ?
            new IrcUser(u, player) : new IrcUser(u));


            channel.Users.AddRange(users);

            foreach (var user in e.users)
            {
                if (SuggestList.IndexOf(user) == -1) SuggestList.Add(user);
            }

            if (!IsChannelSelected(e.channel)) return;
            Users.Clear();
            Users.AddRange(users);
        }

        private void IrcClient_ChannelMessageReceived(object sender, (string channel, string from, string message) e)
        {
            var channel = GetChannel(e.channel);
            var user = new IrcUser(e.from);
            if (!(channel is GroupChannel group && group.TryGetUser(e.from, out user)))
            {
                if (channel is DialogueChannel dialogue && dialogue.Receiver.Name == e.from)
                {
                    user = dialogue.Receiver;
                }
                else
                {
                    user = new IrcUser(e.from);
                    if (_fafPlayersService.TryGetPlayer(e.from.Trim('@').Trim('`'), out var player)) user.SetPlayer(player);
                }
            }
            var message = new IrcUserMessage(e.message, user);
            channel.History.Add(message);
            if (!IsChannelSelected(e.channel)) return;
            History.Add(message);
        }

        private IrcChannel GetChannel(string channel)
        {
            var channels = Channels;
            for (int i = 0; i < channels.Count; i++)
            {
                var cachedChannel = channels[i];
                if (cachedChannel.Name == channel) return cachedChannel;
            }
            IrcChannel newChannel = channel.StartsWith('#') ? new GroupChannel(channel) :
                _fafPlayersService.TryGetPlayer(channel, out var player) ? new DialogueChannel(channel, player) :
                new DialogueChannel(channel);
            Channels.Add(newChannel);
            SelectedChannel = newChannel;
            return newChannel;
        }

        #region IrcChannel
        private IrcChannel _IrcChannel;
        public IrcChannel SelectedChannel
        {
            get => _IrcChannel;
            set
            {
                if (Set(ref _IrcChannel, value))
                {
                    History.Clear();
                    Users.Clear();
                    if (value is null)
                    {
                        return;
                    }
                    History.AddRange(value.History);
                    if(value is GroupChannel group)
                    {
                        Users.AddRange(group.Users);
                    }
                }
            }
        }
        #endregion

        private readonly ConcurrentObservableCollection<IrcChannel> Channels;
        private readonly CollectionViewSource ChannelsSource;
        public ICollectionView ChannelsView => ChannelsSource.View;

        private readonly ConcurrentObservableCollection<IrcUser> Users;
        private readonly CollectionViewSource UsersSource;
        public ICollectionView UsersView => UsersSource.View;


        private readonly ConcurrentObservableCollection<IrcMessage> History;
        private readonly CollectionViewSource HistorySource;
        public ICollectionView HistoryView => HistorySource.View;

        public async Task<(string channel, int users)[]> GetChannelsAsync()
        {
            var ircClient = IrcClient;
            if (ircClient is null) return null;
            if (!ircClient.IsConnected) return null;
            (string channel, int users)[] channels = null;
            ircClient.List();
            ircClient.AvailableChannels += (s, e) => channels = e;
            var task = Task.Run(async () =>
            {
                while (channels is null)
                {
                    await Task.Delay(50);
                }
            });
            await task.WaitAsync(TimeSpan.FromSeconds(3));
            return channels;
        }

        private bool IsChannelSelected(string channel) => SelectedChannel.Name == channel;

        public ICommand JoinChannelCommand { get; }
        private bool CanJoinChannelCommand(object arg) => arg is string channel && 
            !string.IsNullOrWhiteSpace(channel) && !Channels.Any(c => c.Name == channel);
        private void OnJoinChannelCommand(object arg)
        {
            var ircClient = IrcClient;
            if (!ircClient.IsConnected) return;
            var channel = (string)arg;
            if (channel.StartsWith('#'))
            {
                ircClient.Join(channel);
                AddUserChannel(channel);
                return;
            }
            OpenPrivateCommand.Execute(channel);
        }

        private string LeftChannel = null;
        public ICommand LeaveChannelCommand { get; }
        private void OnLeaveChannelCommand(object arg)
        {
            if (arg is not string channel) return;
            if (string.IsNullOrWhiteSpace(channel)) return;
            Channels.Remove(GetChannel(channel));
            if (!channel.StartsWith('#')) return;
            LeftChannel = channel;
            IrcClient.Leave(channel);
            DeleteUserChannel(channel);
        }

        public ICommand SendMessageCommand { get; }
        private void OnSendMessageCommand(object arg)
        {
            var ircClient = IrcClient;
            if (!ircClient.IsConnected) return;
            if (arg is not string text) return;
            if (string.IsNullOrWhiteSpace(text)) return;
            if (text.StartsWith('/'))
            {
                ircClient.SendAsync(text[1..]);
                return;
            }
            
            if (SelectedChannel is null) return;
            IrcClient.SendMessage(SelectedChannel.Name, text);
        }

        public ICommand OpenPrivateCommand { get; }
        public void OnOpenPrivateCommand(object arg)
        {
            if (arg is not string user) return;
            SelectedChannel = GetChannel(user);
        }

        public ICommand ReconnectCommand { get; }
        private bool CanReconnectCommand(object arg) => true;
        private void OnReconnectCommand(object arg)
        {
            IrcClient.Restart();
        }

        public ICommand RenameCommand { get; }
        private bool CanRenameCommand(object arg) => IrcClient.IsConnected is true;
        private async void OnRenameCommand(object arg)
        {
            var textbox = new Wpf.Ui.Controls.TextBox();
            textbox.PlaceholderText = "New IRC username";
            textbox.Margin = new System.Windows.Thickness(0, 10, 0, 0);
            //var dialog = DialogService.GetContentPresenter();
            //dialog.Content = textbox;
            //dialog.ButtonLeftName = "Rename";
            //dialog.ButtonRightName = "Cancel";
            //await dialog.ShowAndWaitAsync("Rename IRC name", null);
            //dialog.Hide();
            GC.Collect();
            if (string.IsNullOrWhiteSpace(textbox.Text)) return;
            IrcClient.Rename(textbox.Text);
        }

        private void AddUserChannel(string channel)
        {
            if (UserChannels.Contains(channel)) return;
            UserChannels.Add(new(channel));
            UserSettings.Update($"Irc:UserChannels", UserChannels.ToArray());
        }
        private void DeleteUserChannel(string channel)
        {
            if (UserChannels.Remove(channel)) 
                UserSettings.Update($"Irc:UserChannels", UserChannels.ToArray());
        }
    }
}
