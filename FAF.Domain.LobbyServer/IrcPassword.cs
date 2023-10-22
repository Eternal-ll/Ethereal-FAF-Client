using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
	public class IrcPassword : Base.ServerMessage
	{
		[JsonPropertyName("irc_password")]
		public string Password { get; set; }
	}
}
