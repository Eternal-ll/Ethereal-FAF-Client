using FAF.Domain.LobbyServer.Enums;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class MatchCancelled : Base.ServerMessage
    {
        [JsonPropertyName("game_od")]
        public long GameId { get; set; }
        [JsonPropertyName("queue_name")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MatchmakingType Queue { get; set; }
    }
}
