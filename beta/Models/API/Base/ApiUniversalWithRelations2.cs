using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace beta.Models.API.Base
{
    public class ApiUniversalWithRelations2 : ApiUniversalWithAttributes
    {
        [JsonPropertyName("relationships")]
        public Dictionary<string, ApiUniversalArrayRelationship> Relations { get; set; }
    }
}
