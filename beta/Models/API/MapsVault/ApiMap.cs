using beta.Models.API.Universal;
using System;

namespace beta.Models.API.MapsVault
{
    internal class ApiMap : Base.ApiUniversalData
    {
        public string DisplayedName => Attributes["displayName"];
        public string BattleType => Attributes["battleType"];
        public string MapType => Attributes["mapType"];
        public int GamesPlayed => int.Parse(Attributes["gamesPlayed"]);
        public bool IsRecommended => bool.Parse(Attributes["recommended"]);

        public DateTime CreateTime => DateTime.Parse(Attributes["createTime"]);
        public DateTime UpdateTime => DateTime.Parse(Attributes["updateTime"]);

        public ApiUniversalStatistics StatisticsSummary { get; set; }
        public ApiUniversalSummary ReviewsSummary { get; set; }
        public MapVersionModel[] Versions { get; set; }
    }
    internal class ApiMapModel : ApiMap
    {
        //public MapVersionModel[] Versions { get; set; }
        public string Author { get; set; }
    }
}
