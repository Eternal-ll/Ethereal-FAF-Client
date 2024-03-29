﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.Base
{
    public class ApiUniversalWithRelations : ApiUniversalTypeId
    {
        [JsonPropertyName("relationships")]
        public Dictionary<string, ApiUniversalArrayRelationship> Relations { get; set; }

        [JsonPropertyName("included")]
        public ApiUniversalData[] Included { get; set; }

        [JsonPropertyName("meta")]
        public ApiVaultMeta Meta { get; set; }
    }
}
