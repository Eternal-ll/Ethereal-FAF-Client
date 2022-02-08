using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace beta.Infrastructure.Converters
{
    internal class ChatPlayerHighlightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            var textBlock = (TextBlock)value;
            var inlines = textBlock.Inlines;

            var login = Properties.Settings.Default.PlayerNick;
            for (int i = 0; i < inlines.Count; i++)
            {
                var enumrator = inlines.GetEnumerator();
                while (enumrator.MoveNext())
                {
                    var inline = enumrator.Current;
                    if (inline is InlineUIContainer ui)
                    {
                        if (ui.Child is Button btn)
                        {
                            if (btn.Content as string == login)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
