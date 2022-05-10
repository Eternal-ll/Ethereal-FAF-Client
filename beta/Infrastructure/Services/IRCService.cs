using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Debugger;
using beta.Models.Enums;
using beta.Models.IRC;
using beta.Models.IRC.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace beta.Infrastructure.Services
{
    internal class IrcService : IIrcService
    {
        #region Events
        public event EventHandler<IrcState> StateChanged;

        /// <summary>
        /// User connected to IRC server
        /// </summary>
        public event EventHandler<string> UserConnected;
        /// <summary>
        /// User disconnected from IRC server
        /// </summary>
        public event EventHandler<string> UserDisconnected;

        /// <summary>
        /// User joined to specific channel
        /// </summary>
        public event EventHandler<IrcUserJoin> UserJoined;
        /// <summary>
        /// User left from specific channel
        /// </summary>
        public event EventHandler<IrcUserLeft> UserLeft;

        public event EventHandler<IrcUserChangedName> UserChangedName;

        /// <summary>
        /// Private message received from specific user
        /// </summary>
        public event EventHandler<IrcPrivateMessage> PrivateMessageReceived;
        /// <summary>
        /// Channel message received from specific channel
        /// </summary>
        public event EventHandler<IrcChannelMessage> ChannedMessageReceived;

        /// <summary>
        /// Specific channel topic updated
        /// </summary>
        public event EventHandler<IrcChannelTopicUpdated> ChannelTopicUpdated;

        /// <summary>
        /// Specific channel topic changed by specific user
        /// </summary>
        public event EventHandler<IrcChannelTopicChangedBy> ChannelTopicChangedBy;
        
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<IrcChannelUsers> ChannelUsersReceived;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<IrcNotificationMessage> NotificationMessageReceived;

        #endregion

        #region Properties

        private ManagedTcpClient ManagedTcpClient;

        private string Nick;
        private string Password;

        public List<string> Channels { get; private set; } = new();

        #region State
        private IrcState _State;
        public IrcState State
        {
            get => _State;
            set
            {
                if (_State != value)
                {
                    _State = value;
                    OnStateChanged(value);
                }
            }
        } 
        #endregion

        #endregion

        #region Methods

        #region Public

        public void Restart(string nickname, string password)
        {
            Quit();
            Authorize(nickname, password);
        }

        public void Authorize(string nickname, string password)
        {
            if (State == IrcState.Connected || State == IrcState.PendingConnection || State == IrcState.PendingAuthorization)
            {
                return;
            }
            Nick = nickname;
            Password = password;
            Connect(null, 0);
        }

        private void TcpClientStateChanged(object sender, ManagedTcpClientState e)
        {
            switch (e)
            {
                case ManagedTcpClientState.Disconnected: State = IrcState.Disconnected;
                    State = IrcState.Disconnected;

                    foreach (var item in ChannelUsers)
                    {
                        item.Value.Clear();
                    }
                    break;
                case ManagedTcpClientState.TimedOut: State = IrcState.TimedOut;
                    State = IrcState.TimedOut;
                    break;
                case ManagedTcpClientState.CantConnect: State = IrcState.CantConnect;
                    State = IrcState.CantConnect;
                    break;
                case ManagedTcpClientState.PendingConnection: State = IrcState.PendingConnection;
                    State = IrcState.PendingConnection;
                    break;
                case ManagedTcpClientState.Connected:
                    var client = (ManagedTcpClient)sender;
                    client.DataReceived += ManagedTcpClient_DataReceived;
                    AppDebugger.LOGIRC($"Connected to IRC Server");
                    AppDebugger.LOGIRC($"Sending authorization information...");
                    Authorize();
                    State = IrcState.Connected;
                    break;
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
            if (ManagedTcpClient is not null)
            {
                ManagedTcpClient.Dispose();
                ManagedTcpClient = null;
            }

            ManagedTcpClient = new()
            {
                ThreadName = "IRC TCP",
                Host = "lobby.faforever.com",
                Port = 6667
            };
            ManagedTcpClient.StateChanged += TcpClientStateChanged;
            ManagedTcpClient.Connect();
        }

        public void Join(string channel, string key = null) => Send(IrcCommands.Join(channel, key));

        public void Leave(string channel)
        {
            Channels.Remove(channel);
            ChannelUsers.Remove(channel);
            Send(IrcCommands.Leave(channel));
        }

        public void Leave(string[] channels)
        {
            throw new NotImplementedException();
        }

        public void LeaveAll() => Send(IrcCommands.LeaveAllJoinedChannels());

        public void Ping() => Send(IrcCommands.Ping());

        public void Quit(string reason = null) => Send(IrcCommands.Quit(reason));

        public void SendMessage(string channelOrUser, string message)
        {
            Send(IrcCommands.Message(channelOrUser, message));
            if (channelOrUser.StartsWith('#'))
            {
                OnChannelMessage(new(channelOrUser, Nick, message));
            }
        }

        public void SetTopic(string channel, string topic = null) => Send(IrcCommands.Topic(channel, topic));

        public void SendInvite(string user, string channel) => Send(IrcCommands.Invite(user, channel));

        #endregion

        #region Private

        private void Send(string message)
        {
            if (ManagedTcpClient == null)
            {
                return;
                //throw new NullReferenceException(nameof(ManagedTcpClient));
            }

            //TODO fix 
            if (ManagedTcpClient?.TcpClient.Connected == false)
            {
                State = IrcState.Disconnected;
                return;
            }
            ManagedTcpClient?.Write(message + '\r');
            if (message.StartsWith("QUIT"))
            {
                ManagedTcpClient?.Dispose();
                ManagedTcpClient = null;

                AppDebugger.LOGIRC($"Disconnected from IRC Server");
            }

            AppDebugger.LOGIRC($"You sent: {message}");
        }

        private readonly Dictionary<string, List<string>> ChannelUsers = new();

        public void Test()
        {
            //Send(IrcCommands.Leave("#aeolus"));
            //Send(IrcCommands.Quit());
            //Connect(null, 0);
            //Authorize();
            //Send(IrcCommands.Message("#aeolus", "Check check"));
            //Send(IrcCommands.Join("#test"));
            //Send(IrcCommands.Nickname(Nick + "1"));
            //Send(IrcCommands.Nickname("Eternal-"));
            //Send("NAMES #aeolus");
            //Send("OPER Eternal- Qzwxec12#");
            //Send("MODE #test2 +o MarcSpector");
            //Send("INVITE MarcSpector #aeolus1");
            //Send(IrcCommands.Leave("#aeolus1"));
            //Send(IrcCommands.Join("#aeolus2", "test")); 
            //Send("MODE #aeolus2");
            //Send(IrcCommands.Topic("#aeolus1", "test"));
            Send("KICK #test2 Didvul");
            //Send(IrcCommands.List());
            //Send(IrcCommands.Nickname("Eternal-"));
        }

        public void GetChannelUsers(string channel) => SendCommand(IrcUserCommand.NAMES, IrcCommands.Names(channel), channel);

        Timer timer = null;
        TimerCallback tm = null;

        void ChangeNickName(object obj)
        {
            Send(IrcCommands.Nickname(Properties.Settings.Default.PlayerNick));
            timer?.Dispose();
            timer = null;
        }

        private void ManagedTcpClient_DataReceived(object sender, string data)
        {
            // split the data into parts
            string[] ircData = data.Split(' ');

            // if the message starts with PING we must PONG back
            if (data.StartsWith("PING"))
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
                    State = IrcState.Authorized;
                    AppDebugger.LOGIRC(data[data.LastIndexOf(':')..data.IndexOf('!')]);
                    if (Channels.Count == 0) Join("#aeolus");
                    for (int i = 0; i < Channels.Count; i++)
                    {
                        Join(Channels[i]);
                    }
                    break;
                case "321": // start of list of 322
                    AppDebugger.LOGIRC($"Start of available channels");
                    break;
                case "322": // channel information after 321
                    AppDebugger.LOGIRC($"Channel: {ircData[3]}, users: {ircData[4]}");
                    break;
                case "323": // end of list of 322
                    AppDebugger.LOGIRC($"End of available channels");
                    break;
                case "332": // Sent to a client when joining the <channel> to inform them of the current topic of the channel.
                    //:irc.faforever.com 332 Eternal- #aeolus :FAF rules:...
                    for (int i = 4; i < ircData.Length; i++)
                    {
                        sb.Append(ircData[i] + ' ');
                    }
                    sb.Remove(0, 1);
                    sb.Remove(sb.Length - 2, 2);

                    OnChannelTopicUpdated(new(ircData[3], sb.ToString(), from));

                    AppDebugger.LOGIRC($"Channel: {ircData[3]} topic: {sb}");
                    if (!Channels.Contains(ircData[3])) Channels.Add(ircData[3]);

                    break;
                case "333": // Sent to a client to let them know who set the topic (<nick>) and when they set it (<setat> is a unix timestamp). Sent after RPL_TOPIC.
                    //:irc.faforever.com 333 Eternal- #aeolus Giebmasse_irc 1635328010
                    OnChannelTopicChangedBy(new(ircData[3], ircData[4], ircData[5]));

                    AppDebugger.LOGIRC($"Topic in channel: {ircData[3]} set by: {ircData[4]} at: {DateTime.UnixEpoch.AddSeconds(double.Parse(ircData[5]))}");

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

                        AppDebugger.LOGIRC($"{data}");
                    }
                    break;
                case "353": // channel users after MODE
                    
                    //:irc.faforever.com 353 Eternal- = #aeolus :Eternal- HALEii_MHE_KBAC Stuba88 alximik F
                    string channel = ircData[4];
                    var users = data[(data.LastIndexOf(':') + 1)..^1].Trim().Split();
                    //OnChannelUsersReceived(new(channel, users));

                    //AppDebugger.LOGIRC($"channel: {channel} users count: {users.Length}");
                    AppDebugger.LOGIRC($"channel: {channel} users: {string.Join(", ", users)}");

                    if (ChannelUsers.ContainsKey(channel))
                    {
                        ChannelUsers[channel].AddRange(users);
                    }
                    else
                    {
                        ChannelUsers.Add(channel, new(users));
                    }

                    
                    break;
                case "366":
                    //:irc.faforever.com 366 Eternal- #test :End of /NAMES list.
                    channel = ircData[3];
                    OnChannelUsersReceived(new(channel, ChannelUsers[channel].ToArray()));
                    AppDebugger.LOGIRC($"End of user lists: {channel} users: {ChannelUsers[channel].Count}");
                    ChannelUsers.Remove(channel);
                    break;

                case "433"://Nickname is unavailable: Being held for registered user
                case "432":
                    if (timer is null)
                    {
                        if (tm is null) tm = new(ChangeNickName);
                        Send(IrcCommands.Nickname(Nick + '`'));
                        timer = new Timer(tm, null, 15000, 15000);
                    }
                    break;
                case "JOIN": // someone joined
                    {
                        //:ThurnisHaley!396062@Clk-10163F26.hsd1.ma.comcast.net JOIN :#aeolus
                        channel = ircData[2][1..^1];

                        OnUserJoined(new(channel, from));

                        AppDebugger.LOGIRC($"user: {from} joined to {channel}");

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

                    if (sb.Length > 0)
                    {
                        AppDebugger.LOGIRC($"user: {from} changed topic in channel: {ircData[2]} to: {sb}");
                    }
                    else
                    {
                        AppDebugger.LOGIRC($"user: {from} removed topic in channel: {ircData[2]}");
                    }
                    OnChannelTopicUpdated(new(ircData[2], sb.ToString(), from));

                    break;
                case "NICK": // someone changed their nick
                    var to = data[(data.LastIndexOf(':') + 1)..^1];
                    OnUserChangedName(new(from, to));

                    AppDebugger.LOGIRC($"user: {from} changed his nickname from: {from} to: {to}");

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

                        AppDebugger.LOGIRC($"!!! Notice from {from}: {message}");

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
                            IrcChannelMessage msg = new(target, from, sb.ToString());
                            OnChannelMessage(msg);

                            // TODO hardcoded xD but still
                            if (msg.HasMention)
                            {

                            }

                            AppDebugger.LOGIRC($"user: {from} in channel: {ircData[2]} sent: {sb} ");

                        }
                        else
                        {
                            OnPrivateMessage(new(from, sb.ToString()));

                            AppDebugger.LOGIRC($"user: {from} sent private message: {sb} ");

                        }
                    }
                    break;
                case "PART":
                    {
                        OnUserLeft(new(ircData[2].TrimEnd(), from.Trim()));

                        AppDebugger.LOGIRC($"user: {from} left from {ircData[2]}");

                    }
                    break;
                case "QUIT":// someone left
                    {
                        //:Mavr390!15632@93DACFB2.4D35C09C.2CB4B448.IP QUIT :Quit: Mavr390

                        OnUserDisconnected(from);

                        AppDebugger.LOGIRC($"user: {from} disconnected from IRC server");

                    }
                    break;
                default:

                    if (data.StartsWith("ERROR"))
                    {
                        State = IrcState.Throttled;
                    }
                    AppDebugger.LOGIRC(data);

                    break;
            }

        }

        public void SendCommand(IrcUserCommand command, string text, string channel)
        {
            /* FORMAT
            
            /<Command> <params> <params>

             */
            var data = text.Split();
            switch (command)
            {
                case IrcUserCommand.INVITE:
                    channel = text.Split()[1][1..];
                    string target = text.Split()[1];
                    SendInvite(target, channel);
                    break;
                case IrcUserCommand.JOIN:
                    channel = text.Split()[1];
                    if (!channel.StartsWith('#')) channel = "#" + channel;
                    Join(channel);
                    break;
                case IrcUserCommand.KICK:
                    channel = text.Split()[1][1..];
                    break;
                case IrcUserCommand.NAMES:
                    Send(text);
                    break;
                case IrcUserCommand.LIST:
                    break;
                case IrcUserCommand.PRIVMSG:
                    break;
                case IrcUserCommand.QUIT:
                    foreach (var item in ChannelUsers)
                    {
                        item.Value.Clear();
                    }
                    Quit();
                    break;
                case IrcUserCommand.TOPIC:
                    string newTopic = text[text.IndexOf(' ')..];
                    SetTopic(channel, newTopic);
                    break;
                case IrcUserCommand.PART:
                    if (!channel.StartsWith('#')) channel = "#" + channel;
                    ChannelUsers.Remove(channel);
                    break;
                default:
                    throw new NotImplementedException("IRC Command is not recognized");
            }
        }

        private void OnStateChanged(IrcState state) => StateChanged?.Invoke(this, state);

        private void OnUserConnected(string user) => UserConnected?.Invoke(this, user);
        private void OnUserDisconnected(string user) => UserDisconnected?.Invoke(this, user);

        private void OnUserJoined(IrcUserJoin e) => UserJoined?.Invoke(this, e);
        private void OnUserLeft(IrcUserLeft e) => UserLeft?.Invoke(this, e);

        private void OnUserChangedName(IrcUserChangedName e) => UserChangedName?.Invoke(this, e);

        private void OnPrivateMessage(IrcPrivateMessage e) => PrivateMessageReceived?.Invoke(this, e);
        private void OnChannelMessage(IrcChannelMessage e) => ChannedMessageReceived?.Invoke(this, e);

        private void OnChannelTopicUpdated(IrcChannelTopicUpdated e) => ChannelTopicUpdated?.Invoke(this, e);
        private void OnChannelTopicChangedBy(IrcChannelTopicChangedBy e) => ChannelTopicChangedBy?.Invoke(this, e);
        private void OnChannelUsersReceived(IrcChannelUsers e) => ChannelUsersReceived?.Invoke(this, e);

        private void OnNotificationMessageReceived(IrcNotificationMessage e) => NotificationMessageReceived?.Invoke(this, e);

        #endregion

        #endregion
    }
}
