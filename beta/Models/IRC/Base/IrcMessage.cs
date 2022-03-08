namespace beta.Models.IRC.Base
{
    public abstract class IrcMessage
    {
        public string Text { get; }
        protected IrcMessage(string text) => Text = text;
    }
}
