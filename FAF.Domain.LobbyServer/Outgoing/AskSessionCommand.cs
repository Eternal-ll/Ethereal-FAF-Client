using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer.Outgoing
{
    /// <summary>
    /// 
    /// </summary>
    public class AskSessionCommand : OutgoingCommand
    {
        public AskSessionCommand(string userAgent, string agentVersion) : base("ask_session")
        {
            UserAgent = userAgent;
            AgentVersion = agentVersion;
        }
        [JsonPropertyName("user_agent")]
        public string UserAgent { get; }
        [JsonPropertyName("version")]
        public string AgentVersion { get; }
    }
}
