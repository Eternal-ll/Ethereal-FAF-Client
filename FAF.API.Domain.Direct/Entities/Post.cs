using FAF.Domain.Direct.Models;
using System;
using System.Text.Json.Serialization;

namespace FAF.Domain.Direct.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Post : Base.Entity
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("title")]
        public Content Title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("content")]
        public Content Content { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("excerpt")]
        public Content Excerpt { get; set; }
        
        #region Newshub
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("newshub_backgroundcolor")]
        public string NewshubBackgroundcolor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("newshub_badge")]
        public string NewshubBadge { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("newshub_customStyle")]
        public string NewshubCustomStyle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("newshub_externalLinkUrl")]
        public Uri NewshubExternalLinkUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("newshub_pos")]
        public string NewshubPos { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("newshub_sortIndex")]
        public string NewshubSortIndexStr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NewshubSortIndex => string.IsNullOrWhiteSpace(NewshubSortIndexStr)  ? 0 : int.Parse(NewshubSortIndexStr);
        public int NewshubSortIndexHalf => (int)(NewshubSortIndex * .33);
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("newshub_subtitle")]
        public string NewshubSubtitle { get; set; }
        #endregion

        [JsonPropertyName("_embedded")]
        public Embedded Embedded { get; set; }
    }
}
