using System.Text.Json.Serialization;

namespace beta.Models.API.Base
{
    /// <summary>
    /// 
    /// </summary>
    public class ApiUniversalWithRelations : ApiUniversalWithAttributes
    {
        [JsonPropertyName("relationships")]
        public ApiUniversalRelationships Relations { get; set; }
    }
}
