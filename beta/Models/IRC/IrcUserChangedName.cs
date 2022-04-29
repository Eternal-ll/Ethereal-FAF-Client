namespace beta.Models.IRC
{
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
