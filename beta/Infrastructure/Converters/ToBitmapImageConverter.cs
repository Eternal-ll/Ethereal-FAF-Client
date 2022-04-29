using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Converters
{
    internal class ToBitmapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return null;

            Uri url;

            if (value is Uri uri)
            {
                url = uri;
            }
            else if (value is string str)
            {
                url = new(str);
            }
            else return null;

            var img = new BitmapImage();
            img.BeginInit();

            if (value.ToString().Contains("forum"))
            {
                img.DecodePixelWidth = 45;
                img.DecodePixelHeight = 45;
            }

            img.CacheOption = BitmapCacheOption.OnDemand;
            img.UriCachePolicy = new(System.Net.Cache.RequestCacheLevel.CacheIfAvailable);
            img.UriSource = url;
            img.EndInit();
            return img;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
