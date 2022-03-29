using beta.Models.Server.Enums;
using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public class MatchMakerData : Base.IServerMessage
    {
        public ServerCommand Command { get; set; }

        [JsonPropertyName("queues")]
        public QueueData[] Queues { get; set; }
    }
}
