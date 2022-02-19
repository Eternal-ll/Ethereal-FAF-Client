using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services
{
    public class MapService : IMapService
    {
        public event EventHandler<EventArgs<bool>> MapIsDownloaded;

        private readonly List<BitmapImage> Cache = new();

        public void AddDetailedInfo(GameInfoMessage game)
        {
            throw new NotImplementedException();
        }

        public void Download(Uri url)
        {
            throw new NotImplementedException();
        }

        public BitmapImage GetMap(Uri uri, Folder folder = Folder.MapsSmallPreviews)
        {
            var cache = Cache;

            if (folder == Folder.MapsSmallPreviews)
                for (int i = 0; i < cache.Count; i++)
                    if (cache[i].UriSource.Segments[^1] == uri.Segments[^1])
                        return cache[i];

            var cacheFolder = App.GetPathToFolder(folder);

            if (!Directory.Exists(cacheFolder))
                Directory.CreateDirectory(cacheFolder);

            var localFilePath = cacheFolder + uri.Segments[^1];

            if (File.Exists(localFilePath))
            {
                var img = new BitmapImage(new(localFilePath, UriKind.Absolute), new(System.Net.Cache.RequestCacheLevel.NoCacheNoStore));
                img.Freeze();

                if (folder == Folder.MapsSmallPreviews)
                    cache.Add(img);
                return img;
            }

            BitmapImage image = null;

            image = new();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnDemand;
            image.UriSource = uri;
            image.EndInit();
            image.DownloadCompleted += (s, e) =>
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));

                using var filestream = new FileStream(localFilePath, FileMode.Create);

                encoder.Save(filestream);
                image.Freeze();
                if (folder == Folder.MapsSmallPreviews)
                    cache.Add(image);
            };

            return image;
        }
    }
}
