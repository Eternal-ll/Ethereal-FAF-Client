using System.Text.Json.Serialization;

namespace FAF.Domain.Direct.Entities
{
    public class Author : Base.Entity
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        //[JsonPropertyName("url")]
        //public string Url { get; set; }

        //[JsonPropertyName("description")]
        //public string Description { get; set; }

        //[JsonPropertyName("link")]
        //public Uri Link { get; set; }

        //[JsonPropertyName("slug")]
        //public string Slug { get; set; }

        //[JsonPropertyName("avatar_urls")]
        //public Dictionary<string, Uri> AvatarUrls { get; set; }
    }
}
