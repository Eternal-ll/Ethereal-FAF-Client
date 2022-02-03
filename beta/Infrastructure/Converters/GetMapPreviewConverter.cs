using beta.Models.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Converters
{
    public class GetMapPreviewConverter : IValueConverter
    {
        private readonly IList<KeyValuePair<string, object>> Cache = new List<KeyValuePair<string, object>>();
        private readonly string cacheFolder = App.GetPathToFolder(Folder.MapsSmallPreviews);

        private object Test(string mapName)
        {
            var lenght = Cache.Count;
            for (int i = 0; i < lenght; i++)
            {
                if (Cache[i].Key == mapName) return Cache[i].Value;
            }

            //if (!Directory.Exists(cacheFolder))
            //    Directory.CreateDirectory(cacheFolder);

            var localFilePath = cacheFolder + mapName + ".png";
            BitmapImage image = null;
            if (File.Exists(localFilePath))
            {
                image = new BitmapImage(new Uri(localFilePath, UriKind.Absolute));
                image.DecodePixelHeight = 90;
                image.DecodePixelWidth = 90;
                image.Freeze();

                Cache.Add(new(mapName, image));
                return image;
            }

            var uri = new Uri("https://content.faforever.com/maps/previews/small/" + mapName + ".png", UriKind.Absolute);
            image = new BitmapImage(uri, new(System.Net.Cache.RequestCacheLevel.BypassCache));
            image.DownloadCompleted += (sender, arg) =>
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                using var fileStream = new FileStream(localFilePath, FileMode.Create);
                encoder.Save(fileStream);
                image.DecodePixelWidth = 90;
                image.DecodePixelWidth = 90;
                image.Freeze();
                Cache.Add(new(mapName, image));
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
