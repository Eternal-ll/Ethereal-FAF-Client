using Ethereal.FA.Scmap;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class LocalMapsVM : MapsHostingVM
    {
        private readonly string MapsDirectory;
        private readonly string SmallMapsPreviewsFolder;

        public LocalMapsVM(string mapsDirectory, string smallMapsPreviewsFolder, LobbyClient lobbyClient, ContainerViewModel container, PatchClient patchClient,
            IceManager iceManager)
            : base(lobbyClient, container, patchClient, iceManager)
        {
            MapsDirectory = mapsDirectory;
            SmallMapsPreviewsFolder = smallMapsPreviewsFolder;
            //MapsDirectory = new DirectoryInfo(mapsDirectory);

            Task.Run(() =>
            {
                var maps = new List<LocalMap>();
                string[] scenarios = Directory.GetFiles(mapsDirectory, "*_scenario.lua", SearchOption.AllDirectories);
                foreach (var file in scenarios)
                {
                    var map = new LocalMap();
                    map.FolderName = file.Split('/', '\\')[^2];
                    map.Scenario = MapScenario.FromFile(file);
                    var preview = file.Replace("_scenario.lua", ".png");
                    var scmapPath = file.Replace("_scenario.lua", ".scmap");
                    map.Preview = preview;
                    if (!File.Exists(preview))
                    {
                        var scmap = Scmap.FromFile(scmapPath);
                    }
                    maps.Add(map);
                }
                SetSource(maps);
            });
        }
    }
}
