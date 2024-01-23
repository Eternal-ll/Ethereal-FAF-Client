using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.Models.Update
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UpdateType
    {
        /// <summary>
        /// Usual update
        /// </summary>
        Normal = 1 << 0,
        /// <summary>
        /// Critical update
        /// </summary>
        Critical = 1 << 1,
        /// <summary>
        /// Mandatory update
        /// </summary>
        Mandatory = 1 << 2,
    }
}
