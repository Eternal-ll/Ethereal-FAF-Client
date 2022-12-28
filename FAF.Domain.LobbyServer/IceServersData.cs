using System.Text.Json.Serialization;

namespace FAF.Domain.LobbyServer
{
    public class IceServersData : Base.ServerMessage
    {
        [JsonConverter(typeof(RawStringConverter))]
        public string ice_servers { get; set; }
        public int ttl { get; set; }
    }
}
