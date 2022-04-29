using beta.Models.Ice.Base;
using System.Text.Json.Serialization;

namespace beta.Models.Ice
{
    internal class IcePeerConnectionStateData : IceData
    {

        [JsonPropertyName("connected")]
        public bool IsConnected { get; set; }
    }
}
