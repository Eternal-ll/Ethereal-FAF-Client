using System.Text.Json.Serialization;

namespace FAF.Domain.Direct.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class MediaDetails
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("width")]
        public long Width { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("height")]
        public long Height { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("sizes")]
        public MediaSizes Sizes { get; set; }
    }
}
