using FAF.Domain.LobbyServer.Enums;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class MatchFound : Base.ServerMessage
    {
        [JsonPropertyName("queue_name")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MatchmakingType Queue { get; set; }
        [JsonPropertyName("game_id")]
        public long GameId { get; set; }
    }
}
