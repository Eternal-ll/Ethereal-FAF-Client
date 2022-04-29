using System.Text.Json.Serialization;

namespace beta.Models.API.News
{
    public class Rendered
    {
        [JsonPropertyName("rendered")]
        public string Text { get; set; }
    }
}
