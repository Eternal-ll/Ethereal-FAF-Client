using FAF.Domain.LobbyServer.Enums;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer.Base
{
	public abstract class ServerMessage : IServerMessage
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("command")]
        public ServerCommand Command { get; set; }
    }
}
