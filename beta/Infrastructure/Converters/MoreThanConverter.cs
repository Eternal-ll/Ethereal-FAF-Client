using System;
using System.Globalization;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    public class MoreThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int num = 0;
            if (value is string text)
                num = text.Length;
            else num = System.Convert.ToInt32(value);

            if (parameter == null)
                return num > 0;
            return num > int.Parse(parameter.ToString());
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}