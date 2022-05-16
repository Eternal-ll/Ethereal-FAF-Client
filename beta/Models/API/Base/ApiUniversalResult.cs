using System.Text.Json.Serialization;

namespace beta.Models.API.Base
{
    public class ApiUniversalResult<T> where T : class
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("included")]
        public ApiUniversalWithAttributes[] Included { get; set; }

        [JsonPropertyName("meta")]
        public ApiVaultMeta Meta { get; set; }
    }
}
