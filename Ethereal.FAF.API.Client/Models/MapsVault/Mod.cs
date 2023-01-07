using beta.Models.API.Universal;

namespace beta.Models.API.MapsVault
{
    public class Mod : Base.ApiUniversalData, IVaultEntity
    {
        public string Author => Attributes["author"];
        public string Name => Attributes["displayName"];
        public bool IsRecommended => bool.Parse(Attributes["recommended"]);
        public DateTime UpdateTime => DateTime.Parse(Attributes["updateTime"]);
        public DateTime CreateTime => DateTime.Parse(Attributes["createTime"]);
        public string Uploader { get; set; }
        public ApiUniversalSummary ReviewsSummary { get; set; }
        public ModVersion[] Versions { get; set; }
        public ModVersion LatestVersion { get; set; }

        public bool IsHidden => HasVersion && LatestVersion.IsHidden;
        public bool HasVersion => LatestVersion is not null;
    }
    public enum ModType
    {
        UI,
        SIM,
    }
    public class ModVersion : Base.ApiUniversalData
    {
        public string Uid => Attributes["uid"];
        public ModType ModType => Enum.Parse<ModType>(Attributes["type"]);
        public int Version => int.Parse(Attributes["version"]);
        public string Description => Attributes["description"];
        public string DownloadUrl => Attributes["downloadUrl"];
        public string Filename => Attributes["filename"];
        public bool IsHidden => bool.Parse(Attributes["hidden"]);
        public bool IsRanked => bool.Parse(Attributes["ranked"]);
        public string ThumbnailUrl => Attributes["thumbnailUrl"];
        public DateTime Created => DateTime.Parse(Attributes["createTime"]);
        public DateTime Updated => DateTime.Parse(Attributes["updateTime"]);

        public ApiUniversalStatistics Statistics { get; set; }
        public ApiUniversalSummary Summary { get; set; }
    }
}
