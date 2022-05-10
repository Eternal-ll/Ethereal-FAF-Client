using System;

namespace beta.Models.IRC
{
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
        public override string ToString() => $"Topic changed by {By} at {At.ToShortTimeString()}";
    }
}
