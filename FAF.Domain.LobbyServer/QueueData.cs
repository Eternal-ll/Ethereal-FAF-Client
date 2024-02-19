using System;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class QueueData : Base.ServerMessage
    {
        [JsonPropertyName("queue_name")]
        public string QueueName { get; set; }
        [JsonPropertyName("queue_pop_time")]
        public DateTimeOffset QueuePopTime { get; set; }
        /// <summary>
        /// Seconds to auto-match
        /// </summary>
        [JsonPropertyName("queue_pop_time_delta")]
        public double QueuePopTimeDelta { get; set; }
        [JsonPropertyName("num_players")]
        public int NumPlayers { get; set; }
        [JsonPropertyName("team_size")]
        public int TeamSize { get; set; }
    }
}
