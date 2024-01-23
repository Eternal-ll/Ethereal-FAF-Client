using System;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Models.Settings
{
    public class RemoteClientConfiguration
    {
        [JsonPropertyName("faf-uid")]
        public string FafUidUrl { get; set; }
        [JsonPropertyName("faf-ice-adapter")]
        public string FafIceAdapterUrl { get; set; }
        [JsonPropertyName("java-runtime")]
        public string JavaRuntimeUrl { get; set; }
        [JsonPropertyName("fa-debugger")]
        public string FaDebuggerUrl { get; set; }
        [JsonPropertyName("requiredFiles")]
        public RequiredFile[] RequiredFiles { get; set; } = Array.Empty<RequiredFile>();
    }
    public class RequiredFile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
