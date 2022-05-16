using System.Text.Json.Serialization;

namespace beta.Models.API.Forum
{
    public class ApiForumRecentResult
    {
        public int topicCount { get; set; }
        public int nextStart { get; set; }
        public ForumTopic[] topics { get; set; }
        [JsonPropertyName("pagination")]
        public ForumPagination Pagination { get; set; }

        public string[] Terms => new string[]
        {
            "",
            "daily",
            "weekly",
            "monthly",
        };
    }
}
