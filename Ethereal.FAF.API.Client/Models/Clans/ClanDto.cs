using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.Clans
{

    public class ClanDto
    {
        public int Id { get; set; }
        [JsonPropertyName("createTime")]
        public DateTime CreateTime { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("requiresInvitation")]
        public bool RequiresInvitation { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("tagColor")]
        public string TagColor { get; set; }

        [JsonPropertyName("updateTime")]
        public DateTime UpdateTime { get; set; }

        [JsonPropertyName("websiteUrl")]
        public string WebsiteUrl { get; set; }
        public ApiPlayerData Founder { get; set; }
        public ApiPlayerData Leader { get; set; }
    }
}
