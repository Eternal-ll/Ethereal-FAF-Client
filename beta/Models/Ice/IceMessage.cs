using beta.Models.Ice.Base;
using System.Text.Json.Serialization;

namespace beta.Models.Ice
{
    internal class IceMessage : IceData
    {
        [JsonPropertyName("msg")]
        public string Message { get; set; }
    }
}
