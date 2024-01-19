using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer.Outgoing
{
    public abstract class OutgoingCommand
    {
        public OutgoingCommand(string command) => Command = command;
        [JsonPropertyName("command")]
        public string Command { get; }
    }
}
