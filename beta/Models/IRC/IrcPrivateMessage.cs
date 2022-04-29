namespace beta.Models.IRC
{
    public class IrcPrivateMessage : Base.IrcMessage
    {
        public string From { get; }
        // TODO 
        //public bool HasMention { get; }
        public IrcPrivateMessage(string from, string text) : base(text) => From = from;
    }
}
