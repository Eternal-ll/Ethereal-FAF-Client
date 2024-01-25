using Ethereal.FAF.UI.Client.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Ethereal.FAF.UI.Client.Infrastructure.Converters
{
	public class UriToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!Uri.IsWellFormedUriString(value as string, UriKind.Absolute)) return value;
            BitmapImage image = new();
            image.BeginInit();
            image.DecodePixelWidth = 60;
            image.DecodePixelHeight = 60;
            image.CacheOption = BitmapCacheOption.OnDemand;
            image.UriSource = new Uri(value as string);
            image.EndInit();
            image.DownloadCompleted += Image_DownloadCompleted;
            return image;
        }

        private void Image_DownloadCompleted(object sender, EventArgs e)
        {
            var image = (BitmapImage)sender;
            image.DownloadCompleted -= Image_DownloadCompleted;
            image.Freeze();
        }   

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
