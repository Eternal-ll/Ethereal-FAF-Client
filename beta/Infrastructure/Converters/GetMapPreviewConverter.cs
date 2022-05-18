using beta.Infrastructure.Utils;
using System;
using System.Globalization;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    public class GetMapPreviewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null && parameter is not null) value = parameter;
            if (value is not string name) return null;
           return ImageTools.InitializeLazyBitmapImage("https://content.faforever.com/maps/previews/large/" + name + ".png");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
