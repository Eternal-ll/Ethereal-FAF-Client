using System.Text.Json.Serialization;

namespace beta.Models.API
{
    public class ApiUniversalResultWithMeta<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
        [JsonPropertyName("meta")]
        public ApiVaultMeta Meta { get; set; }
    }
}
