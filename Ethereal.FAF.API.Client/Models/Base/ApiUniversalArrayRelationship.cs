﻿using beta.Infrastructure.Converters.JSON;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.Base
{
    public class ApiUniversalArrayRelationship
    {
        [JsonPropertyName("data")]
        [JsonConverter(typeof(SingleOrArrayConverter<ApiUniversalTypeId>))]
        public List<ApiUniversalTypeId> Data { get; set; }
    }
}
