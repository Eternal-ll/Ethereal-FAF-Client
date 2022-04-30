using beta.Infrastructure.Utils;
using System;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace beta.Models.API.News
{
    public interface IPostModel
    {

    }
    public class PostModel : IPostModel
    {
        [JsonPropertyName("title")]
        public Rendered Title { get; set; }
        [JsonPropertyName("content")]
        public Rendered Content { get; set; }
        public string Description => Content.Text.Length > 5 ? Content.Text[4..^5] : Content.Text;
        [JsonPropertyName("date")]
        public DateTime DateTime { get; set; }
        [JsonPropertyName("newshub_externalLinkUrl")]
        public Uri SourceLink { get; set; }
        [JsonPropertyName("_embedded")]
        public Embedded Embedded { get; set; }
        private BitmapImage _Image;
        public BitmapImage Image
        {
            get
            {
                if (_Image is null && Embedded.Media is not null && Embedded.Media.Length > 0)
                {
                    _Image = ImageTools.InitializeLazyBitmapImage(Embedded.Media[0].ImageUrl);
                }
                return _Image;
            }
        }
    }
}
