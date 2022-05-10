namespace beta.Models.IRC
{
    public class IrcChannelTopicUpdated
    {
        public string Channel { get; }
        public string Topic { get; }
        public string By { get; }

        public IrcChannelTopicUpdated(string channel, string topic, string by)
        {
            Channel = channel;
            Topic = topic;
            By = by;
        }
        public override string ToString() => string.IsNullOrWhiteSpace(Topic)?
            $"Topic removed by {By}":
            $"Topic changed by {By}";

    }
}
