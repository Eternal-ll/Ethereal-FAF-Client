using System;
using System.Globalization;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    internal class RatingTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string type) return value;
            return type switch
            {
                "global" => "Global",
                "ladder_1v1" => "Ladder 1 vs 1",
                "tmm_2v2" => "TMM 2 vs 2",
                "tmm_4v4_full_share" => "TMM 4 vs 4 FS",
                "tmm_4v4_share_until_death" => "TMM 4 vs 4 SUD",
                _ => type,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
