using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Ethereal.FAF.UI.Client.Infrastructure.Converters
{
    public class PathToResourceFlagConverter : IValueConverter
    {
        private const string DefaultFlag = "earth";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var resource = Application.Current
                .TryFindResource($"CountryFlagBitmapImage_{value.ToString().ToLower()}") ??
                Application.Current.TryFindResource(DefaultFlag);
            if (resource is BitmapImage image)
            {
                return image;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
