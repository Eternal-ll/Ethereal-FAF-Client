using beta.Infrastructure.Converters.JSON;
using beta.Models.Server.Enums;
using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public enum GameInitMode : byte
    {
        Normal = 0,
        Auto = 1,
    }
    public class GameLaunchData : Base.ServerMessage
    {
        [JsonPropertyName("args")]
        public object[] args { get; set; }
        public long uid { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FeaturedMod mod { get; set; }
        public string name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameInitMode init_mode { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public GameType game_type { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RatingType rating_type { get; set; }
        public int team { get; set; }

        [JsonConverter(typeof (JsonStringEnumConverter))]
        public Faction faction { get; set; }
        public int expected_players { get; set; }
        public int map_position { get; set; }
        public string mapname { get; set; }
        
        [JsonConverter(typeof(RawStringConverter))]
        public string game_options { get; set; }
    }
}
