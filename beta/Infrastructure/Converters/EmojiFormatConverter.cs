using System;
using System.Globalization;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    /// <summary>
    /// Formatting emoji name to :name:
    /// </summary>
    internal class EmojiFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            return ":" + value.ToString() + ":";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
