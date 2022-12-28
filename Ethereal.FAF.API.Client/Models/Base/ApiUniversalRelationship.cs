using System.Text.Json.Serialization;

namespace beta.Models.API.Base
{
    public class ApiUniversalRelationship
    {
        [JsonPropertyName("data")]
        public ApiUniversalTypeId Data { get; set; }
    }
}
