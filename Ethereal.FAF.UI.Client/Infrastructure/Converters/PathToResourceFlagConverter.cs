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
        private const string DefaultFlagResource = "CountryFlagBitmapImage_earth";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flagName = value?.ToString()?.ToLower() ?? DefaultFlag;
            var resourcePath = $"CountryFlagBitmapImage_{flagName}";
            var resource = Application.Current.TryFindResource(resourcePath)
                ?? Application.Current.FindResource(DefaultFlagResource);
            return resource as BitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
