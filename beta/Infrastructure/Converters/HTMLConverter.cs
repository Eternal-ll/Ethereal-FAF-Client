using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    internal class HTMLConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return null;

            if (value is not string data) return value.ToString();

            StringBuilder main = new();
            StringBuilder second = new();

            for (int i = 0; i < data.Length; i++)
            {
                var letter = data[i];

                if (letter == '<')
                {
                    second.Clear();
                    second.Append(letter);
                    i++;
                    var isClosing = false;
                    while (letter != '>')
                    {
                        if (!isClosing) isClosing = letter == '/';
                        
                        letter = data[i];
                        i++;
                        second.Append(letter);
                    }

                    if (isClosing && second.ToString().Contains("br", StringComparison.OrdinalIgnoreCase))
                    {
                        main.Append('\r');
                    }


                    letter = data[i];
                    main.Append(letter);
                }
                else main.Append(letter);
            }
            return main.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
