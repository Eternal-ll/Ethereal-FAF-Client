using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class MatchMakerData : Base.ServerMessage
    {
        [JsonPropertyName("queues")]
        public QueueData[] Queues { get; set; }
    }
}
