using FAF.Domain.LobbyServer.Base;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    /// <summary>
    /// Lobby notification message
    /// </summary>
    public class Notification : ServerMessage
    {
        /// <summary>
        /// Style
        /// </summary>
        [JsonPropertyName("style")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Enums.NotificationType Style { get; set; }
        /// <summary>
        /// Message
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
