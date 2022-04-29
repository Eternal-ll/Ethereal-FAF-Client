using beta.Models.Ice.Base;
using System.Text.Json.Serialization;

namespace beta.Models.Ice
{
    internal class IceConnectionStateData : IceData
    {
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public IceState State { get; set; }
    }
}
