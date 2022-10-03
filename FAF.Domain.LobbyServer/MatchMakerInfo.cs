using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class MatchmakingData : Base.ServerMessage
    {
        [JsonPropertyName("queues")]
        public QueueData[] Queues { get; set; }
    }
}
