using FAF.Domain.Direct.Entities;
using System.Text.Json.Serialization;

namespace FAF.Domain.Direct.Models
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Embedded
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("wp:featuredmedia")]
        public Media[] Media { get; set; }
        public Media FirstMedia => Media?.Length > 0 ? Media[0] : null;
    }
}
