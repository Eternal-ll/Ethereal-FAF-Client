using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class MatchFound : Base.ServerMessage
    {
        [JsonPropertyName("queue_name")]
        public string Queue { get; set; }
        [JsonPropertyName("game_id")]
        public long GameId { get; set; }
    }
}
