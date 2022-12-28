using System.Text.Json.Serialization;

namespace FAF.Domain.Direct.Models
{
    public class MediaSizes
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("medium")]
        public MediaSizeDetail Medium { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("thumbnail")]
        public MediaSizeDetail Thumbnail { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("full")]
        public MediaSizeDetail Full { get; set; }
    }
}
