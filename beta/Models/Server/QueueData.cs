using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using System;
using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public class QueueData : ServerMessage
    {
        [JsonPropertyName("queue_name")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MatchMakerType Type { get; set; }
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
    public class QueueDataModel : QueueData
    {
        public string Mode => Type switch
        {
            MatchMakerType.ladder1v1 or
            MatchMakerType.tmm2v2 or
            MatchMakerType.tmm4v4_share_until_death => "Share until death",
            MatchMakerType.tmm4v4_full_share => "Full share",
            _ => "Unknown"
        };
        public string Name => Type switch
        {
            MatchMakerType.ladder1v1 => "1 vs 1",
            MatchMakerType.tmm2v2 => "2 vs 2",
            MatchMakerType.tmm4v4_full_share or
            MatchMakerType.tmm4v4_share_until_death => "4 vs 4",
            _ => Type.ToString(),
        };

        public DateTime Updated { get; }
        public QueueDataModel()
        {
            Updated = DateTime.Now;
        }
    }
}
