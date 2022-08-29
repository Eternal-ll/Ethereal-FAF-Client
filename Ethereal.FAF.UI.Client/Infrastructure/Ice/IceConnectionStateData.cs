using Ethereal.FAF.UI.Client.Infrastructure.Ice.Base;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    public class IceConnectionStateData : IceData
    {
        [JsonPropertyName("state")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public IceState State { get; set; }
    }
}
