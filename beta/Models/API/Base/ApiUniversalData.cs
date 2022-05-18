using beta.Infrastructure.Converters.JSON;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace beta.Models.API.Base
{
    /// <summary>
    /// Contains <see cref="ApiUniversalTypeId.Id"/>, <see cref="ApiUniversalTypeId.Type"/>, <see cref="Attributes"/>
    /// </summary>
    public class ApiUniversalData : ApiUniversalTypeId
    {
        [JsonPropertyName("attributes")]
        [JsonConverter(typeof(DictionaryStringConverter))]
        public Dictionary<string, string> Attributes { get; set; }

        [JsonPropertyName("relationships")]
        public Dictionary<string, ApiUniversalArrayRelationship> Relations { get; set; }

        [JsonPropertyName("included")]
        public ApiUniversalData[] Included { get; set; }

        [JsonPropertyName("meta")]
        public ApiVaultMeta Meta { get; set; }
    }
    
}
