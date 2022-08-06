using System;

namespace beta.Models
{
    public enum RatingChange : byte
    {
        None,
        Up,
        Down
    }
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

        #region On fly

        public string FormatName => name switch
        {
            "global" => "Global",
            "ladder_1v1" => "Ladder 1 vs 1",
            "tmm_2v2" => "TMM 2 vs 2",
            "tmm_4v4_share_until_death" => "TMM 4 vs 4 SUD",
            "tmm_4v4_full_share" => "TMM 4 vs 4 FS",
            _ => name
        };

        #region DisplayedRating
        private int? _DisplayedRating; // TODO
        public int DisplayedRating => _DisplayedRating ??= Convert.ToInt32(rating[0] - 3 * rating[1]);
        #endregion

        public int DisplayedRatingDifference => GamesDifference == 0 ? 0 : Convert.ToInt32(RatingDifference[0] - 3 * RatingDifference[1]);

        public RatingChange RatingChange => DisplayedRatingDifference == 0 ? RatingChange.None : DisplayedRatingDifference > 0 ? RatingChange.Up : RatingChange.Down;

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
