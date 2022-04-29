namespace beta.Models.IRC
{
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
}
