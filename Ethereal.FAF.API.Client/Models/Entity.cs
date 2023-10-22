using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models
{
	public class Entity<T> : Base.ApiUniversalTypeId
    {
        [JsonPropertyName("attributes")]
        public T Attributes { get; set; }
    }
}
