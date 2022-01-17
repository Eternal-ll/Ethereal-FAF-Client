using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace beta.Models
{
    public class IrcClient : IDisposable
    {
        #region Variables
        private string _server = "";

        private int _port = 6667;

        private string _ServerPass = "";

        private string _nick = "Test";

        private string _altNick = "";

        private bool _consoleOutput = false;

        // private TcpClient used to talk to the server
        private TcpClient TcpClient;

        // private network stream used to read/write from/to
        private NetworkStream stream;

        // private ssl stream used to read/write from/to
        private SslStream ssl;

        // global variable used to read input from the client
        private string inputLine;

        // stream reader to read from the network stream
        private StreamReader reader;

        // stream writer to write to the stream
        private StreamWriter writer;

        // AsyncOperation used to handle cross-thread wonderness
        private AsyncOperation op;

        #endregion

        #region Constructors

        /// <summary>
        /// IrcClient used to connect to an IRC Server (default port: 6667) (default ssl: false)
        /// </summary>
        /// <param name="Server">IRC Server</param>
        public IrcClient(string Server) : this(Server, 6667) { }

        /// <summary>
        /// IrcClient used to connect to an IRC Server
        /// </summary>
        /// <param name="Server">IRC Server</param>
        /// <param name="Port">IRC Port (6667 if you are unsure)</param>
        public IrcClient(string Server, int Port)
        {
            op = AsyncOperationManager.CreateOperation(null);
            _server = Server;
            _port = Port;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the Server address used
        /// </summary>
        public string Server => _server;

        /// <summary>
        /// Returns the Port used
        /// </summary>
        public int Port => _port;

        /// <summary>
        /// Returns the password used to auth to the server
        /// </summary>
        public string ServerPass
        {
            get => _ServerPass;
            set => _ServerPass = value;
        }
        /// <summary>
        /// Returns the current nick being used.
        /// </summary>
        public string Nick
        {
            get => _nick;
            set => _nick = value;
        }
        /// <summary>
        /// Returns the alternate nick being used
        /// </summary>
        public string AltNick
        {
            get => _altNick;
            set => _altNick = value;
        }
        /// <summary>
        /// Output RAW IRC data to console
        /// </summary>
        public bool ConsoleOutput
        {
            get => _consoleOutput;
            set => _consoleOutput = value;
        }

        /// <summary>
        /// Returns true if the client is connected.
        /// </summary>
        public bool Connected => TcpClient is { Connected: true };

        #endregion

        #region Events

        public event EventHandler<StringEventArgs> Pinged = delegate { };
        public event EventHandler<UpdateUsersEventArgs> UpdateUsers = delegate { };
        public event EventHandler<UserJoinedEventArgs> UserJoined = delegate { };
        public event EventHandler<UserLeftEventArgs> UserLeft = delegate { };
        public event EventHandler<UserNickChangedEventArgs> UserNickChange = delegate { };

        public event EventHandler<ChannelMessageEventArgs> ChannelMessage = delegate { };
        public event EventHandler<NoticeMessageEventArgs> NoticeMessage = delegate { };
        public event EventHandler<PrivateMessageEventArgs> PrivateMessage = delegate { };
        public event EventHandler<StringEventArgs> ServerMessage = delegate { };

        public event EventHandler<StringEventArgs> NickTaken = delegate { };

        public event EventHandler OnConnect = delegate { };

        public event EventHandler<ExceptionEventArgs> ExceptionThrown = delegate { };

        public event EventHandler<ModeSetEventArgs> ChannelModeSet = delegate { };

        private void Fire_UpdateUsers(UpdateUsersEventArgs o)
        {
            op.Post(x => UpdateUsers(this, (UpdateUsersEventArgs)x), o);
        }
        private void Fire_UserJoined(UserJoinedEventArgs o)
        {
            op.Post(x => UserJoined(this, (UserJoinedEventArgs)x), o);

        }
        private void Fire_UserLeft(UserLeftEventArgs o)
        {
            op.Post(x => UserLeft(this, (UserLeftEventArgs)x), o);
        }
        private void Fire_NickChanged(UserNickChangedEventArgs o)
        {
            op.Post(x => UserNickChange(this, (UserNickChangedEventArgs)x), o);
        }
        private void Fire_ChannelMessage(ChannelMessageEventArgs o)
        {
            op.Post(x => ChannelMessage(this, (ChannelMessageEventArgs)x), o);
        }
        private void Fire_NoticeMessage(NoticeMessageEventArgs o)
        {
            op.Post(x => NoticeMessage(this, (NoticeMessageEventArgs)x), o);
        }
        private void Fire_PrivateMessage(PrivateMessageEventArgs o)
        {
            op.Post(x => PrivateMessage(this, (PrivateMessageEventArgs)x), o);
        }
        private void Fire_ServerMesssage(string s)
        {
            op.Post(x => ServerMessage(this, (StringEventArgs)x), new StringEventArgs(s));
        }
        private void Fire_NickTaken(string s)
        {
            op.Post(x => NickTaken(this, (StringEventArgs)x), new StringEventArgs(s));
        }
        private void Fire_Connected()
        {
            op.Post((x) => OnConnect(this, null), null);
        }
        private void Fire_ExceptionThrown(Exception ex)
        {
            op.Post(x => ExceptionThrown(this, (ExceptionEventArgs)x), new ExceptionEventArgs(ex));
        }
        private void Fire_ChannelModeSet(ModeSetEventArgs o)
        {
            op.Post(x => ChannelModeSet(this, (ModeSetEventArgs)x), o);
        }
        #endregion

        #region PublicMethods
        /// <summary>
        /// Connect to the IRC server
        /// </summary>
        public void Connect()
        {
            Thread t = new Thread(DoConnect) { IsBackground = true };
            t.Start();
        }
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        private void DoConnect()
        {
            //try
            //{
            TcpClient = new TcpClient("irc.faforever.com", _port);

            stream = TcpClient.GetStream();
            ssl = new SslStream(stream, false, ValidateServerCertificate, null);
            //SslStream.AuthenticateAsClient("irc.faforever.com");
            //ssl = new SslStream(stream, false);
            ssl.AuthenticateAsClient("irc.faforever.com");
            //reader = new StreamReader(ssl);
            //writer = new StreamWriter(ssl);

            if (!string.IsNullOrEmpty(_ServerPass))
                Send("PASS " + _ServerPass);

            Send("NICK " + _nick);
            Send("USER " + _nick + " 0 * :" + _nick);

            while (TcpClient.Connected)
            {
                Listen();
                Thread.Sleep(10);
            }
            //}
            //catch (Exception ex)
            //{
            //    Fire_ExceptionThrown(ex);
            //}
        }
        /// <summary>
        /// Disconnect from the IRC server
        /// </summary>
        public void Disconnect()
        {
            if (TcpClient != null)
            {
                if (TcpClient.Connected)
                {
                    Send("QUIT Client Disconnected: http://tech.reboot.pro");
                }
                TcpClient = null;
            }
        }
        /// <summary>
        /// Sends the JOIN command to the server
        /// </summary>
        /// <param name="channel">Channel to join</param>
        public void JoinChannel(string channel)
        {
            if (TcpClient != null && TcpClient.Connected)
            {
                Send("JOIN " + channel);
            }
        }
        /// <summary>
        /// Sends the PART command for a given channel
        /// </summary>
        /// <param name="channel">Channel to leave</param>
        public void PartChannel(string channel)
        {
            Send("PART " + channel);
        }
        /// <summary>
        /// Send a notice to a user
        /// </summary>
        /// <param name="toNick">User to send the notice to</param>
        /// <param name="message">The message to send</param>
        public void SendNotice(string toNick, string message)
        {
            Send("NOTICE " + toNick + " :" + message);
        }

        /// <summary>
        /// Send a message to the channel
        /// </summary>
        /// <param name="channel">Channel to send message</param>
        /// <param name="message">Message to send</param>
        public void SendMessage(string channel, string message)
        {
            Send("PRIVMSG " + channel + " :" + message);
        }
        /// <summary>
        /// Send RAW IRC commands
        /// </summary>
        /// <param name="message"></param>
        public void SendRaw(string message)
        {
            Send(message);
        }

        public void Dispose()
        {
            stream.Dispose();
            ssl.Dispose();
            writer.Dispose();
            reader.Dispose();
        }
        #endregion

        #region PrivateMethods

        private readonly List<byte> _queuedMsg = new();
        private readonly Encoding StringEncoder = Encoding.UTF8;
        /// <summary>
        /// Listens for messages from the server
        /// </summary>
        /// 
        private readonly StringBuilder builder = new();
        private void Listen()
        {
            //if (TcpClient == null) return;
            //if (TcpClient.Connected == false) return;

            var c = TcpClient;

            int bytesAvailable = c.Available;
            if (bytesAvailable == 0)
                return;

            while (bytesAvailable > 0 && c.Connected)
            {
                byte[] nextByte = new byte[1];
                ssl.Read(nextByte, 0, 1);

                //bytesReceived.AddRange(nextByte);

                //              == \n
                if (nextByte[0] == 10)
                {
                    ParseData(builder.Clear()
                        .Append(StringEncoder.GetChars(_queuedMsg.ToArray()))
                        .ToString());
                    _queuedMsg.Clear();
                }
                else _queuedMsg.AddRange(nextByte);
            }

        }
        //private void Listen()
        //{

        //    while ((inputLine = reader.ReadLine()) != null)
        //    {
        //        try
        //        {
        //            ParseData(inputLine);
        //            if (_consoleOutput) Console.Write(inputLine);
        //        }
        //        catch (Exception ex)
        //        {
        //            Fire_ExceptionThrown(ex);
        //        }
        //    }//end while
        //}

        /// <summary>
        /// Parses data sent from the server
        /// </summary>
        /// <param name="data">message from the server</param>
        private void ParseData(string data)
        {
            // split the data into parts
            string[] ircData = data.Split(' ');

            var ircCommand = ircData[1];

            // if the message starts with PING we must PONG back
            if (data.Length > 4)
            {
                if (data.Substring(0, 4) == "PING")
                {
                    Send("PONG " + ircData[1].Replace(":", string.Empty));
                    return;
                }

            }

            // re-act according to the IRC Commands
            switch (ircCommand)
            {
                case "001": // server welcome message, after this we can join
                    Fire_Connected();
                    return;
                    Send("MODE " + _nick + " +B");
                    Fire_Connected();    //TODO: this might not work
                    break;
                case "353": // member list
                    {
                        var channel = ircData[4];
                        var userList = JoinArray(ircData, 5).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        Fire_UpdateUsers(new UpdateUsersEventArgs(channel, userList));
                    }
                    break;
                case "433":
                    var takenNick = ircData[3];

                    // notify user
                    Fire_NickTaken(takenNick);

                    // try alt nick if it's the first time 
                    if (takenNick == _altNick)
                    {
                        var rand = new Random();
                        var randomNick = "Guest" + rand.Next(0, 9) + rand.Next(0, 9) + rand.Next(0, 9);
                        Send("NICK " + randomNick);
                        Send("USER " + randomNick + " 0 * :" + randomNick);
                        _nick = randomNick;
                    }
                    else
                    {
                        Send("NICK " + _altNick);
                        Send("USER " + _altNick + " 0 * :" + _altNick);
                        _nick = _altNick;
                    }
                    break;
                case "JOIN": // someone joined
                    {
                        var channel = ircData[2];
                        var user = ircData[0].Substring(1, ircData[0].IndexOf("!", StringComparison.Ordinal) - 1);
                        Fire_UserJoined(new UserJoinedEventArgs(channel, user));
                    }
                    break;
                case "MODE": // MODE was set
                    {
                        var channel = ircData[2];
                        if (channel != Nick)
                        {
                            string from;
                            if (ircData[0].Contains("!"))
                                from = ircData[0].Substring(1, ircData[0].IndexOf("!", StringComparison.Ordinal) - 1);
                            else
                                from = ircData[0].Substring(1);

                            var to = ircData[4];
                            var mode = ircData[3];
                            Fire_ChannelModeSet(new ModeSetEventArgs(channel, from, to, mode));
                        }

                        // TODO: event for userMode's
                    }
                    break;
                case "NICK": // someone changed their nick
                    var oldNick = ircData[0].Substring(1, ircData[0].IndexOf("!", StringComparison.Ordinal) - 1);
                    var newNick = JoinArray(ircData, 3);

                    Fire_NickChanged(new UserNickChangedEventArgs(oldNick, newNick));
                    break;
                case "NOTICE": // someone sent a notice
                    {
                        var from = ircData[0];
                        var message = JoinArray(ircData, 3);
                        if (from.Contains("!"))
                        {
                            from = from.Substring(1, ircData[0].IndexOf('!') - 1);
                            Fire_NoticeMessage(new NoticeMessageEventArgs(from, message));
                        }
                        else
                            Fire_NoticeMessage(new NoticeMessageEventArgs(_server, message));
                    }
                    break;
                case "PRIVMSG": // message was sent to the channel or as private
                    {
                        var from = ircData[0].Substring(1, ircData[0].IndexOf('!') - 1);
                        var to = ircData[2];
                        var message = JoinArray(ircData, 3);

                        // if it's a private message
                        if (string.Equals(to, _nick, StringComparison.CurrentCultureIgnoreCase))
                            Fire_PrivateMessage(new PrivateMessageEventArgs(from, message));
                        else
                            Fire_ChannelMessage(new ChannelMessageEventArgs(to, from, message));
                    }
                    break;
                case "PART":
                case "QUIT":// someone left
                    {
                        var channel = ircData[2];
                        var user = ircData[0].Substring(1, data.IndexOf("!") - 1);

                        Fire_UserLeft(new UserLeftEventArgs(channel, user));
                        //Send("NAMES " + ircData[2]);
                    }
                    break;
                default:
                    // still using this while debugging

                    if (ircData.Length > 3)
                        Fire_ServerMesssage(JoinArray(ircData, 3));

                    break;
            }

        }
        /// <summary>
        /// Strips the message of unnecessary characters
        /// </summary>
        /// <param name="message">Message to strip</param>
        /// <returns>Stripped message</returns>
        private static string StripMessage(string message)
        {
            // remove IRC Color Codes
            foreach (Match m in new Regex((char)3 + @"(?:\d{1,2}(?:,\d{1,2})?)?").Matches(message))
                message = message.Replace(m.Value, "");

            // if there is nothing to strip
            if (message == "")
                return "";
            else if (message.Substring(0, 1) == ":" && message.Length > 2)
                return message.Substring(1, message.Length - 1);
            else
                return message;
        }
        /// <summary>
        /// Joins the array into a string after a specific index
        /// </summary>
        /// <param name="strArray">Array of strings</param>
        /// <param name="startIndex">Starting index</param>
        /// <returns>String</returns>
        private static string JoinArray(string[] strArray, int startIndex)
        {
            return StripMessage(string.Join(" ", strArray, startIndex, strArray.Length - startIndex));
        }
        public void Write(byte[] data)
        {
            //if (_client == null) { throw new Exception("Cannot send data to a null TcpClient (check to see if Connect was called)"); }
            ssl.Write(data, 0, data.Length);
        }

        public void Write(string data)
        {
            if (data == null) { return; }
            Write(StringEncoder.GetBytes(data));
        }
        /// <summary>
        /// Send message to server
        /// </summary>
        /// <param name="message">Message to send</param>
        private void Send(string message)
        {
            Write(message + " \r\n");
            return;
            writer.WriteLine(message);
            writer.Flush();
        }
        #endregion
    }

    public class UpdateUsersEventArgs : EventArgs
    {
        public string Channel { get; internal set; }
        public string[] UserList { get; internal set; }

        public UpdateUsersEventArgs(string channel, string[] userList)
        {
            Channel = channel;
            UserList = userList;
        }
    }

    public class UserJoinedEventArgs : EventArgs
    {
        public string Channel { get; internal set; }
        public string User { get; internal set; }

        public UserJoinedEventArgs(string channel, string user)
        {
            Channel = channel;
            User = user;
        }
    }

    public class UserLeftEventArgs : EventArgs
    {
        public string Channel { get; internal set; }
        public string User { get; internal set; }

        public UserLeftEventArgs(string channel, string user)
        {
            Channel = channel;
            User = user;
        }
    }

    public class ChannelMessageEventArgs : EventArgs
    {
        public string Channel { get; internal set; }
        public string From { get; internal set; }
        public string Message { get; internal set; }

        public ChannelMessageEventArgs(string channel, string from, string message)
        {
            Channel = channel;
            From = from;
            Message = message;
        }
    }

    public class NoticeMessageEventArgs : EventArgs
    {
        public string From { get; internal set; }
        public string Message { get; internal set; }

        public NoticeMessageEventArgs(string from, string message)
        {
            From = from;
            Message = message;
        }
    }

    public class PrivateMessageEventArgs : EventArgs
    {
        public string From { get; internal set; }
        public string Message { get; internal set; }

        public PrivateMessageEventArgs(string from, string message)
        {
            From = from;
            Message = message;
        }
    }

    public class UserNickChangedEventArgs : EventArgs
    {
        public string Old { get; internal set; }
        public string New { get; internal set; }

        public UserNickChangedEventArgs(string oldNick, string newNick)
        {
            Old = oldNick;
            New = newNick;
        }
    }

    public class StringEventArgs : EventArgs
    {
        public string Result { get; internal set; }

        public StringEventArgs(string s)
        {
            Result = s;
        }

        public override string ToString()
        {
            return Result;
        }
    }

    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; internal set; }

        public ExceptionEventArgs(Exception x)
        {
            Exception = x;
        }

        public override string ToString()
        {
            return Exception.ToString();
        }
    }

    public class ModeSetEventArgs : EventArgs
    {
        public string Channel { get; internal set; }
        public string From { get; internal set; }
        public string To { get; internal set; }
        public string Mode { get; internal set; }

        public ModeSetEventArgs(string channel, string from, string to, string mode)
        {
            Channel = channel;
            From = from;
            To = to;
            Mode = mode;
        }
    }
}