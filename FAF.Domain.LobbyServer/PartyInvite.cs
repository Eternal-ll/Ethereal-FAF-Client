using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
	public class PartyInvite : Base.ServerMessage
    {
        [JsonPropertyName("sender")]
        public long SenderId { get; set; }
    }
}
