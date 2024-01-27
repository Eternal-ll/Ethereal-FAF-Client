using Humanizer;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Ethereal.FAF.UI.Client.Infrastructure.Converters
{
    internal class TruncateStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text) return value;
            var maximum = int.TryParse(parameter?.ToString(), out var max) ? max : 50;
            return text.Truncate(maximum);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
