namespace beta.Models.IRC
{
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
}
