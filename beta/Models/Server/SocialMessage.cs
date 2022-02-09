using beta.Models.Server.Base;

namespace beta.Models.Server
{
    public struct SocialMessage : IServerMessage
    {
        //[JsonPropertyName("autojoin")]
        public string[] autojoin { get; set; }
        public string[] channels { get; set; }
        public int[] friends { get; set; }
        public int[] foes { get; set; }
        public int power { get; set; }
        public string command { get; set; }
    }
}
