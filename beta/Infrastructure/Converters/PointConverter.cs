using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    public class PointConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double xValue = (double)values[0];
            double yValue = (double)values[1];
            return new Point(xValue, yValue);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
