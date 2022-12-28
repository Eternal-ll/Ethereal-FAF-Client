using Ethereal.FAF.UI.Client.Infrastructure.Ice.Base;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    internal class IceMessage : IceData
    {
        [JsonPropertyName("msg")]
        public string Message { get; set; }
    }
}
