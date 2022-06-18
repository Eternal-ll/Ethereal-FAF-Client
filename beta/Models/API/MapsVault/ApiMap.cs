using beta.Models.API.Enums;
using beta.Models.API.Universal;
using System;
using System.Collections.Generic;

namespace beta.Models.API.MapsVault
{
    public class ApiMap : Base.ApiUniversalData
    {
        public string DisplayedName => Attributes["displayName"];
        public string BattleType => Attributes["battleType"];
        public string MapType => Attributes["mapType"];
        public int GamesPlayed => int.Parse(Attributes["gamesPlayed"]);
        public bool IsRecommended => bool.Parse(Attributes["recommended"]);
        public DateTime CreateTime => DateTime.Parse(Attributes["createTime"]);
        public DateTime UpdateTime => DateTime.Parse(Attributes["updateTime"]);
    }
    public class ApiMapModel : ApiMap
    {
        public string Author { get; set; }
        public ApiUniversalStatistics StatisticsSummary { get; set; }
        public ApiUniversalSummary ReviewsSummary { get; set; }
        public MapVersionModel LatestVersion { get; set; }
        public MapVersionModel[] Versions { get; set; }



        public bool TryGetRelationId(ApiDataType type, out int id)
        {
            id = -1;
            var relations = Relations;
            if (relations is not null && relations.Count > 0)
            {
                foreach (var relation in relations)
                {
                    if (relation.Value.Data[0].Type == type)
                    {
                        id = relation.Value.Data[0].Id;
                        return true;
                    }
                }
            }
            return false;
        }
        public Dictionary<ApiDataType, int> GetRelations()
        {
            var dic = new Dictionary<ApiDataType, int>();
            var relations = Relations;
            if (relations is not null)
            {
                foreach (var relation in relations)
                {
                    var data = relation.Value.Data;
                    if (data is null) continue;
                    if (dic.ContainsKey(data[0].Type)) continue;
                    dic.Add(data[0].Type, data[0].Id);
                }
            }
            return dic;
        }
    }
}
