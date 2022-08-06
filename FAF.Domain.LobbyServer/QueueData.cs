using FAF.Domain.LobbyServer.Enums;
using System;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class QueueData
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServerCommand Command { get; set; }
        [JsonPropertyName("queue_name")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MatchMakerType Type { get; set; }
        [JsonPropertyName("queue_pop_time")]
        private string _queue_pop_time;
        public string queue_pop_time { get; set; }
        /// <summary>
        /// Seconds to auto-match
        /// </summary>
        public double queue_pop_time_delta { get; set; }
        [JsonPropertyName("num_players")]
        public int CountInQueue { get; set; }
        public int team_size { get; set; }
        public int[][] boundary_80s { get; set; }
        public int[][] boundary_75s { get; set; }
    }
}
