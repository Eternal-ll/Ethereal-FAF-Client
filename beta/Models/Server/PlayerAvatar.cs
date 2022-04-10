using beta.Infrastructure.Utils;
using System;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace beta.Models.Server
{
    public class PlayerAvatar
    {
        public byte[] ImageSource { get; set; }

        private BitmapImage _Preview;
        public BitmapImage Preview => _Preview ??= ImageSource?.ToBitmapImage();


        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("tooltip")]
        public string Tooltip { get; set; }
    }
}
