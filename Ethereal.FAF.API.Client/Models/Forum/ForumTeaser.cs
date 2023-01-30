using System;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.Forum
{
    public class ForumTeaser
    {
        public string content { get; set; }
        public DateTime timestampISO { get; set; }
        [JsonPropertyName("user")]
        public ForumUser Author { get; set; }
        public int index { get; set; }
    }
}
