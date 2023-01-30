using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.Base
{
    public class ApiUniversalRelationship
    {
        [JsonPropertyName("data")]
        public ApiUniversalTypeId Data { get; set; }
    }
}
