using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.Base
{
    public class Response<T> where T : class
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("included")]
        public ApiUniversalData[] Included { get; set; }

        [JsonPropertyName("relationships")]
        public Dictionary<string, ApiUniversalArrayRelationship> Relations { get; set; }

        [JsonPropertyName("meta")]
        public ApiVaultMeta Meta { get; set; }

        public virtual void ParseIncluded()
        {

        }
        public virtual void ParseIncluded(out SortedDictionary<string, string[]> entityProperties)
        {
            entityProperties = null;
        }
    }
}
