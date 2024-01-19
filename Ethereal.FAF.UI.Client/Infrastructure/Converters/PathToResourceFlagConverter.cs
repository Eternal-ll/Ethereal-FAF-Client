using System;
using System.Globalization;
using System.Windows.Data;

namespace Ethereal.FAF.UI.Client.Infrastructure.Converters
{
    public class PathToResourceFlagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => $"/Resources/Images/Flags/{value}.png";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
