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
}
