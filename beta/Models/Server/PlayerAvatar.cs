using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public class PlayerAvatar
    {
        [JsonPropertyName("url")]
        public string UrlSource { get; set; }

        [JsonPropertyName("tooltip")]
        public string Tooltip { get; set; }
    }
}
