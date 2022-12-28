using System;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    /// <summary>
    /// TODO
    /// <see cref="https://github.com/FAForever/server/issues/607"/>
    /// <seealso cref="https://github.com/FAForever/downlords-faf-client/issues/1783"/>
    /// </summary>
    public class MatchConfirmation : Base.ServerMessage
    {
        [JsonPropertyName("expires_at")]
        public DateTime ExpiresAt { get; set; }
        public TimeSpan ExpiresAtTimeSpan => ExpiresAt - DateTime.UtcNow;
        [JsonPropertyName("players_total")]
        public int PlayersTotal { get; set; }
        [JsonPropertyName("players_ready")]
        public int PlayersReady { get; set; }
        [JsonPropertyName("ready")]
        public bool IsReady { get; set; }
    }
}
