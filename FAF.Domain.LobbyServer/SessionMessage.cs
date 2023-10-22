using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
	public class SessionMessage : Base.ServerMessage
    {
        [JsonPropertyName("session")]
        public long Session { get; set; }
	}
}
