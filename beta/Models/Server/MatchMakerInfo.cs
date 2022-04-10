using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public class MatchMakerData : Base.ServerMessage
    {
        [JsonPropertyName("queues")]
        public QueueData[] Queues { get; set; }
    }
}
