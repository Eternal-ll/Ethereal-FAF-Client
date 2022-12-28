using beta.Infrastructure.Converters.JSON;
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
        public string CategoryIcon => GetFontAwesomeTextFromForumIconName(category["icon"]);

        [JsonPropertyName("user")]
        public ForumUser Author { get; set; }
        [JsonPropertyName("teaser")]
        public ForumTeaser Teaser { get; set; }
        public static string GetFontAwesomeTextFromForumIconName(string name) => name switch
        {
            "fa-comment-o" => "\uf075",
            "fa-bell-o" => "\uf0f3",
            "fa-search" => "\uf002",
            "fa-group" => "\uf0c0",
            "fa-user" => "\uf007",
            "fa-fire" => "\uf06d",
            "fa-tags" => "\uf02c",
            "fa-inbox" => "\uf01c",
            "fa-list" => "\uf03a",
            "fa-clock-o" => "\uf017",

            "fa-bullhorn" => "\uf0a1",
            "fa-child" => "\uf1ae",
            "fa-newspaper-o" => "\uf1ea",
            "fa-trophy" => "\uf091",
            "fa-comments-o" => "\uf086",
            "fa-balance-scale" => "\uf24e",
            "fa-lightbulb-o" => "\uf0eb",
            "fa-question" => "\uf128",
            "fa-support" => "\uf1cd",
            "fa-gamepad" => "\uf11b",
            "fa-wikipedia-w" => "\uf266",
            "fa-book" => "\uf02d",
            "fa-diamond" => "\uf3a5",
            "fa-wrench" => "\uf0ad",
            "fa-map" => "\uf279",
            "fa-code" => "\uf121",
            "fa-university" => "\uf19c",
            "fa-paragraph" => "\uf1dd",
            "fa-history" => "\uf1da",


            _ => "?" + name
        };
    }
}
