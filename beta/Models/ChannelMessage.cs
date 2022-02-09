using System;

namespace beta.Models
{
    public struct ChannelMessage
    {
        public string From { get; set; }
        public string Message { get; set; }
        public bool HasMention { get; set; }

        private DateTime? _Created;
        public DateTime Created => _Created ??= DateTime.Now;
    }
}
