using beta.Models;
using System;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IMapService
    {
        public event EventHandler<EventArgs<bool>> MapIsDownloaded;

        public Map GetMap(Uri uri, bool attachScenario = true);
        public BitmapImage GetMapPreview(Uri uri, Folder folder = Folder.MapsSmallPreviews);
        public void AttachMapScenario(ref Map map);
        public void Download(Uri url);
    }
}
