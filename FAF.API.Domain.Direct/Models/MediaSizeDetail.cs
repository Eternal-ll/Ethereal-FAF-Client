using System;
using System.Text.Json.Serialization;

namespace FAF.Domain.Direct.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class MediaSizeDetail
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("file")]
        public string File { get; set; }
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
        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("source_url")]
        public Uri SourceUrl { get; set; }
    }
}
