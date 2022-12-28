using FAF.Domain.LobbyServer.Enums;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class PartyMember
    {
        [JsonPropertyName("player")]
        public long PlayerId { get; set; }

        [JsonPropertyName("factions")]
        public Faction[] Factions { get; set; }
    }
}
