using beta.Infrastructure.Utils;
using System;
using System.Globalization;
using System.Windows.Data;

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

            if (value.ToString().Contains("forum"))
            {
                return ImageTools.InitializeLazyBitmapImage(url, 45, 45);
            }
            return ImageTools.InitializeLazyBitmapImage(url);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
