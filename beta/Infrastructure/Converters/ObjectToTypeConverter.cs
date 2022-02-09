using System;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    public class ObjectToTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.GetType();
        }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
