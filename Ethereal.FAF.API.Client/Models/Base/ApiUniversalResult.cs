using Ethereal.FAF.API.Client.Models.Enums;
using Ethereal.FAF.API.Client.Models.MapsVault;
using Ethereal.FAF.API.Client.Models.Universal;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.Base
{
    public class ApiUniversalResult<T> where T : class
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("included")]
        public ApiUniversalData[] Included { get; set; }

        [JsonPropertyName("relationships")]
        public Dictionary<string, ApiUniversalArrayRelationship> Relations { get; set; }

        [JsonPropertyName("meta")]
        public ApiVaultMeta Meta { get; set; }

        public virtual void ParseIncluded()
        {

        }
        public virtual void ParseIncluded(out SortedDictionary<string, string[]> entityProperties)
        {
            entityProperties = null;
        }
    }
    public class ApiListResult<T>
    {
        [JsonPropertyName("data")]
        public List<ApiUniversalData<T>> Data { get; set; }

        [JsonPropertyName("included")]
        public ApiUniversalData[] Included { get; set; }

        [JsonPropertyName("relationships")]
        public Dictionary<string, ApiUniversalArrayRelationship> Relations { get; set; }

        [JsonPropertyName("meta")]
        public ApiVaultMeta Meta { get; set; }
    }
    public class ModsResult : ApiUniversalResult<Mod[]>
    {
        public override void ParseIncluded()
        {
            var included = Included;
            if (included is null || Data is null) return;
            if (included.Length == 0 || Data.Length == 0) return;
            foreach (var mod in Data)
            {
                if (mod.Relations is null) continue;
                foreach (var relation in mod.Relations)
                {
                    if (relation.Value.Data is null) continue;
                    if (relation.Value.Data.Count == 0) continue;
                    var entity = ApiUniversalTools.GetDataFromIncluded(included, relation.Value.Data[0].Type, relation.Value.Data[0].Id);
                    if (entity is null) continue;

                    switch (relation.Value.Data[0].Type)
                    {
                        case ApiDataType.modVersion:
                            mod.LatestVersion = entity.CastTo<ModVersion>();
                            //if (map.LatestVersion.IsLegacyMap)
                            //{
                            //    map.LatestVersion.Attributes["hidden"] = "false";
                            //}
                            continue;
                        case ApiDataType.player:
                            mod.Uploader = entity?.Attributes["login"];
                            continue;
                        case ApiDataType.modReviewsSummary:
                            mod.ReviewsSummary = entity?.CastTo<ApiUniversalSummary>();
                            continue;
                    }
                }
            }
        }
    }
    public class ApiMapsResult : ApiUniversalResult<ApiMapModel[]>
    {
        public override void ParseIncluded()
        {
            this.ParseIncluded(out var data);
        }
        public override void ParseIncluded(out SortedDictionary<string, string[]> entityProperties)
        {
            entityProperties = null;
            var included = Included;
            var data = Data;
            if (included is null || data is null) return;
            if (included.Length == 0 || data.Length == 0) return;
            entityProperties = new();
            var first = data.FirstOrDefault();
            if (first is not null)
            {
                entityProperties.Add("map", first.Attributes.Keys.ToArray());
            }
            foreach (var map in data)
            {
                try
                {
                    if (map.Relations is null) continue;
                    foreach (var relation in map.Relations)
                    {
                        if (relation.Value.Data is null) continue;
                        if (relation.Value.Data.Count == 0) continue;
                        var entity = ApiUniversalTools.GetDataFromIncluded(included, relation.Value.Data[0].Type, relation.Value.Data[0].Id);
                        if (entity is null) continue;
                        if (entity is not null)
                        {
                            entityProperties.TryAdd(relation.Key, entity.Attributes.Keys.ToArray());
                        }

                        switch (relation.Value.Data[0].Type)
                        {
                            case ApiDataType.mapVersion:
                                map.LatestVersion = entity.CastTo<MapVersionModel>();
                                //if (map.LatestVersion.IsLegacyMap)
                                //{
                                //    map.LatestVersion.Attributes["hidden"] = "false";
                                //}
                                continue;
                            case ApiDataType.player:
                                map.Author = entity?.Attributes["login"];
                                continue;
                            case ApiDataType.mapReviewsSummary:
                                map.ReviewsSummary = entity?.CastTo<ApiUniversalSummary>();
                                continue;
                            case ApiDataType.mapStatistics:
                                map.StatisticsSummary = entity.CastTo<ApiUniversalStatistics>();
                                continue;
                        }
                    }
                }
                catch
                {

                }
            }
        }
    }
    internal class ApiMapResult : ApiUniversalResult<ApiMapModel>
    {
        public override void ParseIncluded()
        {
            var included = Included;
            if (included is null) return;
            if (included.Length == 0) return;
            List<MapVersionModel> versions = new List<MapVersionModel>();
            foreach (var entity in included)
            {
                if (entity.Type == ApiDataType.mapVersion)
                {
                    versions.Add(entity.CastTo<MapVersionModel>());
                    continue;
                }
                if (entity.Type == ApiDataType.player)
                {
                    Data.Author = entity.Attributes["login"];
                    continue;
                }
                if (entity.Type == ApiDataType.mapStatistics)
                {
                    Data.StatisticsSummary = entity.CastTo<ApiUniversalStatistics>();
                    continue;
                }
                if (entity.Type == ApiDataType.mapReviewsSummary)
                {
                    Data.ReviewsSummary = entity.CastTo<ApiUniversalSummary>();
                    continue;
                }
            }
            if (versions.Count > 0)
            {
                Data.Versions = versions.ToArray();
            }
        }
    }
    internal class ApiMapVersionResult : ApiUniversalResult<MapVersionModel>
    {
        public override void ParseIncluded()
        {
            var included = Included;
            if (included is null) return;
            if (included.Length == 0) return;
            List<UniversalReviewModel> reviews = new();
            Dictionary<int, int> points = new()
            {
                { 5, 0 },
                { 4, 0 },
                { 3, 0 },
                { 2, 0 },
                { 1, 0 },
            };
            Dictionary<int, int> playerToReview = new();
            int i = 0;
            foreach (var entity in included)
            {
                switch (entity.Type)
                {
                    case ApiDataType.mapVersionStatistics:
                        Data.Statistics = entity.CastTo<ApiUniversalStatistics>();
                        break;
                    case ApiDataType.mapVersionReview:
                        var model = entity.CastTo<UniversalReviewModel>();
                        model.ParseIncluded();
                        if (model.AuthorId.HasValue)
                        {
                            playerToReview.Add(model.AuthorId.Value, i);
                        }
                        reviews.Add(model);
                        points[(int)model.Score]++;
                        i++;
                        break;
                    case ApiDataType.mapVersionReviewsSummary:
                        Data.Summary = entity.CastTo<ApiUniversalSummary>();
                        break;
                    case ApiDataType.player:
                        if (playerToReview.TryGetValue(entity.Id, out var index))
                        {
                            reviews[index].Author = entity.Attributes["login"];
                        }
                        break;
                    default:
                        break;
                }
            }

            if (reviews.Count > 0)
            {
                Data.Reviews = reviews.ToArray();
            }
            Data.PointsCount = points;
        }
    }
}
