using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer.Outgoing
{
    public class JoinGameCommand : OutgoingCommand
    {
        public JoinGameCommand(long uid, string password = null, int gamePort = 0) : base("game_join")
        {
            Uid = uid;
            Password = password;
            GamePort = gamePort;
        }

        [JsonPropertyName("uid")]
        public long Uid { get; }
        [JsonPropertyName("password")]
        public string Password { get; }
        [JsonPropertyName("gameport")]
        public int GamePort { get; }
    }
}
