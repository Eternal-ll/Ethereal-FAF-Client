using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace beta.Models.API.Base
{
    public class ApiUniversalWithRelations : ApiUniversalTypeId
    {
        [JsonPropertyName("relationships")]
        public Dictionary<string, ApiUniversalArrayRelationship> Relations { get; set; }

        [JsonPropertyName("included")]
        public ApiUniversalData[] Included { get; set; }

        [JsonPropertyName("meta")]
        public ApiVaultMeta Meta { get; set; }
    }
}
