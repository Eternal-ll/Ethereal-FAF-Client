using beta.Models.Server;
using System;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IMapService
    {
        public event EventHandler<EventArgs<bool>> MapIsDownloaded;

        public BitmapImage GetMap(Uri uri);
        public void AddDetailedInfo(GameInfoMessage game);
        public void Download(Uri url);
    }
}
