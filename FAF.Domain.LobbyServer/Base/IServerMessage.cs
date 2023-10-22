using FAF.Domain.LobbyServer.Enums;

namespace FAF.Domain.LobbyServer.Base
{
	public interface IServerMessage
    {
        //public string command { get; set; }

        //[JsonPropertyName("command")]
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        public ServerCommand Command { get; set; }
    }
}
