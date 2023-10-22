using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
	public class IceServersData : Base.ServerMessage
    {
        public IceCoturnServer[] ice_servers { get; set; }
        public int ttl { get; set; }
    }

    public class IceCoturnServer
	{
		[JsonPropertyName("id")]
		public int Id { get; set; }
        [JsonPropertyName("urls")]
        public string[] Urls { get; set; }
        [JsonPropertyName("credential")]
        public string Credential { get; set; }
        [JsonPropertyName("credentialType")]
        public string CredentialType { get; set; }
		[JsonPropertyName("active")]
		public bool Active { get; set; }
		[JsonPropertyName("region")]
		public string Region { get; set; }
		[JsonPropertyName("username")]
		public string Username { get; set; }
    }
}
