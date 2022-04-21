using System;

namespace beta.Models.IRC
{
    public class IrcChannelMessage : Base.IrcMessage
    {
        public string Channel { get; }
        public string From { get; }
        public bool HasMention { get; }
        public bool IsSame { get; set; }
        public IrcChannelMessage(string channel, string from, string text) : base(text)
        {
            Channel = channel;
            From = from;
            // TODO Is it worth it? XD
            HasMention = text.Contains(Properties.Settings.Default.PlayerNick, StringComparison.OrdinalIgnoreCase);
        }
    }
    public class IrcPrivateMessage : Base.IrcMessage
    {
        public string From { get; }
        // TODO 
        //public bool HasMention { get; }
        public IrcPrivateMessage(string from, string text) : base(text) => From = from;
    }

    public class IrcNotificationMessage : Base.IrcMessage
    {
        public IrcNotificationMessage(string text) : base(text) { }
    }

    public class IrcUserJoin
    {
        public string Channel { get; }
        public string User { get; }
        public IrcUserJoin(string channel, string user)
        {
            Channel = channel;
            User = user;
        }
    }
    public class IrcUserLeft
    {
        public string Channel { get; }
        public string User { get; }
        public IrcUserLeft(string channel, string user)
        {
            Channel = channel;
            User = user;
        }
    }

    public class IrcChannelTopicUpdated
    {
        public string Channel { get; }
        public string Topic { get; }

        public IrcChannelTopicUpdated(string channel, string topic)
        {
            Channel = channel;
            Topic = topic;
        }
    }
    public class IrcChannelTopicChangedBy
    {
        public string Channel { get; }
        public string By { get; }
        public DateTime At { get; }

        public IrcChannelTopicChangedBy(string channel, string by, string at)
        {
            Channel = channel;
            By = by;
            At = DateTime.UnixEpoch.AddSeconds(double.Parse(at));
        }
    }

    public class IrcChannelUsers
    {
        public string Channel { get; }
        public string[] Users { get; }
        public IrcChannelUsers(string channel, string[] users)
        {
            Channel = channel;
            Users = users;
        }
    }
    public class IrcUserChangedName
    {
        public string From { get; }
        public string To { get; }
        public IrcUserChangedName(string from, string to)
        {
            From = from;
            To = to;
        }
    }
}
