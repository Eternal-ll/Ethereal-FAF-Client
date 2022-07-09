using beta.Infrastructure.Utils;
using beta.Models.API.Base;
using beta.Models.API.Universal;
using beta.Models.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace beta.Models.API.MapsVault
{
    /// <summary>
    /// API entity of mapVersion table
    /// </summary>
    public class ApiMapVersion : ApiUniversalData
    {
        public DateTime CreateTime => DateTime.Parse(Attributes["createTime"]);
        public string Description => Attributes["description"];
        public Uri DownloadUrl => new Uri(Attributes["downloadUrl"]);
        public string Filename => Attributes["filename"];
        public string FolderName => Attributes["folderName"];
        public int GamesPlayed => int.Parse(Attributes["gamesPlayed"]);
        public int Height => int.Parse(Attributes["height"]);
        public int Width => int.Parse(Attributes["width"]);
        public bool IsHidden => bool.Parse(Attributes["hidden"]);
        public int MaxPlayers => int.Parse(Attributes["maxPlayers"]);
        public bool IsRanked => bool.Parse(Attributes["ranked"]);
        public string ThumbnailUrlLarge => Attributes["thumbnailUrlLarge"];
        public string ThumbnailUrlSmall => Attributes["thumbnailUrlSmall"];
        public DateTime UpdateTime => DateTime.Parse(Attributes["updateTime"]);
        public int Version => int.Parse(Attributes["version"]);
    }
    /// <summary>
    /// Extends <seealso cref="ApiMapVersion"/> for additional properties
    /// </summary>
    public class MapVersionModel : ApiMapVersion
    {
        private bool? _IsLegacyMap;
        /// <summary>
        /// Is map originals from Supreme Commander: Forged Alliance. <seealso cref="LegacyMap"/>
        /// </summary>
        public bool IsLegacyMap => _IsLegacyMap ??= Enum.IsDefined(typeof(LegacyMap),
            //FolderName.Contains('.') ?
            //FolderName.Split('.')[0].ToUpper() :
            FolderName.ToUpper());

        public bool IsLatestVersion { get; set; }
        
        /// <summary>
        /// Local state of map <see cref="LocalMapState"/>
        /// </summary>
        public LocalMapState LocalState { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsDeletable => LocalState switch
        {
            LocalMapState.Unknown => false,
            LocalMapState.NotExist => false,
            LocalMapState.Older => true,
            LocalMapState.Newest => true,
            LocalMapState.Same => true,
            _ => throw new NotImplementedException()
        };

        private BitmapImage _MapSmallPreview;
        /// <summary>
        /// 
        /// </summary>
        public ImageSource MapSmallPreview => _MapSmallPreview ??= ImageTools.InitializeLazyBitmapImage(ThumbnailUrlSmall, 128, 128);

        private ImageSource _MapLargePreview;
        /// <summary>
        /// 
        /// </summary>
        public ImageSource MapLargePreview => _MapLargePreview ??= ImageTools.InitializeLazyBitmapImage(ThumbnailUrlLarge);
        /// <summary>
        /// <seealso cref="Tools.CalculateMapSizeToPixels(int)"/>
        /// </summary>
        public Point MapSizeInPixels => new Point(Width, Height);
        /// <summary>
        /// <seealso cref="Tools.CalculateMapSizeToKm(int)"/>
        /// </summary>
        public Point MapSizeInKm => new Point(Tools.CalculateMapSizeToKm(Width), Tools.CalculateMapSizeToKm(Height));
        /// <summary>
        /// z
        /// </summary>
        public string MapPixelSizeLabel => $"{MapSizeInPixels.X}x{MapSizeInPixels.Y} px";
        /// <summary>
        /// 
        /// </summary>
        public string MapKmSizeLabel => $"{MapSizeInKm.X}x{MapSizeInKm.Y} km";

        public ApiUniversalStatistics Statistics { get; set; }
        public ApiUniversalSummary Summary { get; set; }
        public UniversalReviewModel[] Reviews { get; set; }
        public Dictionary<int, int> PointsCount { get; set; }
        public Dictionary<string, string> Scenario { get; set; }
    }
}
