using beta.Infrastructure.Utils;
using beta.Models.API.Base;
using beta.Models.Enums;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace beta.Models.API
{
    public class ApiMapData : ApiUniversalData
    {
        #region Attributes getters
        public string DisplayedName => Attributes?["displayName"];
        public string BattleType => Attributes?["battleType"];
        public string MapType => Attributes?["mapType"];
        public string GamesPlayed => Attributes?["gamesPlayed"];
        public string IsRecommended => Attributes?["recommended"];

        public string CreateTime => Attributes?["createTime"];
        public string UpdateTime => Attributes?["updateTime"];
        #endregion

        #region Custom properties
        public bool IsLegacyMap { get; set; }
        public LocalMapState LocalState { get; set; }

        private BitmapImage _MapSmallPreview;
        public ImageSource MapSmallPreview => _MapSmallPreview ??= ImageTools.InitializeLazyBitmapImage(ThumbnailUrlSmall, 100, 100);

        private ImageSource _MapLargePreview;
        public ImageSource MapLargePreview => _MapLargePreview ??= ImageTools.InitializeLazyBitmapImage(ThumbnailUrlLarge);
        #endregion


        public Dictionary<string, string> AuthorData { get; set; }
        #region Author data getters
        private string _AuthorLogin;
        public string AuthorLogin
        {
            get
            {
                if (_AuthorLogin is not null) return _AuthorLogin;

                if (AuthorData is not null)
                {
                    _AuthorLogin = AuthorData["login"];
                    AuthorData = null;
                    return _AuthorLogin;
                }

                return "Unknown";
            }
            set => _AuthorLogin = value;
        }
        #endregion

        public Dictionary<string, string> MapData { get; set; }
        #region Map data getters
        public string Description
        {
            get
            {
                var desc = MapData?["description"];
                if (desc is null) return null;

                var del = desc.IndexOf('>');

                if (del != -1) return desc[(del + 1)..];

                return desc;
            }
        }
        public string IsRanked => MapData?["ranked"] is not null ? MapData["ranked"].ToLower() : null;
        public string Version => MapData?["version"];
        public int? Width
        {
            get
            {
                if (MapData?["width"] is null) return null;

                return Tools.CalculateMapSizeToKm(int.Parse(MapData["width"]));
            }
        }
        public int? Height
        {
            get
            {
                if (MapData?["height"] is null) return null;

                return Tools.CalculateMapSizeToKm(int.Parse(MapData["height"]));
            }
        }

        public string MapSize => $"{Width}x{Height} km";
        public bool? IsHidden => bool.TryParse(MapData?["hidden"], out var result) ? result : null;
        public string FolderName => MapData?["folderName"];
        public string MaxPlayers => MapData?["maxPlayers"];
        public string DownloadUrl => MapData?["downloadUrl"];
        public string ThumbnailUrlLarge => MapData?["thumbnailUrlLarge"];
        public string ThumbnailUrlSmall => MapData?["thumbnailUrlSmall"];
        #endregion

        public Dictionary<string, string> ReviewsSummaryData { get; set; }
        #region Reviews summary getters
        public double SummaryPositive => ReviewsSummaryData?["positive"] is null ? 0 : double.Parse(ReviewsSummaryData["positive"].Replace('.', ','));
        public int SummaryReviews => ReviewsSummaryData?["reviews"] is null ? 0 : int.Parse(ReviewsSummaryData["reviews"]);
        public double SummaryScore => ReviewsSummaryData?["score"] is null ? 0 : double.Parse(ReviewsSummaryData["score"].Replace('.', ','));
        public double SummaryLowerBound => double.TryParse(ReviewsSummaryData?["lowerBound"].Replace('.', ',').Replace("null", null), out var result) ? result : 0;
        public double SummaryFiveRate => SummaryLowerBound != 0 ? 5 * SummaryLowerBound : -1;
        #endregion
    }
}
