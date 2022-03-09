using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.IRC;
using beta.ViewModels.Base;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace beta.Infrastructure.Services
{
    internal class IrcService : ViewModel, IIrcService
    {
        #region Events
        public event EventHandler<EventArgs<ManagedTcpClientState>> StateChanged;
        public event EventHandler<bool> IrcConnected;

        /// <summary>
        /// User connected to IRC server
        /// </summary>
        public event EventHandler<EventArgs<string>> UserConnected;
        /// <summary>
        /// User disconnected from IRC server
        /// </summary>
        public event EventHandler<EventArgs<string>> UserDisconnected;

        /// <summary>
        /// User joined to specific channel
        /// </summary>
        public event EventHandler<EventArgs<IrcUserJoin>> UserJoined;
        /// <summary>
        /// User left from specific channel
        /// </summary>
        public event EventHandler<EventArgs<IrcUserLeft>> UserLeft;

        public event EventHandler<EventArgs<IrcUserChangedName>> UserChangedName;

        /// <summary>
        /// Private message received from specific user
        /// </summary>
        public event EventHandler<EventArgs<IrcPrivateMessage>> PrivateMessageReceived;
        /// <summary>
        /// Channel message received from specific channel
        /// </summary>
        public event EventHandler<EventArgs<IrcChannelMessage>> ChannedMessageReceived;

        /// <summary>
        /// Specific channel topic updated
        /// </summary>
        public event EventHandler<EventArgs<IrcChannelTopicUpdated>> ChannelTopicUpdated;

        /// <summary>
        /// Specific channel topic changed by specific user
        /// </summary>
        public event EventHandler<EventArgs<IrcChannelTopicChangedBy>> ChannelTopicChangedBy;
        
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<EventArgs<IrcChannelUsers>> ChannelUsersReceived;

        #endregion

        #region Properties

        #region Private

        private ManagedTcpClient ManagedTcpClient;

        private string Nick;
        private string Password;
        #endregion

        #region Public

        #region ConnectionState
        private ManagedTcpClientState _TCPConnectionState;
        public ManagedTcpClientState TCPConnectionState
        {
            get => _TCPConnectionState;
            set => Set(ref _TCPConnectionState, value);
        }
        #endregion

        #region IsIRCConnected
        private bool _IsIRCConnected;
        public bool IsIRCConnected
        {
            get => _IsIRCConnected;
            set
            {
                if (Set(ref _IsIRCConnected, value))
                {
                    OnIrcConnected(false);
                }
            }
        }
        #endregion
        #endregion

        #endregion

        #region Methods

        #region Public
        public void Authorize(string nickname, string password)
        {
            Nick = nickname;
            Password = password;
            Connect(null, 0);
        }

        private void ManagedTcpClient_StateChanged(object sender, ManagedTcpClientState e)
        {
            if (sender is ManagedTcpClient client)
            {
                client.StateChanged -= ManagedTcpClient_StateChanged;
                if (e == ManagedTcpClientState.Connected)
                {
                    client.DataReceived += ManagedTcpClient_DataReceived;
#if DEBUG
                    App.DebugWindow.LOGIRC($"Connected to IRC Server");
                    App.DebugWindow.LOGIRC($"Sending authorization information...");
#endif
                    Authorize();
                }
            }
        }
        private void Authorize()
        {
            Send(IrcCommands.Pass(Password));
            Send(IrcCommands.Nickname(Nick));
            Send(IrcCommands.UserInfo(Nick, Nick));
        }
        public void Connect(string host, int port)
        {
            if (ManagedTcpClient != null)
            {
                ManagedTcpClient.Dispose();
                ManagedTcpClient = null;
            }

            ManagedTcpClient = new()
            {
                ThreadName = "IRC TCP Service"
            };
            ManagedTcpClient.StateChanged += ManagedTcpClient_StateChanged;
        }

        public void Join(string channel, string key = null) => Send(IrcCommands.Join(channel, key));

        public void Leave(string channel) => Send(IrcCommands.Leave(channel));

        public void Leave(string[] channels)
        {
            throw new NotImplementedException();
        }

        public void LeaveAll() => Send(IrcCommands.LeaveAllJoinedChannels());

        public void Ping() => Send(IrcCommands.Ping());

        public void Quit(string reason = null) => Send(IrcCommands.Quit(reason));

        public void SendMessage(string channelOrUser, string message) => Send(IrcCommands.Message(channelOrUser, message));

        public void SetTopic(string channel, string topic = null) => Send(IrcCommands.Topic(channel, topic));

        public void SendInvite(string user, string channel) => Send(IrcCommands.Invite(user, channel));

        #endregion

        #region Private

        private void Send(string message)
        {
            ManagedTcpClient?.Write(message + '\r');
            if (message.StartsWith("QUIT"))
            {
                ManagedTcpClient.Dispose();
                ManagedTcpClient = null;
                IsIRCConnected = false;
#if DEBUG
                App.DebugWindow.LOGIRC($"Disconnected from IRC Server");
#endif
            }
#if DEBUG
            App.DebugWindow.LOGIRC($"You sent: {message}");
#endif
        }

        public void Test()
        {
            //Send(IrcCommands.Join("#aeolus"));
            Send(IrcCommands.Quit());
            //Connect(null, 0);
            //Authorize();
            //Send(IrcCommands.Message("#aeolus", "Check check"));
            //Send(IrcCommands.Join("#test"));
            //Send(IrcCommands.Nickname(Nick + "1"));
            //Send(IrcCommands.Nickname(Nick));
            //Send("INVITE MarcSpector #aeolus1");
            //Send(IrcCommands.Leave("#aeolus1"));
            //Send(IrcCommands.Join("#aeolus2", "test")); 
            //Send("MODE #aeolus2");
            //Send(IrcCommands.Topic("#aeolus1", "test"));
            //Send("KICK #aeolus1 Eternal-");
            //Send(IrcCommands.List());
        }

        private void ManagedTcpClient_DataReceived(object sender, string data)
        {
            // split the data into parts
            string[] ircData = data.Split(' ');

            // if the message starts with PING we must PONG back
            if (ircData[0] == "PING")
            {
                Send(IrcCommands.Pong(ircData[1][1..^1]));
                return;
            }

            var ircCommand = ircData[1];

            StringBuilder sb = new();

            string from = null;
            var index = data.IndexOf('!');
            if (index != -1)
            {
                from = data[1..index];
            }
            else
            {
                from = data[1..data.IndexOf(' ')];
            }

            // re-act according to the IRC Commands
            switch (ircCommand)
            {
                //https://modern.ircdocs.horse/#names-message
                case "001": // server welcome message, after this we can join
                    //:irc.faforever.com 001 Eternal- :Welcome to the FAForever IRC Network Eternal-!Eternal-@85.26.165.
                    IsIRCConnected = true;
#if DEBUG
                    App.DebugWindow.LOGIRC(data[data.LastIndexOf(':')..data.IndexOf('!')]);
#endif
                    break;
                case "321": // start of list of 322
#if DEBUG
                    App.DebugWindow.LOGIRC($"Start of available channels");
#endif
                    break;
                case "322": // channel information after 321
#if DEBUG
                    App.DebugWindow.LOGIRC($"Channel: {ircData[3]}, users: {ircData[4]}");
#endif
                    break;
                case "323": // end of list of 322
#if DEBUG
                    App.DebugWindow.LOGIRC($"End of available channels");
#endif
                    break;
                case "332": // Sent to a client when joining the <channel> to inform them of the current topic of the channel.
                    //:irc.faforever.com 332 Eternal- #aeolus :FAF rules:...
                    for (int i = 4; i < ircData.Length; i++)
                    {
                        sb.Append(ircData[i] + ' ');
                    }
                    sb.Remove(0, 1);
                    sb.Remove(sb.Length - 2, 2);

                    OnChannelTopicUpdated(new(ircData[3], sb.ToString()));
#if DEBUG
                    App.DebugWindow.LOGIRC($"Channel: {ircData[3]} topic: {sb}");
#endif
                    break;
                case "333": // Sent to a client to let them know who set the topic (<nick>) and when they set it (<setat> is a unix timestamp). Sent after RPL_TOPIC.
                    //:irc.faforever.com 333 Eternal- #aeolus Giebmasse_irc 1635328010
                    OnChannelTopicChangedBy(new(ircData[3], ircData[4], ircData[5]));
#if DEBUG
                    App.DebugWindow.LOGIRC($"Topic in channel: {ircData[3]} set by: {ircData[4]} at: {DateTime.UnixEpoch.AddSeconds(double.Parse(ircData[5]))}");
#endif
                    break;
                case "353": // channel users
                    {
                        //:irc.faforever.com 353 Eternal- = #aeolus :Eternal- HALEii_MHE_KBAC Stuba88 alximik F
                        var channel = ircData[4];
                        var users = data[data.LastIndexOf(':')..^2].Split();

                        OnChannelUsersReceived(new(channel, users));
#if DEBUG   
                        //App.DebugWindow.LOGIRC($"channel: {channel} users count: {users.Length}");
                        App.DebugWindow.LOGIRC($"channel: {channel} users: {string.Join(", ", users)}");
#endif
                    }
                    break;

                case "432": //Nickname is unavailable: Being held for registered user
                case "433":
                    Send(IrcCommands.Nickname(Nick + 1));
                    Timer timer = null;
                    TimerCallback tm = new(ChangeNickName);
                    timer = new Timer(tm, null, 10000, 10000);

                    void ChangeNickName(object obj)
                    {
                        Send(IrcCommands.Nickname(Properties.Settings.Default.PlayerNick));
                    };
                    break;
                case "JOIN": // someone joined
                    {
                        //:ThurnisHaley!396062@Clk-10163F26.hsd1.ma.comcast.net JOIN :#aeolus
                        string channel = ircData[2][1..^1];

                        OnUserJoined(new(channel, from));
#if DEBUG
                        App.DebugWindow.LOGIRC($"user: {from} joined to {channel}");
#endif
                    }
                    break;
                case "KICK":

                    break;
                case "TOPIC": // new topic
                    for (int i = 3; i < ircData.Length; i++)
                    {
                        sb.Append(ircData[i] + ' ');
                    }
                    sb.Remove(0, 1);
                    sb.Remove(sb.Length - 2, 2);
#if DEBUG
                    if (sb.Length > 0)
                    {
                        App.DebugWindow.LOGIRC($"user: {from} changed topic in channel: {ircData[2]} to: {sb}");
                    }
                    else
                    {
                        App.DebugWindow.LOGIRC($"user: {from} removed topic in channel: {ircData[2]}");
                    }
#endif
                    break;
                case "MODE": // MODE was set
                    {
                        //:Eternal-1 MODE Eternal-1 :+iwx

                        //var channel = ircData[2];
                        //if (channel != Nick)
                        //{
                        //    string from;
                        //    if (ircData[0].Contains("!"))
                        //        from = ircData[0].Substring(1, ircData[0].IndexOf("!", StringComparison.Ordinal) - 1);
                        //    else
                        //        from = ircData[0].Substring(1);

                        //    var to = ircData[4];
                        //    var mode = ircData[3];
                        //    //Fire_ChannelModeSet(new ModeSetEventArgs(channel, from, to, mode));
                        //}

                        // TODO: event for userMode's
#if DEBUG
                        App.DebugWindow.LOGIRC($"{data}");
#endif
                    }
                    break;
                case "NICK": // someone changed their nick
                    var to = data[(data.LastIndexOf(':') + 1)..^1];
                    OnUserChangedName(new(from, to));
#if DEBUG
                    App.DebugWindow.LOGIRC($"user: {from} changed his nickname from: {from} to: {to}");
#endif
                    break;
                case "NOTICE": // someone sent a notice
                    {
                        //:irc.faforever.com NOTICE * :*** Looking up your hostname...
                        string message = null;
                        int i = 1;
                        while (i < data.Length)
                        {
                            var letter = data[i];

                            if (letter == '\r')
                            {
                                sb.Remove(0, 1);
                                message = sb.ToString();
                                break;
                            }

                            if (letter == ':' || sb.Length > 0) sb.Append(letter);
                            i++;
                        }
#if DEBUG
                        App.DebugWindow.LOGIRC($"!!! Notice from {from}: {message}");
#endif
                    }
                    break;
                case "PRIVMSG": // message was sent to the channel or as private
                    {
                        var target = ircData[2];

                        for (int i = 3; i < ircData.Length; i++)
                        {
                            sb.Append(ircData[i] + ' ');
                        }
                        sb.Remove(0, 1);
                        sb.Remove(sb.Length - 2, 2);

                        if (target[0] == '#')
                        {
                            OnChannelMessage(new(target, from, sb.ToString()));
#if DEBUG
                            App.DebugWindow.LOGIRC($"user: {from} in channel: {ircData[2]} sent: {sb} ");
#endif
                        }
                        else
                        {
                            OnPrivateMessage(new(from, sb.ToString()));
#if DEBUG
                            App.DebugWindow.LOGIRC($"user: {from} sent private message: {sb} ");
#endif
                        }
                    }
                    break;
                case "PART":
                    {
                        OnUserLeft(new(ircData[2], from));
#if DEBUG
                        App.DebugWindow.LOGIRC($"user: {from} left from {ircData[2]}");
#endif
                    }
                    break;
                case "QUIT":// someone left
                    {
                        //:Mavr390!15632@93DACFB2.4D35C09C.2CB4B448.IP QUIT :Quit: Mavr390

                        OnUserDisconnected(from);
#if DEBUG
                        App.DebugWindow.LOGIRC($"user: {from} disconnected from IRC server");
#endif
                    }
                    break;
                default:

#if DEBUG
                    App.DebugWindow.LOGIRC(data);
#endif
                    break;
            }

        }

        private void OnIrcConnected(bool isConnected) => IrcConnected?.Invoke(this, isConnected);

        private void OnUserConnected(string user) => UserConnected?.Invoke(this, user);
        private void OnUserDisconnected(string user) => UserDisconnected?.Invoke(this, user);

        private void OnUserJoined(IrcUserJoin data) => UserJoined?.Invoke(this, data);
        private void OnUserLeft(IrcUserLeft data) => UserLeft?.Invoke(this, data);

        private void OnUserChangedName(IrcUserChangedName data) => UserChangedName?.Invoke(this, data);

        private void OnPrivateMessage(IrcPrivateMessage data) => PrivateMessageReceived?.Invoke(this, data);
        private void OnChannelMessage(IrcChannelMessage data) => ChannedMessageReceived?.Invoke(this, data);

        private void OnChannelTopicUpdated(IrcChannelTopicUpdated data) => ChannelTopicUpdated?.Invoke(this, data);
        private void OnChannelTopicChangedBy(IrcChannelTopicChangedBy data) => ChannelTopicChangedBy?.Invoke(this, data);
        private void OnChannelUsersReceived(IrcChannelUsers data) => ChannelUsersReceived?.Invoke(this, data);

        #endregion

        #endregion
    }
}
