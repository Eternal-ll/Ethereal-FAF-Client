using Ethereal.FAF.API.Client.Models.Attributes;
using Ethereal.FAF.API.Client.Models.Universal;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.API.Client.Models.MapsVault
{
    public interface IVaultEntity
    {
        public bool IsHidden { get; }
        public bool HasVersion { get; }
    }
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
    public class MapPoolAssignmentParams
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("spawn")]
        public int SpawnsCount { get; set; }

        [JsonPropertyName("type")]
        public int Size { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
    /// <summary>
    /// https://api.faforever.com/data/mapPoolAssignment
    /// </summary>
    public class ApiMapPoolAssignment : Base.ApiUniversalData
    {
        public DateTime CreateTime => DateTime.Parse(Attributes["createTime"]);
        public DateTime UpdateTime => DateTime.Parse(Attributes["updateTime"]);
        public int Weight => int.Parse(Attributes["weight"]);
        //MapPoolAssignmentParams Params => Attributes.TryGetValue("params", out var parameters) && !string.IsNullOrWhiteSpace(parameters) ?
        //    JsonSerializer.Deserialize<MapPoolAssignmentParams>(parameters) : null;
        public MapVersionModel LatestVersion { get; set; }
    }
    public class ApiMapModel : ApiMap, IVaultEntity
    {
        public string Author { get; set; } = "Unknown";

        public string SmallPreviewUrl => LatestVersion is null ?
            $"https://via.placeholder.com/60x60?text={DisplayedName}.png" :
            LatestVersion.ThumbnailUrlLarge.Replace("faforever.ru", "content.faforever.ru");


        public ApiUniversalStatistics StatisticsSummary { get; set; }
        public ApiUniversalSummary ReviewsSummary { get; set; }
        public MapVersionModel LatestVersion { get; set; }
        public MapVersionModel[] Versions { get; set; }

        public bool IsHidden => HasVersion && LatestVersion.IsHidden;
        public bool HasVersion => LatestVersion is not null;
    }
}
