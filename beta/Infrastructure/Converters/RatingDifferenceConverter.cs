using beta.Models.Server;
using System;
using System.Globalization;
using System.Windows.Data;

namespace beta.Infrastructure.Converters
{
    //
    public class RatingDifferenceConverter : IValueConverter
    {   
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Rating rating = (Rating)value;

            double calculated = rating.rating[0] - 3 * rating.rating[1];

            //rating.DisplayedRating = System.Convert.ToInt32(calculated);

            //if (rating.GamesDifference > 0)
            //{
            //    double difference = rating.RatingDifference[0] - 3 * rating.RatingDifference[1];
            //    rating.DisplayedRatingDifference = System.Convert.ToInt32(difference);
            //}
            return rating;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
