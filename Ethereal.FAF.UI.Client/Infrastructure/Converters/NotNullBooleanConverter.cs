using System;
using System.Globalization;

namespace Ethereal.FAF.UI.Client.Infrastructure.Converters
{
    public sealed class NotNullBooleanConverter : BooleanConverter<bool>
    {
        public NotNullBooleanConverter(bool trueValue, bool falseValue) : base(trueValue, falseValue)
        {
        }
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? True : False;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return base.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
