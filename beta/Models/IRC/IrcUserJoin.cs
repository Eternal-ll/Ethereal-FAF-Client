namespace beta.Models.IRC
{
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
}
