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
        public string ShortText => Text is null ? null : Text.Length > 200 ? Text.Substring(0, 200) + "..." : Text;

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("protected")]
        public bool IsProtected { get; set; }
    }
}
