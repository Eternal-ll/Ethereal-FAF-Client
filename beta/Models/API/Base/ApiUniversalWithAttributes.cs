using beta.Infrastructure.Converters.JSON;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace beta.Models.API.Base
{
    /// <summary>
    /// Contains <see cref="ApiUniversalTypeId.Id"/>, <see cref="ApiUniversalTypeId.Type"/>, <see cref="Attributes"/>
    /// </summary>
    public class ApiUniversalWithAttributes : ApiUniversalTypeId
    {
        [JsonPropertyName("attributes")]
        [JsonConverter(typeof(DictionaryStringConverter))]
        public Dictionary<string, string> Attributes { get; set; }
    }
}
