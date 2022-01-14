using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace beta.Infrastructure.Converters
{
    public class TransparentColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color convert = (Color) value; // Color is a struct, so we cast
            return Color.FromArgb(0, convert.R, convert.G, convert.B);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
