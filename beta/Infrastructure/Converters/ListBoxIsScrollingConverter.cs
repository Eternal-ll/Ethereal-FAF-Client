using beta.Infrastructure.Utils;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    internal class ListBoxIsScrollingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ListBox listBox) return false;
            var scroll = Tools.FindChild<ScrollViewer>(listBox);
            return scroll.ContentVerticalOffset > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
