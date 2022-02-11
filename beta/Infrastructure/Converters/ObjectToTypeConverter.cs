using System;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    /// <summary>
    /// Converting binding value to Type for comparing stuff
    /// </summary>
    public class ObjectToTypeConverter : IValueConverter
    {
        /// <summary>
        /// Returns Type of binding value or Null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => value?.GetType();
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
