using System;
using System.Globalization;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    /// <summary>
    /// Converter for checking if value is null. Returns Bool of binding value
    /// </summary>
    internal class IsNullConverter : IValueConverter
    {
        /// <summary>
        /// Returns Bool True/False of binding is null expression
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value == null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
