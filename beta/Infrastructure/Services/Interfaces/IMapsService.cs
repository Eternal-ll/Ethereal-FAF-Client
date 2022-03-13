using beta.Models;
using beta.Models.Enums;
using beta.Models.Server;
using System;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IMapsService
    {
        public GameMap GetMap(Uri uri, bool attachScenario = true);
        public Map GetMap(Uri uri, PreviewType previewType, bool attachScenario = true);
        public BitmapImage GetMapPreview(Uri uri, Folder folder = Folder.MapsSmallPreviews);
        public void AttachMapScenario(GameMap map);
        public void Download(Uri url);


        public bool IsLegacyMap(string name);
        public LocalMapState CheckLocalMap(string name);
    }
}
