using System.Text.Json.Serialization;

namespace beta.Models.Ice.Base
{
    internal abstract class IceData
    {
        [JsonPropertyName("localPlayedId")]
        public int LocalPlayedId { get; set; }

        [JsonPropertyName("remotePlayedId")]
        public int RemotePlayedId { get; set; }
    }
}
