using System.Text.Json.Serialization;

namespace FAF.Domain.Direct.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Content
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("rendered")]
        public string Text { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("protected")]
        public bool IsProtected { get; set; }
    }
}
