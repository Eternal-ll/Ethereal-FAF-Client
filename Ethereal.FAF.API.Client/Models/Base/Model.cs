using beta.Infrastructure.Converters.JSON;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.Base
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Model<T> : ApiUniversalTypeId where T : class
    {
        [JsonPropertyName("attributes")]
        public T Attributes { get; set; }

        [JsonPropertyName("relationships")]
        public Dictionary<string, ApiUniversalArrayRelationship> Relations { get; set; }

        [JsonPropertyName("included")]
        public ApiUniversalData[] Included { get; set; }

        [JsonPropertyName("meta")]
        public ApiVaultMeta Meta { get; set; }
        public virtual void ParseIncluded() { }
    }
    
}
