namespace beta.Models.IRC
{
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
}
