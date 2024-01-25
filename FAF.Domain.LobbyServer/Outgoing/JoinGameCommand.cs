using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer.Outgoing
{
    public class JoinGameCommand : OutgoingCommand
    {
        public JoinGameCommand(long uid, int gamePort) : base("game_join")
        {
            Uid = uid;
            GamePort = gamePort;
        }

        [JsonPropertyName("uid")]
        public long Uid { get; }
        [JsonPropertyName("gameport")]
        public int GamePort { get; }
    }
}
