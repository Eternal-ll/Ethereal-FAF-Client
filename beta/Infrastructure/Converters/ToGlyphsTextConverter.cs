using System;
using System.Globalization;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    /// <summary>
    /// Using low UI element Glyps for perfomance
    /// </summary>
    public class ToGlyphsTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return "!Null";
            var text = value.ToString();
            return text.Length == 0 ? ' ' : text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}