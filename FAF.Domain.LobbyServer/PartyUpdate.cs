using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class PartyUpdate : Base.ServerMessage
    {
        [JsonPropertyName("owner")]
        public long OwnerId { get; set; }

        [JsonPropertyName("members")]
        public PartyMember[] Members { get; set; }
    }
}
