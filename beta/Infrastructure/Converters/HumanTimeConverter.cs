using System;
using System.Globalization;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    internal class HumanTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return null;
            if (value is not DateTime time || !DateTime.TryParse(value.ToString(), out time)) return null;
            var now = DateTime.UtcNow;
            var dif = now - time;
            var seconds = dif.TotalSeconds;

            return seconds switch
            {
                < 60 => $"{seconds} seconds ago",
                >=60 and <120 => $"Minute ago",
                < 3600 => $"{dif.Minutes} minutes ago",
                >= 3600 and < 7200 => $"Hour ago",
                >= 7200 => $"{dif.Hours} hours ago",
                _ => $"{seconds} ago"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
