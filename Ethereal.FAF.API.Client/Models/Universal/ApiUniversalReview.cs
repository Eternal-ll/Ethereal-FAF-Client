using FAF.Domain.LobbyServer;

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
        public double LowerBound => double.Parse(Attributes["lowerBound"].Replace('.',','));
        public double Negative => double.Parse(Attributes["negative"].Replace('.', ','));
        public double Positive => double.Parse(Attributes["positive"].Replace('.', ','));
        public int ReviewsCount => int.Parse(Attributes["reviews"]);
        public double Score => double.Parse(Attributes["score"].Replace('.', ','));
        public double Average => Score / ReviewsCount;
    }
    public class ApiUniversalStatistics : Base.ApiUniversalData
    {
        public int DownloadsCount => int.Parse(Attributes["downloads"]);
        public int DrawsCount => int.Parse(Attributes["draws"]);
        public int PlaysCount => int.Parse(Attributes["plays"]);
    }
}
