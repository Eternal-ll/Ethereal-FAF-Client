using System.Text.Json.Serialization;

namespace beta.Models.API.Forum
{
    public class ForumUser
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [JsonPropertyName("userslug")]
        public string userslug { get; set; }
        public string UserUrl => "https://forum.faforever.com/user/" + userslug;

        [JsonPropertyName("picture")]
        public string picture { get; set; }
        public string UserPictureUrl => !string.IsNullOrWhiteSpace(picture) ? "https://forum.faforever.com" + picture : null;

        [JsonPropertyName("icon:text")]
        public string UserIconText { get; set; }
        [JsonPropertyName("icon:bgColor")]
        public string UserIconBgColor { get; set; }
    }
}
