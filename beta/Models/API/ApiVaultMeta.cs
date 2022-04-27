using beta.Infrastructure.Utils;
using beta.Models.API.Base;
using beta.Models.API.Enums;
using beta.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace beta.Models.API
{
    public class ApiVaultMeta
    {
        [JsonPropertyName("page")]
        public ApiVaultPage Page { get; set; }
    }
    public class ApiMapData : ApiUniversalWithRelations
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
        public ImageSource MapSmallPreview
        {
            get
            {
                if (_MapSmallPreview is null)
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.DecodePixelWidth = 100;
                    img.DecodePixelHeight = 100;
                    img.CacheOption = BitmapCacheOption.OnDemand;
                    img.UriCachePolicy = new(System.Net.Cache.RequestCacheLevel.CacheIfAvailable);
                    img.UriSource = new(ThumbnailUrlSmall);
                    img.EndInit();
                    _MapSmallPreview = img;
                }
                return _MapSmallPreview;
            }
        }
        private ImageSource _MapLargePreview;
        public ImageSource MapLargePreview
        {
            get
            {
                if (_MapLargePreview is null)
                {
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.DecodePixelWidth = 100;
                    img.DecodePixelHeight = 100;
                    img.CacheOption = BitmapCacheOption.OnDemand;
                    img.UriCachePolicy = new(System.Net.Cache.RequestCacheLevel.CacheIfAvailable);
                    img.UriSource = new(ThumbnailUrlLarge);
                    img.EndInit();
                    _MapLargePreview = img;
                }
                return _MapLargePreview;
            }
        }

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
        public string IsRanked => MapData?["ranked"];
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

        #endregion
    }

    public class ApiAvatarAssignmentsResult : ApiUniversalResult<ApiAvatarAssignment>
    {
        [JsonPropertyName("included")]
        public ApiUniversalWithAttributes[] Included { get; set; }
    }

    public class ApiFeaturedModFileResults
    {
        [JsonPropertyName("data")]
        public ApiFeaturedModFileData[] Data { get; set; }
    }

    public class ApiUniversalResults
    {
        [JsonPropertyName("data")]
        public ApiMapData[] Data { get; set; }

        [JsonPropertyName("included")]
        public ApiUniversalWithAttributes[] Included { get; set; }

        public Dictionary<string, string> GetAttributesFromIncluded(ApiDataType type, int id)
        {
            if (Included is null) return null;

            for (int i = 0; i < Included.Length; i++)
            {
                var item = Included[i];

                if (item.Type != type) continue;

                if (item.Id == id)
                    return item.Attributes;
            }

            return null;
        }

        [JsonPropertyName("meta")]
        public ApiVaultMeta Meta { get; set; }
    }
    public class ApiMapResult
    {
        [JsonPropertyName("data")]
        public ApiMapData Data { get; set; }
    }

    public class DictionaryStringConverter : JsonConverter<Dictionary<string, string>>
    {
        public override Dictionary<string, string> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var dictionary = new Dictionary<string, string>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("JsonTokenType was not PropertyName");
                }

                var propertyName = reader.GetString();

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new JsonException("Failed to get property name");
                }

                reader.Read();

                // Add to dictionary.
                dictionary.Add(propertyName, Encoding.UTF8.GetString(reader.ValueSpan));
            }

            throw new JsonException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            Dictionary<string, string> dictionary,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
