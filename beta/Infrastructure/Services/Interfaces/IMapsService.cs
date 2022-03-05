using beta.Models;
using System;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IMapsService
    {
        public GameMap GetMap(Uri uri, bool attachScenario = true);
        public BitmapImage GetMapPreview(Uri uri, Folder folder = Folder.MapsSmallPreviews);
        public void AttachMapScenario(GameMap map);
        public void Download(Uri url);
    }
}
