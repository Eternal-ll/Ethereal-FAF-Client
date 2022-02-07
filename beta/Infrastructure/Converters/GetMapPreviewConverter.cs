using beta.Models;
using beta.Models.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net.Cache;

namespace beta.Infrastructure.Converters
{
    public class GetMapPreviewConverter : IValueConverter
    {
        private readonly IList<BitmapImage> Cache = new List<BitmapImage>();

        private object Test(string mapName)
        {
            mapName += ".png";
            for (int i = 0; i < Cache.Count; i++)
            {
                var cache = Cache[i];
                if (cache.UriSource.Segments[^1] == mapName)
                    return cache;
            }

            var cacheFolder = App.GetPathToFolder(Folder.MapsSmallPreviews);

            if (!Directory.Exists(cacheFolder))
                Directory.CreateDirectory(cacheFolder);

            BitmapImage image = new();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;

            RequestCachePolicy cachePolicy = new(RequestCacheLevel.NoCacheNoStore);

            var localFilePath = cacheFolder + mapName;
            if (File.Exists(localFilePath))
            {
                image.UriSource = new Uri(localFilePath, UriKind.Absolute);
                image.EndInit();
                image.DecodePixelHeight = 90;
                image.DecodePixelWidth = 90;
                image.Freeze();

                Cache.Add(image);
                return image;
            }
            image.UriSource = new Uri("https://content.faforever.com/maps/previews/small/" + mapName);
            image.EndInit();
            image.DecodePixelHeight = 90;
            image.DecodePixelWidth = 90;

            image.DownloadCompleted += (sender, args) =>
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)sender));

                using var filestream = new FileStream(localFilePath, FileMode.Create);

                encoder.Save(filestream);
                Cache.Add(image);
                image.DownloadCompleted -= (sender, args) => { };
                image.Freeze();
            };
            
            return image;

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value ==null)return null;
            var MapNameType = (KeyValuePair<string, PreviewType>)value;
            return MapNameType.Value switch
            {
                PreviewType.Normal => Test(MapNameType.Key),
                PreviewType.Coop => App.Current.Resources["CoopIcon"],
                PreviewType.Neroxis => App.Current.Resources["MapGenIcon"],
                _ => App.Current.Resources["QuestionIcon"]
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
