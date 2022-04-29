using beta.Infrastructure.Converters.JSON;
using System.Text.Json.Serialization;

namespace beta.Models.Server
{
    public class IceServersData : Base.ServerMessage
    {
        [JsonConverter(typeof(RawStringConverter))]
        public string ice_servers { get; set; }
        public int ttl { get; set; }
    }
}
