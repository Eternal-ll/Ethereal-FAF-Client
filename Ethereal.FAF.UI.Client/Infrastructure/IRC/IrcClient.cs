using Ethereal.FAF.UI.Client.Models.IRC.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TcpClient = NetCoreServer.TcpClient;

namespace Ethereal.FAF.UI.Client.Infrastructure.IRC
{
    public sealed class IrcClient : TcpClient
    {
        //public event EventHandler<IrcState> StateChanged;

        public event EventHandler<bool> Authorized;

        public event EventHandler<string> UserConnected;
        public event EventHandler<(string user, string id)> UserDisconnected;
        public event EventHandler<(string channel, string user)> UserJoined;
        public event EventHandler<(string channel, string user)> UserLeft;
        public event EventHandler<(string user, string to)> UserChangedName;

        public event EventHandler<(string channel, string from, string message)> ChannelMessageReceived;

        public event EventHandler<(string channel, string topic, string by)> ChannelTopicUpdated;
        public event EventHandler<(string channel, string user, string at)> ChannelTopicChangedBy;

        public event EventHandler<(string channel, string[] users)> ChannelUsersReceived;
        public event EventHandler<(string channel, int users)[]> AvailableChannels;

        public event EventHandler<string> NotificationMessageReceived;


        public string User;
        private string UserId;
        private string OriginalNick;
        private string Password;
        private readonly List<string> ChannelUsersCache = new();
        private readonly ILogger Logger;

        public IrcClient(string host, int port, ILogger logger)
            : base(address: Dns.GetHostEntry(host).AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork),
                  port: port)
        {
            Logger = logger;
        }
        List<byte> Cache = new();
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            if (Cache.Count == 0 && buffer[0] == '{' && buffer[^2] == '\r' && buffer[^1] == '\n')
            {
                ProcessData(Encoding.UTF8.GetString(buffer, 0, (int)size));
                return;
            }
            byte last = 0;
            for (int i = (int)offset; i < (int)size; i++)
            {
                if (buffer[i] == '\r')
                {
                    ProcessData(Encoding.UTF8.GetString(Cache.ToArray(), 0, Cache.Count));
                    Cache.Clear();
                    i++;
                    continue;
                }
                //last = buffer[i];
                Cache.Add(buffer[i]);
            }
        }

        private void SetupRenameTask() => RenameTask = Task.Run(async () =>
        {
            await Task.Delay(15000);
            SendAsync(IrcCommands.Nickname(OriginalNick));
            await Task.Delay(20000);
            if (User != OriginalNick)
            {
                SetupRenameTask();
            }
            else
            {
                RenameTask = null;
            }
        });

        private Task RenameTask;

        List<(string channel, int users)> Channels;
        private void ProcessData(string data)
        {
            if (data.EndsWith("NOTICE * :*** Looking up your hostname..."))
            {
                PassAuthorization();
            }

            //Logger.LogTrace($"{data}");
            string[] ircData = data.Split(' ');
            if (data.StartsWith("PING"))
            {
                Send(IrcCommands.Pong(ircData[1][1..]));
                return;
            }
            var ircCommand = ircData[1];
            StringBuilder sb = new();
            string from = string.Empty;
            for (int i = 1; i < data.Length; i++)
            {
                var letter = data[i];
                if (letter == '!' || letter == ' ') break;
                from += letter;
            }

            var user = "";
            
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] == '@') break;
                user += data[i];
            }
            Logger.LogTrace($"[Inbound message] {data}");
            switch (ircCommand)
            {
                //https://modern.ircdocs.horse/#names-message
                case "001": // server welcome message, after this we can join
                    //:irc.faforever.com 001 Eternal- :Welcome to the FAForever IRC Network Eternal-!Eternal-@85.26.165.
                    //State = IrcState.Authorized;
                    //AppDebugger.LOGIRC(data[data.LastIndexOf(':')..data.IndexOf('!')]);
                    //Join("#aeolus");
                    Authorized?.Invoke(this, true);
                    break;
                case "451":
                    PassAuthorization();
                    break;
                case "321": // start of list of 322
                    //AppDebugger.LOGIRC($"Start of available channels");
                    Channels = new();
                    break;
                case "322": // channel information after 321
                    //AppDebugger.LOGIRC($"Channel: {ircData[3]}, users count: {ircData[4]}");
                    Channels.Add((ircData[3], int.Parse(ircData[4])));
                    break;
                case "323": // end of list of 322
                    //AppDebugger.LOGIRC($"End of available channels");
                    AvailableChannels?.Invoke(this, Channels.ToArray());
                    Channels = null;
                    break;
                case "332": // Sent to a client when joining the <channel> to inform them of the current topic of the channel.
                    //:irc.faforever.com 332 Eternal- #aeolus :FAF rules:...
                    for (int i = 4; i < ircData.Length; i++)
                    {
                        sb.Append(ircData[i] + ' ');
                    }
                    sb.Remove(0, 1);
                    sb.Remove(sb.Length - 2, 2);
                    ChannelTopicUpdated?.Invoke(this, (ircData[3], sb.ToString(), null));
                    break;
                case "333": // Sent to a client to let them know who set the topic (<nick>) and when they set it (<setat> is a unix timestamp). Sent after RPL_TOPIC.
                    //:irc.faforever.com 333 Eternal- #aeolus Giebmasse_irc 1635328010
                    ChannelTopicChangedBy?.Invoke(this, (ircData[3], ircData[4], ircData[5]));

                    //AppDebugger.LOGIRC($"Topic in channel: {ircData[3]} set by: {ircData[4]} at: {DateTime.UnixEpoch.AddSeconds(double.Parse(ircData[5]))}");

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

                        //AppDebugger.LOGIRC($"{data}");
                    }
                    break;
                case "353": // channel users after MODE
                    //:irc.faforever.com 353 Eternal- = #aeolus :Eternal- HALEii_MHE_KBAC Stuba88 alximik F
                    string channel = ircData[4];
                    var users = data[(data.LastIndexOf(':') + 1)..].Trim().Split();
                    ChannelUsersCache.AddRange(users);
                    break;
                case "366":
                    //:irc.faforever.com 366 Eternal- #test :End of /NAMES list.
                    channel = ircData[3];
                    ChannelUsersReceived?.Invoke(this, (channel, ChannelUsersCache.ToArray()));
                    ChannelUsersCache.Clear();
                    break;
                case "433"://Nickname is unavailable: Being held for registered user
                case "432":
                    if (RenameTask is null)
                    {
                        SetupRenameTask();
                    }
                    break;
                case "438":
                    break;
                case "JOIN": // someone joined
                    {
                        ////:ThurnisHaley!396062@Clk-10163F26.hsd1.ma.comcast.net JOIN :#aeolus
                        channel = ircData[2][1..];

                        UserJoined?.Invoke(this, (channel, from));
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
                    sb.Remove(sb.Length - 1, 1);
                    ChannelTopicUpdated?.Invoke(this, (ircData[2], sb.ToString(), from));
                    break;
                case "NICK": // someone changed their nick
                    var to = data[(data.LastIndexOf(':') + 1)..];
                    if (from == User)
                    {
                        User = to;
                    }
                    UserChangedName?.Invoke(this, (from, to));
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

                        //AppDebugger.LOGIRC($"!!! Notice from {from}: {message}");

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
                        var t = sb.ToString();

                        if (target[0] == '#')
                        {
                            ChannelMessageReceived?.Invoke(this, (target, from, sb.ToString()));
                        }
                        else
                        {
                            ChannelMessageReceived?.Invoke(this, (from, from, sb.ToString()));
                            //PrivateMessageReceived?.Invoke(this, (from, sb.ToString(), from));
                        }
                    }
                    break;
                case "PART":
                    {
                        //if (Nick == from) return;
                        UserLeft?.Invoke(this, (ircData[2].TrimEnd(), from.Trim()));
                    }
                    break;
                case "QUIT":// someone left
                    {
                        //:Mavr390!15632@93DACFB2.4D35C09C.2CB4B448.IP QUIT :Quit: Mavr390
                        UserDisconnected?.Invoke(this, (from, data.Split('!', '@')[1]));
                    }
                    break;
                default:
                    break;
            }


            switch (ircCommand)
            {
                case "341": // invited
                case "NOTICE":
                case "442": // cant invite, you are not in channel
                case "443": // cant invite, already in channel
                case "482": // cant kick, you are not channel operator
                case "403": // no such channel
                case "461":
                    for (int i = 3; i < ircData.Length; i++)
                    {
                        sb.Append(ircData[i] + ' ');
                    }
                    NotificationMessageReceived?.Invoke(this, sb.ToString());
                    break;
                default: break;
            }
        }

        public void Authorize(string login, string id, string password)
        {
            User = login;
            OriginalNick = login;
            UserId = id;
            Password = password;
        }
        public void PassAuthorization()
        {
            SendAsync(IrcCommands.Pass(Password));
            SendAsync(IrcCommands.UserInfo(UserId, User));
            SendAsync(IrcCommands.Nickname(User));
        }

        public void Restart()
        {
            if (IsConnected)
            {
                Quit();
                DisconnectAsync();
            }
            ConnectAsync();
        }
        public void GetChannelUsers(string channel) => Send(IrcCommands.Names(channel));
        public void Rename(string name) => Send(IrcCommands.Nickname(name));
        public void Join(string channel, string key = null) => Send(IrcCommands.Join(channel, key));
        public void Leave(string channel) => Send(IrcCommands.Leave(channel));
        public void Leave(string[] channels) => Send(IrcCommands.Leave(channels));
        public void LeaveAll() => Send(IrcCommands.LeaveAllJoinedChannels());
        public void Ping() => Send(IrcCommands.Ping());
        public void List(int minimum = 2) => Send(IrcCommands.List(minimum));
        public void Quit(string reason = null) => Send(IrcCommands.Quit(reason));
        public void SendMessage(string channelOrUser, string message)
        {
            Send(IrcCommands.Message(channelOrUser, message));
            ChannelMessageReceived?.Invoke(this, (channelOrUser, User, message));
        }
        public void SetTopic(string channel, string topic = null) => Send(IrcCommands.Topic(channel, topic));
        public void SendInvite(string user, string channel) => Send(IrcCommands.Invite(user, channel));

        protected override void OnConnected()
        {
        }
        protected override void OnDisconnected()
        {
            User = OriginalNick;
            Authorized?.Invoke(this, false);
            base.OnDisconnected();
        }
        public override bool SendAsync(string text)
        {
            Logger.LogTrace($"[Outbound message] {text}");
            if (text[^1] != '\r') text += '\r';
            return base.SendAsync(text);
        }
        public override long Send(string text)
        {
            Logger.LogTrace($"[Outbound message] {text}");
            if (text[^1] != '\r') text += '\r';
            return base.Send(text);
        }
    }
}
