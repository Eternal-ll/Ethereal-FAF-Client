using FAF.Domain.Direct.Models;
using System;
using System.Text.Json.Serialization;

namespace FAF.Domain.Direct.Entities
{
    public class Media : Base.Entity
    {
        [JsonPropertyName("title")]
        public Content Title { get; set; }

        [JsonPropertyName("media_details")]
        public MediaDetails MediaDetails { get; set; }
        [JsonPropertyName("source_url")]
        public Uri SourceUrl { get; set; }
    }
}
