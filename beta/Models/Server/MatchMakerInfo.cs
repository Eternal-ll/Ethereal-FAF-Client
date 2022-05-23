using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public class MatchMakerData : Base.ServerMessage
    {
        [JsonPropertyName("queues")]
        public QueueDataModel[] Queues { get; set; }
    }
}
