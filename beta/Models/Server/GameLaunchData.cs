using beta.Models.Server.Enums;
using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public class GameLaunchData : Base.IServerMessage
    {
        public ServerCommand Command { get; set; }

        [JsonPropertyName("args")]
        public object[] args { get; set; }
        public long uid { get; set; }
        public FeaturedMod mod { get; set; }
        public string name { get; set; }
        public int init_mode { get; set; }
        public GameType game_type { get; set; }
        public RatingType rating_type { get; set; }
    }
}
