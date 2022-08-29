using System.Text.Json.Serialization;

namespace beta.Models.API.Base
{
    public class ApiVaultMeta
    {
        [JsonPropertyName("page")]
        public ApiVaultPage Page { get; set; }
    }
}
