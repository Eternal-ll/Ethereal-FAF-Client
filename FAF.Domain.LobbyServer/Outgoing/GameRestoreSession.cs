using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer.Outgoing
{
    public class GameRestoreSession : OutgoingCommand
    {
        public GameRestoreSession(long uid) : base("restore_game_session")
        {
            GameId = uid;   
        }
        [JsonPropertyName("game_id")]
        public long GameId { get; }
    }
}
