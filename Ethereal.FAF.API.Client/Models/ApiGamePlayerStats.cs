using Ethereal.FAF.API.Client.Models.Base;
using System;

namespace Ethereal.FAF.API.Client.Models
{
    public class ApiGamePlayerStats : ApiUniversalData
    {
        private static double GetDouble(string value) => double.Parse(value.Replace('.', ','));
        public double AfterDeviation => GetDouble(Attributes["afterDeviation"]);
        public double AfterMean => GetDouble(Attributes["afterMean"]);
        public double BeforeDeviation => GetDouble(Attributes["beforeDeviation"]);
        public double BeforeMean => GetDouble(Attributes["beforeMean"]);

        public DateTime? ScoreDateTime => DateTime.TryParse(Attributes["scoreTime"], out var res) ? res : null;

        private int? _RatingAfter;
        public int RatingAfter => _RatingAfter ??= Convert.ToInt32(AfterMean - 3 * AfterDeviation);

        private int? _RatingBefore;
        public int RatingBefore => _RatingBefore ??= Convert.ToInt32(AfterMean - 3 * AfterDeviation);

        private int? _RatingDifference;
        public int RatingDifference => _RatingDifference ??= RatingBefore - RatingAfter;
    }
}
