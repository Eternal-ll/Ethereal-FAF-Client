using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services
{
    public class AvatarService : IAvatarService
    {
        private readonly ISessionService SessionService;
        private readonly List<BitmapImage> Cache = new();

        public AvatarService(ISessionService sessionService)
        {
            SessionService = sessionService;
        }

        public BitmapImage GetAvatar(Uri uri)
        {
            var cache = Cache;

            for (int i = 0; i < cache.Count; i++)
                if (cache[i].UriSource.Segments[^1] == uri.Segments[^1])
                    return cache[i];

            var cacheFolder = App.GetPathToFolder(Folder.PlayerAvatars);
            var localFilePath = cacheFolder + uri.Segments[^1];

            if (File.Exists(localFilePath))
            {
                var img = new BitmapImage(new(localFilePath, UriKind.Absolute), new(System.Net.Cache.RequestCacheLevel.NoCacheNoStore));
                img.Freeze();

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
                cache.Add(image);
            };

            return image;
        }

        public void SetAvatar()
        {
            throw new NotImplementedException();
        }
    }
}
