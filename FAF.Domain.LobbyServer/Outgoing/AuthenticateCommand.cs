using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer.Outgoing
{
    /// <summary>
    /// Authenticate in lobby
    /// </summary>
    public class AuthenticateCommand : OutgoingCommand
    {
        public AuthenticateCommand(string token, string uniqueId, long session) : base("auth")
        {
            Token = token;
            UniqueId = uniqueId;
            Session = session;
        }
        /// <summary>
        /// User JWT access token
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; }
        /// <summary>
        /// Generated UID for session
        /// </summary>
        [JsonPropertyName("unique_id")]
        public string UniqueId { get; }
        /// <summary>
        /// Lobby session ID
        /// </summary>
        [JsonPropertyName("session")]
        public long Session { get; }
    }
}
