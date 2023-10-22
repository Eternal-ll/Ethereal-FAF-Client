using FAF.Domain.LobbyServer.Base;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
	public class LobbyPlayers : ServerMessage
    {
        [JsonPropertyName("players")]
        public PlayerInfoMessage[] Players { get; set; }
    }
}
