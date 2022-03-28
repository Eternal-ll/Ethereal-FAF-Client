using beta.Models;
using beta.Models.Enums;
using beta.Models.Server;
using beta.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IMapsService
    {
        public event EventHandler<string> DownloadCompleted;
        public GameMap GetMap(Uri uri, bool attachScenario = true);
        public Map GetMap(Uri uri, PreviewType previewType, bool attachScenario = true);
        public BitmapImage GetMapPreview(Uri uri, Folder folder = Folder.MapsSmallPreviews);
        public void AttachMapScenario(GameMap map);
        public Task<DownloadViewModel> Download(Uri uri);

        public bool IsLegacyMap(string name);
        public LocalMapState CheckLocalMap(string name);
        public string[] GetLocalMaps();
    }
}
