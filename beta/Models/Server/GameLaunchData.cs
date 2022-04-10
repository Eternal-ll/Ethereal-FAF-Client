using beta.Models.Server.Enums;
using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public class GameLaunchData : Base.ServerMessage
    {
        [JsonPropertyName("args")]
        public object[] args { get; set; }
        public long uid { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FeaturedMod mod { get; set; }
        public string name { get; set; }
        public int init_mode { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType game_type { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RatingType rating_type { get; set; }
    }
}
