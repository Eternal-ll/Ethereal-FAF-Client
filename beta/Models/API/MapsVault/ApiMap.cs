using beta.Models.API.Universal;
using beta.Models.Server;

namespace beta.Models.API.MapsVault
{
    internal class ApiMap : Base.ApiUniversalData
    {
        public string DisplayedName => Attributes?["displayName"];
        public string BattleType => Attributes?["battleType"];
        public string MapType => Attributes?["mapType"];
        public string GamesPlayed => Attributes?["gamesPlayed"];
        public string IsRecommended => Attributes?["recommended"];

        public string CreateTime => Attributes?["createTime"];
        public string UpdateTime => Attributes?["updateTime"];

        public ApiUniversalStatistics StatisticsSummary { get; set; }
        public ApiUniversalSummary ReviewsSummary { get; set; }
        public MapVersionModel[] Versions { get; set; }
    }
    internal class ApiMapModel : ApiMap
    {
        //public MapVersionModel[] Versions { get; set; }
        public string Author { get; set; }
        public PlayerInfoMessage AuthorInstance { get; set; }
    }
}
