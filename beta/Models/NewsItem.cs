using beta.ViewModels.Base;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace beta.Models
{
    public class NewsItem : ViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Uri DestinationUri { get; set; }
        public string Author { get; set; }
        public Uri MediaUri { get; set; }
        private BitmapImage _BitmapImage;
        public BitmapImage BitmapImage
        {
            get
            {
                if (_BitmapImage == null)
                {
                    _BitmapImage = new BitmapImage(MediaUri, new(System.Net.Cache.RequestCacheLevel.NoCacheNoStore));
                    _BitmapImage.DownloadCompleted += (s, e) => OnPropertyChanged(nameof(ImageVisibility));
                    _BitmapImage.DownloadCompleted -= (s, e) => OnPropertyChanged();
                }
                return _BitmapImage;
            }
        }
        public bool IsDownloading => BitmapImage.IsDownloading;
        public Visibility ImageVisibility => BitmapImage.IsDownloading ? Visibility.Visible : Visibility.Collapsed;
    }
}
