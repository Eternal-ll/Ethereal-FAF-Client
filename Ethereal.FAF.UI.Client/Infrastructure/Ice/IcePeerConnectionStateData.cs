using Ethereal.FAF.UI.Client.Infrastructure.Ice.Base;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Infrastructure.Ice
{
    public class IcePeerConnectionStateData : IceData
    {

        [JsonPropertyName("connected")]
        public bool IsConnected { get; set; }
    }
}
