using System;

namespace beta.Models
{
    /// <summary>
    /// Used by PlayerInfoMessage. 
    /// </summary>
    public class Rating
    {
        public double[] rating { get; set; }
        public int number_of_games { get; set; }
        public string name { get; set; }

        //public DateTime? Updated;
        #region Custom properties

        #region On player update
        public double[] RatingDifference = new double[2];
        public int GamesDifference { get; set; }
        #endregion

        #region Filling from converter

        #region DisplayedRating
        private int? _DisplayedRating; // TODO
        public int DisplayedRating => _DisplayedRating ??= Convert.ToInt32(rating[0] - 3 * rating[1]);
        #endregion

        public int DisplayedRatingDifference => GamesDifference == 0 ? 0 : Convert.ToInt32(RatingDifference[0] - 3 * RatingDifference[1]);

        public string DisplayedDifferenceArrow
        {
            get
            {
                if (DisplayedRatingDifference == 0)
                    return null;
                if (DisplayedRatingDifference > 0)
                    return "↑";
                return "↓";
            }
        }

        #endregion

        #endregion
    }
}
