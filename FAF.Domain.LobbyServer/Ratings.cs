using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public partial class Ratings
    {
        [JsonPropertyName("global")]
        public Rating Global { get; set; }

        [JsonPropertyName("ladder_1v1")]
        public Rating Ladder1V1 { get; set; }

        [JsonPropertyName("tmm_2v2")]
        public Rating Tmm2V2 { get; set; }

        [JsonPropertyName("tmm_4v4_full_share")]
        public Rating Tmm4V4FullShare { get; set; }

        [JsonPropertyName("tmm_4v4_share_until_death")]
        public Rating Tmm4V4ShareUntilDeath { get; set; }
    }
}
