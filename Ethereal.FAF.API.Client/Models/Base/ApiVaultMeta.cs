using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.Base
{
    public class ApiVaultMeta
    {
        [JsonPropertyName("page")]
        public ApiVaultPage Page { get; set; }
    }
}
