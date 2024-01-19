using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Models.Update
{
    /// <summary>
    /// Client update channel
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UpdateChannel
    {
        Unknown,
        Stable,
        Preview,
        Development
    }
}
