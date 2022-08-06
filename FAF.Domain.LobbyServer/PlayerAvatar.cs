using System;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class PlayerAvatar
    {
        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("tooltip")]
        public string Tooltip { get; set; }
    }
}
