using beta.Models.Server;
using System;
using System.Globalization;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    public class RatingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            //bool showDifference = false;

            //var param = parameter.ToString().ToLower();
            //if (param.Length>0)
            //{   
            //    showDifference = param[0] == 't';
            //}

            Rating rating = (Rating)value;

            double calculated = rating.rating[0] - 3 * rating.rating[1];
            return System.Convert.ToInt32(calculated);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
