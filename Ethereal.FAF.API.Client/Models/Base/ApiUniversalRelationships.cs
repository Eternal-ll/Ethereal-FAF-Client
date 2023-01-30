using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.Base
{
    public class ApiUniversalRelationships
    {
        [JsonPropertyName("author")]
        public ApiUniversalRelationship Author { get; set; }

        [JsonPropertyName("latestVersion")]
        public ApiUniversalRelationship LatestVersion { get; set; }

        [JsonPropertyName("reviewsSummary")]
        public ApiUniversalRelationship ReviewsSummary { get; set; }
    }
}
