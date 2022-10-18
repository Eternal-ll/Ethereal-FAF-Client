using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.IRC;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Models.IRC;
using Meziantou.Framework.WPF.Collections;
using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class ChatViewModel : Base.ViewModel
    {
        private readonly LobbyClient LobbyClient;
        private readonly IrcClient IrcClient;
        private readonly PlayersViewModel Players;

        public ChatViewModel(LobbyClient lobbyClient, IrcClient ircClient, PlayersViewModel players)
        {
            JoinChannelCommand = new LambdaCommand(OnJoinChannelCommand);
            LeaveChannelCommand = new LambdaCommand(OnLeaveChannelCommand);

            Channels = new();
            ChannelsSource = new()
            {
                Source = Channels.AsObservable
            };
            ChannelsSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SelectedChannel.Group)));

            Users = new();
            UsersSource = new()
            {
                Source = Users.AsObservable
            };

            History = new();
            HistorySource = new()
            {
                Source = History.AsObservable
            };

            lobbyClient.IrcPasswordReceived += LobbyClient_IrcPasswordReceived;
            lobbyClient.WelcomeDataReceived += LobbyClient_WelcomeDataReceived;
            lobbyClient.SocialDataReceived += LobbyClient_SocialDataReceived;

            ircClient.ChannelMessageReceived += IrcClient_ChannelMessageReceived;
            ircClient.ChannelUsersReceived += IrcClient_ChannelUsersReceived;
            ircClient.UserJoined += IrcClient_UserJoined;
            ircClient.UserLeft += IrcClient_UserLeft;
            ircClient.UserDisconnected += IrcClient_UserDisconnected;
            ircClient.ChannelTopicChangedBy += IrcClient_ChannelTopicChangedBy;

            LobbyClient = lobbyClient;
            IrcClient = ircClient;
            Players = players;
        }

        private void IrcClient_ChannelTopicChangedBy(object sender, (string channel, string topic, string at) e)
        {
            if (GetChannel(e.channel) is GroupChannel group) group.Title = e.topic;
        }

        private void IrcClient_UserDisconnected(object sender, string e)
        {
            foreach (var channel in Channels)
            {
                if (channel is GroupChannel group)
                {
                    group.RemoveUser(e);
                }
                if (channel.IsSelected)
                {
                    Users.Remove(e);
                }
            }
        }

        private void IrcClient_UserLeft(object sender, (string channel, string user) e)
        {
            if (LeftChannel == e.channel) return;
            var channel = (GroupChannel)GetChannel(e.channel);
            channel.RemoveUser(e.user);
            if (!channel.IsSelected) return;
            Users.Remove(e.user);
        }

        private void IrcClient_UserJoined(object sender, (string channel, string user) e)
        {
            var channel = (GroupChannel)GetChannel(e.channel);
            channel.AddUser(e.user);
            if (!channel.IsSelected) return;
            Users.AddRange(e.user);
        }

        private void IrcClient_ChannelUsersReceived(object sender, (string channel, string[] users) e)
        {
            var channel = (GroupChannel)GetChannel(e.channel);
            channel.Users.Clear();
            channel.Users.AddRange(e.users);
            if (!channel.IsSelected) return;
            Users.Clear();
            Users.AddRange(e.users);
        }

        private IrcChannel GetChannel(string channel)
        {
            var channels = Channels;
            for (int i = 0; i < channels.Count; i++)
            {
                if (channels[i].Name == channel) return channels[i];
            }
            IrcChannel newChannel = channel.StartsWith('#') ? new GroupChannel(channel) : new DialogueChannel(channel, null);
            Channels.Add(newChannel);
            return newChannel;
        }

        private void IrcClient_ChannelMessageReceived(object sender, (string channel, string from, string message) e)
        {
            var channel = GetChannel(e.channel);
            var message = new IrcUserMessage(e.message, e.from);
            channel.History.Add(message);
            if (!channel.IsSelected) return;
            History.Add(message);
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

        private readonly ConcurrentObservableCollection<string> Users;
        private readonly CollectionViewSource UsersSource;
        public ICollectionView UsersView => UsersSource.View;


        private readonly ConcurrentObservableCollection<IrcMessage> History;
        private readonly CollectionViewSource HistorySource;
        public ICollectionView HistoryView => HistorySource.View;
        private string Password;

        private void LobbyClient_IrcPasswordReceived(object sender, string e)
        {
            Password = e;
        }
        private void LobbyClient_WelcomeDataReceived(object sender, Welcome e)
        {
            IrcClient.Authorize(e.login, e.id.ToString(), Password);
        }
        private void LobbyClient_SocialDataReceived(object sender, global::FAF.Domain.LobbyServer.SocialData e)
        {
            foreach (var item in e.autojoin)
            {
                IrcClient.Join(item);
            }
        }


        public ICommand JoinChannelCommand { get; }

        private void OnJoinChannelCommand(object arg)
        {
            if (arg is not string channel) return;
            if (string.IsNullOrWhiteSpace(channel)) return;
            IrcClient.Join(channel);
            return;
            IrcClient.Restart();
        }

        private string LeftChannel = "";
        public ICommand LeaveChannelCommand { get; }
        private void OnLeaveChannelCommand(object arg)
        {
            if (arg is not string channel) return;
            if (string.IsNullOrWhiteSpace(channel)) return;
            LeftChannel = channel;
            Channels.Remove(GetChannel(channel));
            IrcClient.Leave(channel);
        }

        public ICommand SendMessageCommand { get; }
        private void OnSendMessageCommand(object arg)
        {
            if (arg is not string text) return;
            if (string.IsNullOrWhiteSpace(text)) return;
            if (SelectedChannel is null) return;
            IrcClient.SendMessage(SelectedChannel.Name, text);  
        }
    }
}
