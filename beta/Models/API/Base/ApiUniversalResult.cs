using beta.Models.API.Enums;
using beta.Models.API.MapsVault;
using beta.Models.API.Universal;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace beta.Models.API.Base
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

        public virtual void ParseIncluded() {}
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
