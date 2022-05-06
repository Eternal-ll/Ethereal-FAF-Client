using beta.Models;
using beta.Models.Enums;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Local cache service
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets image locally from <see cref="Folder"/> or downloading to <paramref name="folder"/> and returns image
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public BitmapImage GetImage(Uri uri, Folder folder);
        /// <summary>
        /// Set preview image to GameMap
        /// </summary>
        /// <param name="game"></param>
        /// <param name="uri"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public Task SetMapSource(GameMap game, Uri uri, Folder folder);

        /// <summary>
        /// Get bitmap image source in bytes
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public Task<byte[]> GetBitmapSource(Uri uri, Folder folder);
    }
}
