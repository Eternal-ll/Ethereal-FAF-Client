using beta.Models.Server.Base;

namespace beta.Models.Server
{
    public class SocialMessage : IServerMessage
    {
        public string[] autojoin { get; set; }
        public string[] channels { get; set; }
        public long[] friends { get; set; }
        public long[] foes { get; set; }
        public int power { get; set; }
        public string command { get; set; }
    }
}
