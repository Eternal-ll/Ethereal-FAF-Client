using FAF.Domain.LobbyServer;
using System.Globalization;

namespace beta.Models.API.Universal
{
    public class ApiUniversalReview : Base.ApiUniversalData
    {
        public DateTime CreateTime => DateTime.Parse(Attributes["createTime"]);
        public double Score => double.Parse(Attributes["score"]);
        public string Text => Attributes["text"];
        public DateTime UpdateTime => DateTime.Parse(Attributes["updateTime"]);
    }
    public class UniversalReviewModel : ApiUniversalReview
    {
        public string Author { get; set; }
        public int? AuthorId { get; set; }
        public PlayerInfoMessage PlayerInstance { get; set; }
        public override void ParseIncluded()
        {
            var included = Relations;
            if (included is null) return;
            if (included.Count == 0) return;
            foreach (var entity in included)
            {
                if (entity.Value.Data[0].Type == Enums.ApiDataType.player)
                {
                    AuthorId = entity.Value.Data[0].Id;
                    break;
                }
            }
        }
    }
    public class ApiUniversalSummary : Base.ApiUniversalData
    {
        public double LowerBound => double.Parse(Attributes["lowerBound"], CultureInfo.InvariantCulture);
        public double Negative => double.Parse(Attributes["negative"], CultureInfo.InvariantCulture);
        public double Positive => double.Parse(Attributes["positive"], CultureInfo.InvariantCulture);
        public int ReviewsCount => int.Parse(Attributes["reviews"], CultureInfo.InvariantCulture);
        public double Score => double.Parse(Attributes["score"], CultureInfo.InvariantCulture);
        public double Average => Score / ReviewsCount;
        public double AverageRounded => Math.Round(Average);
        private static double WilsonAlgorithm(double positive, double negative)
        {
            return ((positive + 1.9208) / (positive + negative) -
                    1.96 * Math.Sqrt((positive * negative) / (positive + negative) + 0.9604) /
                    (positive + negative)) / (1 + 3.8416 / (positive + negative));
        }
    }
    public class ApiUniversalStatistics : Base.ApiUniversalData
    {
        public int DownloadsCount => int.Parse(Attributes["downloads"]);
        public int DrawsCount => int.Parse(Attributes["draws"]);
        public int PlaysCount => int.Parse(Attributes["plays"]);
    }
}
