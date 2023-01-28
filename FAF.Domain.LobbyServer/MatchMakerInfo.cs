using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    /// <summary>
    /// State of queue search
    /// </summary>
    public enum QueueSearchState : int
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
