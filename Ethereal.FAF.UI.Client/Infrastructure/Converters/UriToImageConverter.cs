using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Ethereal.FAF.UI.Client.Infrastructure.Converters
{
    public class UriToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string url) return value;
            //if (!Uri.IsWellFormedUriString(value as string, UriKind.Absolute)) return value;
            var fileExist = File.Exists(url);
            BitmapImage image = new();
            image.BeginInit();
            image.DecodePixelWidth = 60;
            image.DecodePixelHeight = 60;
            image.CacheOption = BitmapCacheOption.OnLoad;
            FileStream? stream = null;
            if (fileExist)
            {
                stream = File.OpenRead(url);
                image.StreamSource = stream;
            }
            else
            {
                image.UriSource = new Uri(value as string);
            }
            image.EndInit();
            if (image.CanFreeze)
            {
                image.Freeze();
            }
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
            }
            //image.DownloadCompleted += Image_DownloadCompleted;
            return image;
        }

        private void Image_DownloadCompleted(object sender, EventArgs e)
        {
            var image = (BitmapImage)sender;
            //image.DownloadCompleted -= Image_DownloadCompleted;
            image.Freeze();
        }   

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
