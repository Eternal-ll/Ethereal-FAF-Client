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
            // TODO any ideas of usage?
            if (uri is null)
                throw new ArgumentNullException();

            // TODO that have to be fixed somehow, save it to local property
            // but it is wrong on startup
            var cacheFolder = App.GetPathToFolder(folder);

            // TODO do we need this check so often?
            if (!Directory.Exists(cacheFolder))
                Directory.CreateDirectory(cacheFolder);

            var localFilePath = cacheFolder + uri.Segments[^1];

            BitmapImage image = new();
            image.BeginInit();

            SetDecodePixels(image, folder);

            image.CacheOption = BitmapCacheOption.OnDemand;

            if (File.Exists(localFilePath))
            {
                image.UriSource = new(localFilePath, UriKind.Absolute);
                image.EndInit();
                image.Freeze();

                return image;
            }
            image.BaseUri = new(localFilePath);
            image.UriSource = uri;
            image.EndInit();

            image.DownloadCompleted += OnImageDonwloadComplete; 

            return image;
        }

        private static void OnImageDonwloadComplete(object s, EventArgs e)
        {
            var image = (BitmapImage)s;
            image.DownloadCompleted -= OnImageDonwloadComplete;

            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using FileStream filestream = new(image.BaseUri.AbsolutePath, FileMode.Create);
            encoder.Save(filestream);
            image.Freeze();
        }

        /// <summary>
        /// Sets decode pixel height/width for image based on target local cache folder
        /// </summary>
        /// <param name="image">Image</param>
        /// <param name="folder">Target local cache folder</param>
        private static void SetDecodePixels(BitmapImage image, Folder folder)
        {
            switch (folder)
            {
                case Folder.Common:
                    return;
                case Folder.MapsSmallPreviews:
                    image.DecodePixelHeight = 100;
                    image.DecodePixelWidth = 100;
                    return;
                case Folder.MapsLargePreviews:
                    image.DecodePixelHeight = 256;
                    image.DecodePixelWidth = 256;
                    return;
                case Folder.Game:
                    return;
                case Folder.Mods:
                    return;
                case Folder.Maps:
                    return;
                case Folder.ProgramData:
                    return;
                case Folder.PlayerAvatars:
                    return;
                case Folder.Emoji:
                    return;
                default:
                    return;
            }
        }
    }
}
