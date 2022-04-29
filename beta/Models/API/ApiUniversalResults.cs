using beta.Models.API.Base;
using beta.Models.API.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace beta.Models.API
{
    public class ApiUniversalResults
    {
        [JsonPropertyName("data")]
        public ApiMapData[] Data { get; set; }

        [JsonPropertyName("included")]
        public ApiUniversalWithAttributes[] Included { get; set; }

        public Dictionary<string, string> GetAttributesFromIncluded(ApiDataType type, int id)
        {
            if (Included is null) return null;

            for (int i = 0; i < Included.Length; i++)
            {
                var item = Included[i];

                if (item.Type != type) continue;

                if (item.Id == id)
                    return item.Attributes;
            }

            return null;
        }

        [JsonPropertyName("meta")]
        public ApiVaultMeta Meta { get; set; }
    }
}
