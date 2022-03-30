using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public class QueueData : IServerMessage
    {
        public ServerCommand Command { get; set; }


        [JsonPropertyName("queue_name")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MatchMakerType Type { get; set; }
        public string Name
        {
            get => Type switch
            {
                MatchMakerType.ladder1v1 => throw new System.NotImplementedException(),
                MatchMakerType.tmm2v2 => throw new System.NotImplementedException(),
                MatchMakerType.tmm4v4_full_share => throw new System.NotImplementedException(),
                MatchMakerType.tmm4v4_share_until_death => throw new System.NotImplementedException(),
                _ => throw new System.NotImplementedException(),
            };
        }
        public string queue_pop_time { get; set; }
        public double queue_pop_time_delta { get; set; }
        public int num_players { get; set; }
        public int team_size { get; set; }
        public int[][] boundary_80s { get; set; }
        public int[][] boundary_75s { get; set; }
    }
}
