using beta.Infrastructure.Utils;
using beta.Models.API.Base;
using beta.Models.Enums;
using System;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace beta.Models.API.MapsVault
{
    internal class ApiMapVersion : ApiUniversalData
    {
        public DateTime CreateTime => DateTime.Parse(Attributes["createTime"]);
        public string Description => Attributes["description"];
        public Uri DownloadUrl => new Uri(Attributes["downloadUrl"]);
        public string Filename => Attributes["filename"];
        public string folderName => Attributes["folderName"];
        public int GamesPlayed => int.Parse(Attributes["gamesPlayed"]);
        public int Height => int.Parse(Attributes["height"]);
        public int Width => int.Parse(Attributes["width"]);
        public bool IsHidden => bool.Parse(Attributes["hidden"]);
        public int MaxPlayers => int.Parse(Attributes["maxPlayers"]);
        public bool IsRanked => bool.Parse(Attributes["ranked"]);
        public string thumbnailUrlLarge => Attributes["thumbnailUrlLarge"];
        public string thumbnailUrlSmall => Attributes["thumbnailUrlSmall"];
        public DateTime UpdateTime => DateTime.Parse(Attributes["updateTime"]);
        public int Version => int.Parse(Attributes["version"]);
    }
    internal class MapVersionModel : ApiMapVersion
    {
        private bool? _IsLegacyMap;
        /// <summary>
        /// Is map originals from Supreme Commander: Forged Alliance. <seealso cref="LegacyMap"/>
        /// </summary>
        public bool IsLegacyMap => _IsLegacyMap ??= Enum.IsDefined(typeof(LegacyMap),
            folderName.Contains('.') ?
            folderName.Split('.')[0].ToUpper() :
            folderName.ToUpper());
        
        //public LocalMapState LocalState { get; set; }

        private BitmapImage _MapSmallPreview;
        /// <summary>
        /// 
        /// </summary>
        public ImageSource MapSmallPreview => _MapSmallPreview ??= ImageTools.InitializeLazyBitmapImage(thumbnailUrlLarge, 100, 100);

        private ImageSource _MapLargePreview;
        /// <summary>
        /// 
        /// </summary>
        public ImageSource MapLargePreview => _MapLargePreview ??= ImageTools.InitializeLazyBitmapImage(thumbnailUrlSmall);
        /// <summary>
        /// <seealso cref="Tools.CalculateMapSizeToPixels(int)"/>
        /// </summary>
        public Point MapSizeInPixels => new Point(Width, Height);
        /// <summary>
        /// <seealso cref="Tools.CalculateMapSizeToKm(int)"/>
        /// </summary>
        public Point MapSizeInKm => new Point(Tools.CalculateMapSizeToKm(Width), Tools.CalculateMapSizeToKm(Height));
        /// <summary>
        /// 
        /// </summary>
        public string MapPixelSizeLabel => $"{MapSizeInPixels.X}x{MapSizeInPixels.Y} px";
        /// <summary>
        /// 
        /// </summary>
        public string MapKmSizeLabel => $"{MapSizeInKm.X}x{MapSizeInKm.Y} km";
    }
}
