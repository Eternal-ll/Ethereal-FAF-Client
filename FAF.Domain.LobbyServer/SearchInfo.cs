using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class SearchInfo : Base.ServerMessage
    {
        [JsonPropertyName("queue_name")]
        public string Queue { get; set; }
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QueueSearchState State { get; set; }
    }
}
