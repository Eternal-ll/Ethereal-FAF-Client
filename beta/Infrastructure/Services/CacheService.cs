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

            if (folder == Folder.MapsLargePreviews)
            {
                image.DecodePixelHeight = 256;
                image.DecodePixelWidth = 256;
            }
            else if (folder == Folder.MapsSmallPreviews)
            {
                image.DecodePixelHeight = 100;
                image.DecodePixelWidth = 100;
            }

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnDemand;

            if (File.Exists(localFilePath))
            {
                image.UriSource = new(localFilePath, UriKind.Absolute);
                image.EndInit();
                image.Freeze();

                return image;
            }
            image.UriSource = uri;
            image.EndInit();

            void handler(object s, EventArgs e)
            {
                image.DownloadCompleted -= handler;

                PngBitmapEncoder encoder = new();
                encoder.Frames.Add(BitmapFrame.Create(image));

                using FileStream filestream = new(localFilePath, FileMode.Create);

                encoder.Save(filestream);
                image.Freeze();
            }

            image.DownloadCompleted += handler; 

            return image;
        }
    }
}
