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
        public int DisplayedRating { get; set; }
        public int DisplayedRatingDifference { get; set; }
        #endregion

        #endregion
    }
}
