using beta.Models;
using System;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface ICacheService
    {
        /// <summary>
        /// Gets image locally from <see cref="Folder"/> or downloading to <paramref name="folder"/> and returns image
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public BitmapImage GetImage(Uri uri, Folder folder);
    }
}
