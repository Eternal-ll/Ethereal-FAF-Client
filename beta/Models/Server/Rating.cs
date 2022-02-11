using System;

namespace beta.Models.Server
{
    public class Rating
    {
        public double[] rating { get; set; }
        public int number_of_games { get; set; }
        public string name { get; set; }

        //public DateTime? Updated;
        #region Custom properties

        #region On player update
        public double[] RatingDifference;
        public int GamesDifference;
        #endregion

        #region Filling from converter

        #region DisplayedRating
        private int? _DisplayedRating;
        public int DisplayedRating => _DisplayedRating ??= Convert.ToInt32(rating[0] - 3 * rating[1]);
        #endregion

        public int DisplayedRatingDifference => GamesDifference == 0 ? 0 : Convert.ToInt32(RatingDifference[0] - 3 * RatingDifference[1]);
        
        #endregion

        #endregion
    }
}
