using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Ethereal.FAF.UI.Client.Infrastructure.Converters
{
    public class BorderClipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 3 &&
                values[0] is double double1 &&
                values[1] is double double2 &&
                values[2] is CornerRadius radius1)
            {
                var width = double1;
                var height = double2;
                if (width < double.Epsilon || height < double.Epsilon)
                {
                    return Geometry.Empty;
                }
                var radius = radius1;
                // Actually we need more complex geometry, when CornerRadius has different values.
                // But let me not to take this into account, and simplify example for a common value.
                var clip = new RectangleGeometry(new Rect(0, 0, width, height), radius.TopLeft, radius.TopLeft);
                clip.Freeze();
                return clip;
            }
            return DependencyProperty.UnsetValue;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
