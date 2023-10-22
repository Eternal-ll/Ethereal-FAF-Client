using FAF.Domain.LobbyServer.Base;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
	public class LobbyGames : ServerMessage
    {
        [JsonPropertyName("games")]
        public GameInfoMessage[] Games { get; set; }
    }
}
