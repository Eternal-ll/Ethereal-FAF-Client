using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        public BitmapImage GetImage(Uri uri, Folder folder = Folder.Common)
        {
            if (uri == null)
                throw new ArgumentNullException();

            var cacheFolder = App.GetPathToFolder(folder);

            if (!Directory.Exists(cacheFolder))
                Directory.CreateDirectory(cacheFolder);

            var localFilePath = cacheFolder + uri.Segments[^1];

            BitmapImage image = new();
            image.CacheOption = BitmapCacheOption.OnDemand;

            //if (folder == Folder.MapsLargePreviews)
            //{
            //    image.DecodePixelHeight = 512;
            //    image.DecodePixelWidth = 512;
            //}
            //else
            //{
            //    image.DecodePixelHeight = 100;
            //    image.DecodePixelWidth = 100;
            //}

            image.BeginInit();

            if (File.Exists(localFilePath))
            {
                image.UriSource = new(localFilePath, UriKind.Absolute);
                image.EndInit();
                image.Freeze();

                return image;
            }
            image.UriSource = uri;
            image.EndInit();
            image.DownloadCompleted += (s, e) =>
            {
                PngBitmapEncoder encoder = new();
                encoder.Frames.Add(BitmapFrame.Create(image));

                using FileStream filestream = new(localFilePath, FileMode.Create);

                encoder.Save(filestream);
                image.Freeze();
            };

            return image;
        }
    }
}
