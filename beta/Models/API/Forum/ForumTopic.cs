using beta.Infrastructure.Converters.JSON;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace beta.Models.API.Forum
{
    public class ForumTopic
    {
        public string title { get; set; }
        public int viewcount { get; set; }
        public int postcount { get; set; }
        public int downvotes { get; set; }
        public int upvotes { get; set; }
        public DateTime timestampISO { get; set; }
        public DateTime lastposttimeISO { get; set; }

        [JsonConverter(typeof(DictionaryStringConverter))]
        public Dictionary<string, string> category { get; set; }
        public string CategoryName => category["name"];
        public Uri CategoryURL => new("https://forum.faforever.com/category/" + category["slug"]);
        public string slug { get; set; }
        public Uri SourceUrl => new("https://forum.faforever.com/topic/" + slug);

        [JsonPropertyName("user")]
        public ForumUser Author { get; set; }
        [JsonPropertyName("teaser")]
        public ForumTeaser Teaser { get; set; }
    }
}
