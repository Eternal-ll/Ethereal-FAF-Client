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
        public string Name => Type switch
        {
            MatchMakerType.ladder1v1 => "1v1",
            MatchMakerType.tmm2v2 => "2v2",
            MatchMakerType.tmm4v4_full_share or
            MatchMakerType.tmm4v4_share_until_death => "4v4",
            _ => Type.ToString(),
        };
        public string Mode => Type switch
        {
            MatchMakerType.ladder1v1 or
            MatchMakerType.tmm2v2 or
            MatchMakerType.tmm4v4_share_until_death => "Share until death",
            MatchMakerType.tmm4v4_full_share => "Full share",
            _ => "Unknown"
        };
        public string queue_pop_time { get; set; }
        public double queue_pop_time_delta { get; set; }

        [JsonPropertyName("num_players")]
        public int CountInQueue { get; set; }
        public int team_size { get; set; }
        public int[][] boundary_80s { get; set; }
        public int[][] boundary_75s { get; set; }

        public DateTime Updated { get; }
        public QueueData()
        {
            Updated = DateTime.Now;
        }
    }
}
