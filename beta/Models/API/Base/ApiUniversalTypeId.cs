using beta.Models.API.Enums;
using System.Text.Json.Serialization;

namespace beta.Models.API.Base
{
    /// <summary>
    /// Contains <see cref="Id"/>, <see cref="Type"/>
    /// </summary>
    public class ApiUniversalTypeId
    {
        [JsonPropertyName("id")]
        public string _IdString { get; set; }
        public int Id => int.Parse(_IdString);

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ApiDataType Type { get; set; }
    }
}
