using System;

namespace beta.Models.Emoji
{
    public abstract class EmojiCache
    {
        public string Name { get; set; }
        public Uri PathToFile { get; set; }
    }
}
