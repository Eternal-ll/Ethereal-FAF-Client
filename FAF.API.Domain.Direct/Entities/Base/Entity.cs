using FAF.Domain.Direct.Models;
using System;
using System.Text.Json.Serialization;

namespace FAF.Domain.Direct.Entities.Base
{
    /// <summary>
    /// 
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("date_gmt")]
        public DateTime DateGmt { get; set; }

        [JsonPropertyName("guid")]
        public Content Guid { get; set; }

        [JsonPropertyName("modified")]
        public DateTime Modified { get; set; }

        [JsonPropertyName("modified_gmt")]
        public DateTime ModifiedGmt { get; set; }
    }
}
