using System;
using System.Text.Json.Serialization;

namespace beta.Models.API.News
{
    public class FeaturedMedia
    {
        [JsonPropertyName("source_url")]
        public Uri ImageUrl { get; set; }
    }
}
