using Ethereal.FAF.API.Client.Models.Enums;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.Base
{
    /// <summary>
    /// Contains <see cref="Id"/>, <see cref="Type"/>
    /// </summary>
    public class ApiUniversalTypeId
    {
        [JsonPropertyName("id")]
        public string _IdString { get; set; }
        [JsonIgnore]
        public int Id => int.Parse(_IdString);

        [JsonPropertyName("type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ApiDataType Type { get; set; }
    }
}
