using System;

namespace beta.Models.IRC.Base
{
    public abstract class IrcMessage
    {
        public string Text { get; }
        public DateTime Created { get; } = DateTime.Now;
        protected IrcMessage(string text) => Text = text;
    }
}
