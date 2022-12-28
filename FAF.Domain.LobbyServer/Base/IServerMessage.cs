using FAF.Domain.LobbyServer.Enums;
using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer.Base
{
    public interface IServerMessage
    {
        //public string command { get; set; }

        //[JsonPropertyName("command")]
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        public ServerCommand Command { get; set; }
    }
    public abstract class ServerMessage : IServerMessage
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("command")]
        public ServerCommand Command { get; set; }
    }
}
