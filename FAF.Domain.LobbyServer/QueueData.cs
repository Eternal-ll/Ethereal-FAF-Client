using FAF.Domain.LobbyServer.Enums;
using System;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class QueueData : Base.ServerMessage
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServerCommand Command { get; set; }
        [JsonPropertyName("queue_name")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MatchmakingType Type { get; set; }
        [JsonPropertyName("queue_pop_time")]
        private string _queue_pop_time;
        public string queue_pop_time { get; set; }
        /// <summary>
        /// Seconds to auto-match
        /// </summary>
        public double queue_pop_time_delta { get; set; }
        public TimeSpan PopTimeSpan => TimeSpan.FromSeconds(queue_pop_time_delta);
        [JsonPropertyName("num_players")]
        public int PlayersCountInQueue { get; set; }
        [JsonPropertyName("team_size")]
        public int TeamSize { get; set; }
        public int[][] boundary_80s { get; set; }
        public int[][] boundary_75s { get; set; }

        public bool IsGood(RatingType ratingType) => (int)ratingType - 1 == (int)Type;
    }
}
