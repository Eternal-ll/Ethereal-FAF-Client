using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Enums;
using beta.Models.Server;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading.Tasks;
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
            var path = image.BaseUri.AbsolutePath.Replace("%2520", " ");
            using FileStream filestream = new(path, FileMode.Create);
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

        private readonly ConcurrentDictionary<Uri, byte[]> Cache = new();

        public async Task SetMapSource(GameMap game, Uri uri, Folder folder) => game.ImageSource = await GetBitmapSource(uri, folder);

        public async Task<byte[]> GetBitmapSource(Uri uri, Folder folder)
        {
            if (Cache.TryGetValue(uri, out var cache)) return cache;

            var cacheFolder = App.GetPathToFolder(folder);
            // TODO do we need this check so often?
            if (!Directory.Exists(cacheFolder))
                Directory.CreateDirectory(cacheFolder);

            var localFilePath = cacheFolder + uri.Segments[^1];

            if (File.Exists(localFilePath))
            {
                return Cache.GetOrAdd(uri, await File.ReadAllBytesAsync(localFilePath));
            }
            else
            {
                try
                {
                    WebRequest request = WebRequest.Create(uri);
                    using var response = await request.GetResponseAsync();
                    using MemoryStream ms = new();
                    await response.GetResponseStream().CopyToAsync(ms);
                    var data = ms.ToArray();

                    using FileStream filestream = new(localFilePath, FileMode.Create);
                    await filestream.WriteAsync(data);
                    return Cache.GetOrAdd(uri, data);
                }
                catch(Exception ex)
                {
                    return Array.Empty<byte>();
                }
                return null;
            }
        }
    }
}
