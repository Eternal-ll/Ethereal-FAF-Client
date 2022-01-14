using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace beta.Models
{
    public class Irc
    {
        #region Variables
        NetworkStream stream;
        TcpClient irc;
        StreamWriter Sender;
        private StreamReader Reader;
        string nick = null;
        PingSender pingSender;
 
        /// <summary>
        /// Адрес IRC-сервера
        /// </summary>
        public string Server { get; set; }
 
        /// <summary>
        /// Порт IRC-сервера
        /// </summary>
        public int Port { get; set; }
 
        /// <summary>
        /// Ник бота
        /// </summary>
        public string Nick
        {
            get
            {
                return this.nick;
            }
            set
            {
                this.nick = value;
                if (this.Connected)
                {
                    this.SendMessage("NICK " + this.nick);
                }
            }
        }
 
        /// <summary>
        /// Имя пользователя и Реальное имя
        /// </summary>
        public string UserName { get; set; }
 
        /// <summary>
        /// Проверка подключения к серверу
        /// </summary>
        public bool Connected { get; set; }
 
        /// <summary>
        /// Исключать в выводе Pong
        /// </summary>
        public bool RemovePongs { get; set; }
 
        /// <summary>
        /// Каналы для подключения
        /// </summary>
        private List<string> _channels = new List<string>();
 
        /// <summary>
        /// Каналы для автозахода
        /// </summary>
        public List<string> Channels { get { return _channels; } set { _channels = value; } }
 
        private List<string> _sendafterconnection = new List<string>();
 
        /// <summary>
        /// Сообщения для отправки после подключения
        /// </summary>
        public List<string> MessagesForSendAfterConnection { get { return _sendafterconnection; } set { _sendafterconnection = value; } }
        #endregion
 
        /// <summary>
        /// Инициализация экземпляра класса Irc
        /// </summary>
        /// <param name="Server">Адрес Irc-сервера</param>
        /// <param name="Port">Порт Irc-сервера</param>
        /// <param name="Nick">Ник Irc-пользователя</param>
        /// <param name="UserName">Реальное имя пользователя и идентификатор в формате "USER Идентификатор 8 * :Реальное_имя"</param>
        public Irc(string Server, int Port, string Nick, string UserName)
        {
            this.Server = Server;
            this.Port = Port;
            this.Nick = Nick;
            this.UserName = UserName;
        }
 
        /// <summary>
        /// Открыть соединение
        /// </summary>
        public void Open()
        {
            irc = new TcpClient(this.Server, this.Port);
            stream = irc.GetStream();
            Sender = new StreamWriter(stream, Encoding.Default);
            Reader = new StreamReader(stream, Encoding.Default);
            pingSender = new PingSender(this.Server, this.Sender);
            pingSender.Start();
            this.SendMessage(this.UserName);
            this.SendMessage("NICK " + this.Nick);
            this.Connected = true;
            for (int i = 0; i < this._sendafterconnection.Count; i++)
            {
                this.SendMessage(this._sendafterconnection[i]);
            }
            for (int i = 0; i < this._channels.Count; i++)
            {
                this.JoinChannel(this._channels[i]);
            }
            this.SendMessage("MODE " + this.Nick + " :+B");
        }
 
        /// <summary>
        /// Закрыть соединение
        /// </summary>
        public void Close()
        {
            SendMessage("QUIT");
            pingSender.Stop();
            irc.Close();
            Sender.Close();
            Reader.Close();
            stream.Close();
            this.Connected = false;
        }
 
        /// <summary>
        /// Отправить сообщение серверу
        /// </summary>
        /// <param name="IRCMessage">Сообщение</param>
        public void SendMessage(string IRCMessage)
        {
            for (int i = 0; i < IRCMessage.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Length; i++)
            {
                Sender.WriteLine(IRCMessage.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)[i]);
                Sender.Flush();
            }
        }
 
        /// <summary>
        /// Полученное сообщение от сервера
        /// </summary>
        public IRCMessage ReturnedMessage
        {
            get
            {
                IRCMessage retmsg = new IRCMessage();
                if (Connected)
                {
                    if (RemovePongs)
                    {
                        retmsg.RawMessage = "";
                        do
                        {
                            retmsg.RawMessage = Reader.ReadLine();
                        }
                        while (retmsg.RawMessage.Contains(this.Server) && retmsg.RawMessage.Contains("PONG"));
                        return retmsg;
                    }
                    retmsg.RawMessage = Reader.ReadLine();
                    return retmsg;
                }
                else
                    return null;
            }
        }
 
        /// <summary>
        /// Заход на канал
        /// </summary>
        /// <param name="Channel"></param>
        public void JoinChannel(string Channel)
        {
            if (!Channels.Contains(Channel))
                Channels.Add(Channel);
            if (Connected)
                SendMessage("JOIN " + Channel);
        }
 
        /// <summary>
        /// Заход на каналы
        /// </summary>
        /// <param name="Channels">Массив каналов</param>
        public void JoinChannels(string[] Channels)
        {
            for (int i = 0; i < Channels.Length; ++i)
            {
                JoinChannel(Channels[i]);
            }
        }
    }
    partial class PingSender
    {
        #region Variables
        string PING = "PING :";
        string Server = null;
        private Thread pingSender;
        private StreamWriter Sender;
        #endregion
        public PingSender(string Server, StreamWriter Sender)
        {
            this.Sender = Sender;
            this.Server = Server;
            pingSender = new Thread(new ThreadStart(this.Run));
        }
        public void Start()
        {
            pingSender.Start();
        }
        public void Stop()
        {
            pingSender.Abort();
        }
        public void Run()
        {
            while (true)
            {
                try
                {
                    Sender.WriteLine(PING + this.Server);
                    Sender.Flush();
                    Thread.Sleep(15000);
                }
                catch { break; }
            }
        }
    }
    public class IRCMessage
    {
        #region Variables
        private string _rawMessage = "";
        private string _nick = "";
        private string _newnick = "";
        private string _kickednick = "";
        private string _kickreason = "";
        private string _quitmessage = "";
        private string _textmessage = "";
        private string _host = "";
        private string _ident = "";
        private string _command = "";
        private string _mode = "";
        private string _channel = "";
        private string _bans = "";
        private string _accessusers = "";
        private DateTime _dateandtime = DateTime.Now;
        #endregion
 
        /// <summary>
        /// Raw-сообщение
        /// </summary>
        public string RawMessage { get { return this._rawMessage; } set { _rawMessage = value; ParseMessage(); } }
 
        /// <summary>
        /// Ник
        /// </summary>
        public string Nick { get { return _nick; } }
 
        /// <summary>
        /// Новый ник
        /// </summary>
        public string NewNick { get { return _newnick; } }
 
        /// <summary>
        /// Ник кикнутого
        /// </summary>
        public string KickedNick { get { return _kickednick; } }
 
        /// <summary>
        /// Причина кика
        /// </summary>
        public string KickReason { get { return _kickreason; } }
 
        /// <summary>
        /// Текст выходного сообщения
        /// </summary>
        public string QuitMessage { get { return _quitmessage; } }
 
        /// <summary>
        /// Канал
        /// </summary>
        public string Channel { get { return _channel; } }
 
        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string TextMessage { get { return _textmessage; } }
 
        /// <summary>
        /// Хост пользователя
        /// </summary>
        public string Host { get { return _host; } }
 
        /// <summary>
        /// Список забаненных\разбаненных при режимах +b или -b
        /// </summary>
        public string Bans { get { return _bans; } }
 
        /// <summary>
        /// Список пользователей, который получили\лишились прав при режимах v, h, o, a, q.
        /// </summary>
        public string AccessUsers { get { return _accessusers; } }
 
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public string Identificator { get { return _ident; } }
 
        /// <summary>
        /// Команда
        /// </summary>
        public string Command { get { return _command; } }
 
        /// <summary>
        /// Новый режим
        /// </summary>
        public string Mode { get { return _mode; } }
 
        /// <summary>
        /// Дата и время сообщения
        /// </summary>
        public DateTime DateAndTime { get { return _dateandtime; } }
 
        /// <summary>
        /// Парсинг сообщения
        /// </summary>
        private void ParseMessage()
        {
            try
            {
                this._command = this.RawMessage.Split(' ')[1];
                switch (this._command)
                {
                    case "PRIVMSG":
                        this._nick = this.RawMessage.Split(':')[1].Split('!')[0];
                        this._host = this.RawMessage.Split('@')[1].Split(' ')[0];
                        this._ident = this.RawMessage.Split('!')[1].Split(' ')[0];
                        this._channel = this.RawMessage.Split(' ')[2];
                        for (int i = 2; i < this.RawMessage.Split(':').Length; i++)
                        {
                            this._textmessage += this.RawMessage.Split(':')[i] + ":";
                        }
                        this._textmessage = this._textmessage.Remove(this._textmessage.Length - 1, 1);
                        if (this._textmessage.StartsWith("ACTION "))
                        {
                            this._textmessage = this._textmessage.Replace("ACTION ", "").Replace("", "");
                            this._command = "ACTION";
                        }
                        break;
                    case "NOTICE":
                        this._nick = this.RawMessage.Split(':')[1].Split('!')[0];
                        this._host = this.RawMessage.Split('@')[1].Split(' ')[0];
                        this._ident = this.RawMessage.Split('!')[1].Split(' ')[0];
                        for (int i = 2; i < this.RawMessage.Split(':').Length; i++)
                        {
                            this._textmessage += this.RawMessage.Split(':')[i] + ":";
                        }
                        this._textmessage = this._textmessage.Remove(this._textmessage.Length - 1, 1);
                        break;
                    case "JOIN":
                        this._nick = this.RawMessage.Split(':')[1].Split('!')[0];
                        this._host = this.RawMessage.Split('@')[1].Split(' ')[0];
                        this._ident = this.RawMessage.Split('!')[1].Split(' ')[0];
                        this._channel = this.RawMessage.Split(':')[2];
                        break;
                    case "NICK":
                        this._nick = this.RawMessage.Split(':')[1].Split('!')[0];
                        this._host = this.RawMessage.Split('@')[1].Split(' ')[0];
                        this._ident = this.RawMessage.Split('!')[1].Split(' ')[0];
                        this._newnick = this.RawMessage.Split(':')[2];
                        break;
                    case "QUIT":
                        this._nick = this.RawMessage.Split(':')[1].Split('!')[0];
                        this._host = this.RawMessage.Split('@')[1].Split(' ')[0];
                        this._ident = this.RawMessage.Split('!')[1].Split(' ')[0];
                        try
                        {
                            for (int i = 2; i < this.RawMessage.Split(':').Length; i++)
                            {
                                this._quitmessage += this.RawMessage.Split(':')[i] + ":";
                            }
                            this._quitmessage = this._quitmessage.Remove(this._quitmessage.Length - 1, 1);
                        }
                        catch { }
                        break;
                    case "PART":
                        this._nick = this.RawMessage.Split(':')[1].Split('!')[0];
                        this._host = this.RawMessage.Split('@')[1].Split(' ')[0];
                        this._ident = this.RawMessage.Split('!')[1].Split(' ')[0];
                        this._channel = this.RawMessage.Split(' ')[2];
                        try
                        {
                            for (int i = 2; i < this.RawMessage.Split(':').Length; i++)
                            {
                                this._quitmessage += this.RawMessage.Split(':')[i] + ":";
                            }
                            this._quitmessage = this._quitmessage.Remove(this._quitmessage.Length - 1, 1);
                        }
                        catch { }
                        break;
                    case "KICK":
                        this._nick = this.RawMessage.Split(':')[1].Split('!')[0];
                        this._host = this.RawMessage.Split('@')[1].Split(' ')[0];
                        this._ident = this.RawMessage.Split('!')[1].Split(' ')[0];
                        this._channel = this.RawMessage.Split(' ')[2];
                        this._kickednick = this.RawMessage.Split(' ')[3];
                        for (int i = 2; i < this.RawMessage.Split(':').Length; i++)
                        {
                            this._kickreason += this.RawMessage.Split(':')[i] + ":";
                        }
                        this._kickreason = this._kickreason.Remove(this._kickreason.Length - 1, 1);
                        break;
                    case "MODE":
                        this._nick = this.RawMessage.Split(':')[1].Split('!')[0];
                        this._host = this.RawMessage.Split('@')[1].Split(' ')[0];
                        this._ident = this.RawMessage.Split('!')[1].Split(' ')[0];
                        this._channel = this.RawMessage.Split(' ')[2];
                        this._mode = this.RawMessage.Split(' ')[3];
                        if (this.Mode.Contains("b"))
                        {
                            for (int i = 4; i < this.RawMessage.Split(' ').Length; i++)
                            {
                                this._bans += this.RawMessage.Split(' ')[i] + " ";
                            }
                            this._bans = this._bans.Remove(this._bans.Length - 1, 1);
                        }
                        if (this.Mode.Contains("v") || this.Mode.Contains("h") || this.Mode.Contains("o") || this.Mode.Contains("a") || this.Mode.Contains("q"))
                        {
                            for (int i = 4; i < this.RawMessage.Split(' ').Length; i++)
                            {
                                this._accessusers += this.RawMessage.Split(' ')[i] + " ";
                            }
                            this._accessusers = this._accessusers.Remove(this._accessusers.Length - 1, 1);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch { }
        }
 
        /// <summary>
        /// Конвертированное в читабельный вид сообщение
        /// </summary>
        public string ConvertedMessage { get { return ConvMessage(); } }
 
        /// <summary>
        /// Функция преобразования сообщения в читаемый вид
        /// </summary>
        /// <returns>Возвращает конвертированную строку.</returns>
        private string ConvMessage()
        {
            string ConvertedMessage = "";
            switch (this.Command)
            {
                case "PRIVMSG":
                    ConvertedMessage = String.Format("[{0}] {1} <{2}> {3}", DateAndTime.ToString("dd.MM.yyyy") + " " + DateAndTime.ToLongTimeString(), this.Channel, this.Nick, this.TextMessage);
                    break;
                case "ACTION":
                    ConvertedMessage = String.Format("[{0}] {1} * <{2}> {3}", DateAndTime.ToString("dd.MM.yyyy") + " " + DateAndTime.ToLongTimeString(), this.Channel, this.Nick, this.TextMessage);
                    break;
                case "NOTICE":
                    ConvertedMessage = String.Format("[{0}] -{1}-: {2}", DateAndTime.ToString("dd.MM.yyyy") + " " + DateAndTime.ToLongTimeString(), this.Nick, this.TextMessage);
                    break;
                case "JOIN":
                    ConvertedMessage = String.Format("[{0}] {1} ({2}) зашел на канал {3}.", DateAndTime.ToString("dd.MM.yyyy") + " " + DateAndTime.ToLongTimeString(), this.Nick, this.Identificator, this.Channel);
                    break;
                case "PART":
                    if (this.QuitMessage == "")
                        ConvertedMessage = String.Format("[{0}] {1} ({2}) покинул канал {3}.", DateAndTime.ToString("dd.MM.yyyy") + " " + DateAndTime.ToLongTimeString(), this.Nick, this.Identificator, this.Channel);
                    else
                        ConvertedMessage = String.Format("[{0}] {1} ({2}) покинул канал {3} ({4}).", DateAndTime.ToString("dd.MM.yyyy") + " " + DateAndTime.ToLongTimeString(), this.Nick, this.Identificator, this.Channel, this.QuitMessage);
                    break;
                case "QUIT":
                    if (this.QuitMessage == "")
                        ConvertedMessage = String.Format("[{0}] {1} ({2}) вышел.", DateAndTime.ToString("dd.MM.yyyy") + " " + DateAndTime.ToLongTimeString(), this.Nick, this.Identificator);
                    else
                        ConvertedMessage = String.Format("[{0}] {1} ({2}) вышел ({3}).", DateAndTime.ToString("dd.MM.yyyy") + " " + DateAndTime.ToLongTimeString(), this.Nick, this.Identificator, this.QuitMessage);
                    break;
                case "KICK":
                    ConvertedMessage = String.Format("[{0}] {1} кикнул пользователя {2} ({3}).", DateAndTime.ToString("dd.MM.yyyy") + " " + DateAndTime.ToLongTimeString(), this.Nick, this.KickedNick, this.KickReason);
                    break;
                case "MODE":
                    if (this.Bans == "" && this.AccessUsers == "")
                        ConvertedMessage = String.Format("[{0}] {1} меняет режим на {2}.", DateAndTime.ToString("dd.MM.yyyy") + " " + DateAndTime.ToLongTimeString(), this.Nick, this.Mode);
                    else if (this.Bans != "")
                        ConvertedMessage = String.Format("[{0}] {1} меняет режим на {2} {3}.", DateAndTime.ToString("dd.MM.yyyy") + " " + DateAndTime.ToLongTimeString(), this.Nick, this.Mode, this.Bans);
                    else if (this.AccessUsers != "")
                        ConvertedMessage = String.Format("[{0}] {1} меняет режим на {2} {3}.", DateAndTime.ToString("dd.MM.yyyy") + " " + DateAndTime.ToLongTimeString(), this.Nick, this.Mode, this.AccessUsers);
                    break;
                case "NICK":
                    ConvertedMessage = String.Format("[{0}] {1} сменил ник на {2}.", DateAndTime.ToString("dd.MM.yyyy") + " " + DateAndTime.ToLongTimeString(), this.Nick, this.NewNick);
                    break;
                default:
                    ConvertedMessage = RawMessage;
                    break;
            }
            return ConvertedMessage;
        }
    }
}