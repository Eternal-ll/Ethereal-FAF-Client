using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public class SocialMessage : IServerMessage
    {
        public ServerCommand Command { get; set; }
        [JsonPropertyName("autojoin")]
        public string[] autojoin { get; set; }
        public string[] channels { get; set; }
        public List<int> friends { get; set; }
        public List<int> foes { get; set; }
        public int power { get; set; }
        public string command { get; set; }
    }
}
