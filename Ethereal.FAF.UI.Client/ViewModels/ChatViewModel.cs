using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.IRC;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.Models.IRC;
using FAF.Domain.LobbyServer;
using Meziantou.Framework.WPF.Collections;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class ServerIrcChannel
    {
        public ServerIrcChannel(ServerManager serverManager, string ircChannel)
        {
            ServerManager = serverManager;
            IrcChannel = ircChannel;
        }
        public ServerManager ServerManager { get; private set; }
        public string IrcChannel { get; }
    }
    public sealed class ChatViewModel : Base.ViewModel
    {
        private readonly PlayersViewModel Players;

        private readonly IConfiguration Configuration;

        private readonly NotificationService NotificationService;
        private readonly SnackbarService SnackbarService;
        private readonly DialogService DialogService;

        private readonly ConcurrentObservableCollection<ServerIrcChannel> UserChannels = new();
        private readonly HashSet<string> SelfLogins = new();

        public ServersManagement ServersManagement { get; }

        public ChatViewModel(
            ServersManagement serversManagement,
            PlayersViewModel players, NotificationService notificationService, DialogService dialogService, IConfiguration configuration, SnackbarService snackbarService)
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
            ChannelsSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SelectedChannel.ServerManager)));

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

            //lobbyClient.IrcPasswordReceived += LobbyClient_IrcPasswordReceived;
            //lobbyClient.WelcomeDataReceived += LobbyClient_WelcomeDataReceived;
            //lobbyClient.SocialDataReceived += LobbyClient_SocialDataReceived;

            serversManagement.ServerManagerAdded += (s, manager) =>
            {
                SelectedServerManager = manager;
                var ircClient = manager.GetIrcClient();
                manager.IrcAuthorized = (s, authorized) =>
                {
                    IrcClient_Authorized(s, authorized, manager);
                };
                manager.IrcChannelMessageReceived = (s, e) =>
                {
                    IrcClient_ChannelMessageReceived(s, e, manager);
                };

                manager.IrcChannelUsersReceived = (s, e) =>
                {
                    IrcClient_ChannelUsersReceived(s, e, manager);
                };
                manager.IrcUserJoined = (s, e) =>
                {
                    IrcClient_UserJoined(s, e, manager);
                };
                manager.IrcUserLeft = (s, e) =>
                {
                    IrcClient_UserLeft(s, e, manager);
                };
                manager.IrcUserDisconnected = (s, e) =>
                {
                    IrcClient_UserDisconnected(s, e, manager);
                };
                manager.IrcUserChangedName = (s, e) =>
                {
                    IrcClient_UserChangedName(s, e, manager);
                };
                manager.IrcChannelTopicUpdated = (s, e) =>
                {
                    IrcClient_ChannelTopicUpdated(s, e, manager);
                };
                manager.IrcChannelTopicChangedBy = (s, e) =>
                {
                    IrcClient_ChannelTopicChangedBy(s, e, manager);
                };


                ircClient.Authorized += manager.IrcAuthorized;
                ircClient.ChannelMessageReceived += manager.IrcChannelMessageReceived;
                ircClient.ChannelUsersReceived += manager.IrcChannelUsersReceived;
                ircClient.UserJoined += manager.IrcUserJoined;
                ircClient.UserLeft += manager.IrcUserLeft;
                ircClient.UserDisconnected += manager.IrcUserDisconnected;
                ircClient.UserChangedName += manager.IrcUserChangedName;
                ircClient.ChannelTopicUpdated += manager.IrcChannelTopicUpdated;
                ircClient.ChannelTopicChangedBy += manager.IrcChannelTopicChangedBy;
                ircClient.NotificationMessageReceived += IrcClient_NotificationMessageReceived;
            };
            Players = players;
            NotificationService = notificationService;
            DialogService = dialogService;
            Configuration = configuration;
            ServersManagement = serversManagement;
            SnackbarService = snackbarService;
        }

        #region SelectedServerManager
        private ServerManager _SelectedServerManager;
        public ServerManager SelectedServerManager
        {
            get => _SelectedServerManager;
            set => Set(ref _SelectedServerManager, value);
        }
        #endregion

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

        private void IrcClient_Authorized(object sender, bool e, ServerManager manager)
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
                foreach (var channel in UserChannels)
                {
                    if (channel.ServerManager.Equals(manager))
                    {
                        UserChannels.Remove(channel);
                    }
                }
                var autojoin = Configuration.GetSection($"Servers:{manager.Server.ShortName}:IRC:UserChannels")
                    .Get<string[]>()?
                    .Where(c => !string.IsNullOrWhiteSpace(c));
                if (autojoin is null) return;
                foreach (var channel in autojoin)
                {
                    if (string.IsNullOrWhiteSpace(channel)) continue;
                    manager.GetIrcClient().Join(channel.StartsWith('#') ? channel : '#' + channel);
                    UserChannels.Add(new(manager, channel));
                }
                return;
            }
            if (SelectedChannel is not null && SelectedChannel.ServerManager.Equals(manager))
            {
                SelectedChannel = null;
                Users.Clear();
                History.Clear();
                SuggestList.Clear();
            }
            foreach (var channel in Channels)
            {
                if (channel.ServerManager.Equals(manager))
                {
                    Channels.Remove(channel);
                }
            }
            GC.Collect();
        }

        private void IrcClient_ChannelTopicUpdated(object sender, (string channel, string topic, string by) e, ServerManager manager)
        {
            if (GetChannel(e.channel, manager) is GroupChannel group)
            {
                group.Title = e.topic;
                group.TopicChangedBy = e.by;
                group.TopicChangedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        private void IrcClient_ChannelTopicChangedBy(object sender, (string channel, string user, string at) e, ServerManager manager)
        {
            if (GetChannel(e.channel, manager) is GroupChannel group)
            {
                group.TopicChangedBy = e.user;
                group.TopicChangedAt = long.Parse(e.at);
            }
        }

        private void IrcClient_UserChangedName(object sender, (string user, string to) e, ServerManager manager)
        {
            foreach (var channel in Channels)
            {
                if (!channel.ServerManager.Equals(manager)) continue;
                if (channel is GroupChannel group)
                {
                    group.ReplaceUser(e.user, e.to);
                }
                if (IsChannelSelected(channel.Name, manager))
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

        private void IrcClient_UserDisconnected(object sender, (string user, string id) e, ServerManager manager)
        {
            foreach (var channel in Channels)
            {
                if (!channel.ServerManager.Equals(manager)) continue;
                if (channel is GroupChannel group)
                {
                    group.RemoveUser(e.user);
                }
                if (channel.IsSelected)
                {
                    if (Players.TryGetPlayer(e.user, manager, out var player))
                    {
                        player.IrcUsername = null;
                    }
                    Users.Remove(Users.FirstOrDefault(u => u.Name == e.user));
                }
            }
            SuggestList.Remove(e.user);
        }

        private void IrcClient_UserLeft(object sender, (string channel, string user) e, ServerManager manager)
        {
            if (LeftChannel == e.channel) return;
            var channel = (GroupChannel)GetChannel(e.channel, manager);
            var ircClient = manager.GetIrcClient();
            if (e.user.Trim('@') == ircClient.User)
            {
                Channels.Remove(channel);
            }
            channel.RemoveUser(e.user);
            if (!IsChannelSelected(e.channel, manager)) return;
            Users.Remove(Users.FirstOrDefault(u => u.Name == e.user));
        }

        private void IrcClient_UserJoined(object sender, (string channel, string user) e, ServerManager manager)
        {
            var channel = (GroupChannel)GetChannel(e.channel, manager);
            IrcUser user = new(e.user);
            if (Players.TryGetPlayer(e.user, manager, out var player))
            {
                user.SetPlayer(player);
            }
            channel.AddUser(user);

            if (!SuggestList.Any(u => u == e.user)) SuggestList.Add(e.user);


            if (!IsChannelSelected(e.channel, manager)) return;
            Users.Add(user);
        }

        private void IrcClient_ChannelUsersReceived(object sender, (string channel, string[] users) e, ServerManager manager)
        {
            if (string.IsNullOrWhiteSpace(e.channel)) return;
            var channel = (GroupChannel)GetChannel(e.channel, manager);
            channel.Users.Clear();

            var users = e.users.Select(u =>
            Players.TryGetPlayer(u.Trim('@').Trim('`'), manager, out var player) ?
            new IrcUser(u, player) :
            new IrcUser(u));


            channel.Users.AddRange(users);

            foreach (var user in e.users)
            {
                if (SuggestList.IndexOf(user) == -1) SuggestList.Add(user);
            }

            if (!IsChannelSelected(e.channel, manager)) return;
            Users.Clear();
            Users.AddRange(users);
        }

        private void IrcClient_ChannelMessageReceived(object sender, (string channel, string from, string message) e, ServerManager manager)
        {
            var channel = GetChannel(e.channel, manager);
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
                    if (Players.TryGetPlayer(e.from.Trim('@').Trim('`'), manager, out var player)) user.SetPlayer(player);
                }
            }
            var message = new IrcUserMessage(e.message, user);
            channel.History.Add(message);
            if (!IsChannelSelected(e.channel, manager)) return;
            History.Add(message);
        }

        private IrcChannel GetChannel(string channel, ServerManager manager)
        {
            var channels = Channels;
            for (int i = 0; i < channels.Count; i++)
            {
                var cachedChannel = channels[i];
                if (cachedChannel.Name == channel && cachedChannel.ServerManager.Equals(manager)) return cachedChannel;
            }
            IrcChannel newChannel = channel.StartsWith('#') ? new GroupChannel(channel, manager) :
                Players.TryGetPlayer(channel, manager, out var player) ? new DialogueChannel(channel, player, manager) :
                new DialogueChannel(channel, manager);
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
        private string Password;

        public async Task<(string channel, int users)[]> GetChannelsAsync()
        {
            var ircClient = SelectedServerManager?.GetIrcClient();
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

        private bool IsChannelSelected(string channel, ServerManager manager) =>
            SelectedChannel.Name == channel &&
            SelectedChannel.ServerManager.Equals(manager);

        public ICommand JoinChannelCommand { get; }
        private bool CanJoinChannelCommand(object arg) => arg is string channel && 
            !string.IsNullOrWhiteSpace(channel) && !Channels.Any(c => c.Name == channel);
        private void OnJoinChannelCommand(object arg)
        {
            var ircClient = SelectedServerManager?.GetIrcClient();
            if (ircClient is null)
            {
                return;
            }
            if (!ircClient.IsConnected) return;
            var channel = (string)arg;
            if (channel.StartsWith('#'))
            {
                ircClient.Join(channel);
                AddUserChannel(channel, SelectedServerManager);
                return;
            }
            OpenPrivateCommand.Execute(channel);
        }

        private string LeftChannel = null;
        public ICommand LeaveChannelCommand { get; }
        private void OnLeaveChannelCommand(object arg)
        {
            //if (arg is not string channel) return;
            //if (string.IsNullOrWhiteSpace(channel)) return;
            //Channels.Remove(GetChannel(channel));
            //if (!channel.StartsWith('#')) return;
            //LeftChannel = channel;
            //IrcClient.Leave(channel);
            //DeleteUserChannel(channel);
        }

        public ICommand SendMessageCommand { get; }
        private void OnSendMessageCommand(object arg)
        {
            var ircClient = SelectedServerManager?.GetIrcClient();
            if (ircClient is null)
            {
                return;
            }
            if (!ircClient.IsConnected) return;
            if (arg is not string text) return;
            if (string.IsNullOrWhiteSpace(text)) return;
            if (text.StartsWith('/'))
            {
                ircClient.SendAsync(text[1..]);
                return;
            }
            
            if (SelectedChannel is null) return;
            SelectedChannel.ServerManager
                .GetIrcClient()
                .SendMessage(SelectedChannel.Name, text);
            //IrcClient.SendAsync(text);
        }

        public ICommand OpenPrivateCommand { get; }
        public void OnOpenPrivateCommand(object arg)
        {
            if (arg is not string user) return;
            //var channel = GetChannel(user);
            //SelectedChannel = channel;
        }

        public ICommand ReconnectCommand { get; }
        private bool CanReconnectCommand(object arg) => true;
        private void OnReconnectCommand(object arg)
        {
            if (SelectedServerManager is null)
            {
                SnackbarService.Timeout = 7000;
                SnackbarService.Show("Warning", "I am multiverse client, but i can`t decide which universe to choose for you.",
                     Wpf.Ui.Common.SymbolRegular.Warning20);
                return;
            }
            SelectedServerManager?.GetIrcClient().Restart();
        }

        public ICommand RenameCommand { get; }
        private bool CanRenameCommand(object arg) => SelectedServerManager?.GetIrcClient().IsConnected is true;
        private async void OnRenameCommand(object arg)
        {
            var textbox = new Wpf.Ui.Controls.TextBox();
            textbox.PlaceholderText = "New IRC username";
            textbox.Margin = new System.Windows.Thickness(0, 10, 0, 0);
            var dialog = DialogService.GetDialogControl();
            dialog.Content = textbox;
            dialog.ButtonLeftName = "Rename";
            dialog.ButtonRightName = "Cancel";
            await dialog.ShowAndWaitAsync("Rename IRC name", null);
            dialog.Hide();
            GC.Collect();
            if (string.IsNullOrWhiteSpace(textbox.Text)) return;
            SelectedServerManager?
                .GetIrcClient()
                .Rename(textbox.Text);
        }

        private void AddUserChannel(string channel, ServerManager manager)
        {
            if (UserChannels.Any(c => c.IrcChannel == channel && c.ServerManager.Equals(manager))) return;
            UserChannels.Add(new(manager, channel));
            UserSettings.Update($"Servers:{manager.Server.ShortName}:IRC:UserChannels", UserChannels.Select(s=>s.IrcChannel));
        }
        private void DeleteUserChannel(string channel, ServerManager manager)
        {
            if (UserChannels.Remove(UserChannels.FirstOrDefault(c => c.IrcChannel == channel && c.ServerManager.Equals(manager)))) 
                UserSettings.Update($"Servers:{manager.Server.ShortName}:IRC:UserChannels", UserChannels.Select(s => s.IrcChannel));
        }
    }
}
