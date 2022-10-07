using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public enum SearchInfoState : int
    {
        Start,
        Stop
    }
    public class MatchmakingData : Base.ServerMessage
    {
        [JsonPropertyName("queues")]
        public QueueData[] Queues { get; set; }
    }
}
