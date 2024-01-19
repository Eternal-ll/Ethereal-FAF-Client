using System;
using System.Globalization;
using System.Windows.Data;

namespace Ethereal.FAF.UI.Client.Infrastructure.Converters
{
    public class IsValueLessThanParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(parameter?.ToString(), out var minValue))
            {
                if (double.TryParse(value?.ToString(), out var currentValue))
                {
                    return minValue > currentValue;
                }
                return false;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
