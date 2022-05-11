using System;
using System.Windows.Media.Imaging;

namespace beta.Models
{
    internal class AvatarModel
    {
        public string ToolTip { get; set; }
        public Uri Url { get; set; }
        private BitmapImage _Preview;
        public BitmapImage Preview
        {
            get
            {
                if (_Preview is not null) return _Preview;
                BitmapImage img = new();
                img.BeginInit();
                img.DecodePixelHeight = 20;
                img.DecodePixelWidth = 40;
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.UriSource = Url;
                img.EndInit();
                _Preview = img;
                return _Preview;
            }
        }

        public string Filename { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsSelected { get; set; }
    }
}
