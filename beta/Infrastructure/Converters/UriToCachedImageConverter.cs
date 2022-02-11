using beta.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Cache;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Converters
{
    public class UriToCachedImageConverter : IValueConverter
    {
        // TODO: Move to service !!!
        private readonly IList<BitmapImage> Cache = new List<BitmapImage>();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            Folder folder = (Folder)parameter;

            Uri url = (Uri)value;
            string fileName = url.Segments[^1];

            for (int i = 0; i < Cache.Count; i++)
            {
                var cache = Cache[i];
                if (cache.UriSource.Segments[^1] == fileName)
                    return cache;
            }

            var cacheFolder = App.GetPathToFolder(folder);

            if (!Directory.Exists(cacheFolder))
                Directory.CreateDirectory(cacheFolder);

            BitmapImage image = new();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;

            RequestCachePolicy cachePolicy = new(RequestCacheLevel.NoCacheNoStore);

            var localFilePath = cacheFolder + fileName;
            if (File.Exists(localFilePath))
            {
                image.UriSource = new Uri(localFilePath, UriKind.Absolute);
                image.EndInit();

                Cache.Add(image);
                return image;
            }
            image.UriSource = url;
            image.EndInit();

            image.DownloadCompleted += (sender, args) =>
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)sender));

                using var filestream = new FileStream(localFilePath, FileMode.Create);

                encoder.Save(filestream);
                Cache.Add(image);
                image.DownloadCompleted -= (sender, args) => { };
            };

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
