using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services
{
    public class MapService : IMapService
    {
        public event EventHandler<EventArgs<bool>> MapIsDownloaded;

        private readonly ICacheService CacheService;

        public MapService(ICacheService cacheService)
        {
            CacheService = cacheService;
        }

        public void AddDetailedInfo(GameInfoMessage game)
        {
            throw new NotImplementedException();
        }

        public void Download(Uri url)
        {
            throw new NotImplementedException();
        }

        public BitmapImage GetMap(Uri uri, Folder folder = Folder.MapsSmallPreviews) => CacheService.GetImage(uri, folder);
    }
}
