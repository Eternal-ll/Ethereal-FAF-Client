using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.Attributes
{
	public class CoturnServerAttributes
	{
		[JsonPropertyName("active")]
		public bool Active { get; set; }

		[JsonPropertyName("credential")]
		public string Credential { get; set; }

		[JsonPropertyName("credentialType")]
		public string CredentialType { get; set; }

		[JsonPropertyName("region")]
		public string Region { get; set; }

		[JsonPropertyName("urls")]
		public string[] Urls { get; set; }

		[JsonPropertyName("username")]
		public string Username { get; set; }
	}
}
