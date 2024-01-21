using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Models.Settings
{
    public class RemoteClientConfiguration
    {
        [JsonPropertyName("files")]
        public RemoteFilesConfiguration Files { get; set; }
    }
    public class RemoteFilesConfiguration
    {
        [JsonPropertyName("faf-uid")]
        public string FafUidUrl { get; set; }
        [JsonPropertyName("faf-ice-adapter")]
        public string FafIceAdapterUrl { get; set; }
        [JsonPropertyName("java-runtime")]
        public string JavaRuntimeUrl { get; set; }
    }
}
