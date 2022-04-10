using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace beta.Models.API
{
    public class Record
    {
        [JsonPropertyName("data")]
        public List<FeaturedModFile> FeaturedModFiles { get; set; }
    }
}
