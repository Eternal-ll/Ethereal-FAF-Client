using System.Text.Json.Serialization;

namespace beta.Models.API.News
{
    public class Embedded
    {
        [JsonPropertyName("wp:featuredmedia")]
        public FeaturedMedia[] Media { get; set; }
    }
}
