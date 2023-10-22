using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models
{
	public class FafApiResult<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
