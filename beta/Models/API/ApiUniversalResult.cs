using System.Text.Json.Serialization;

namespace beta.Models.API
{
    public class ApiUniversalResult<T> where T : class
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
