using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class LocalMapsVM : MapsHostingVM
    {
        private readonly string MapsDirectory;

        public LocalMapsVM(LobbyClient lobbyClient, ContainerViewModel container, PatchClient patchClient,
            IceManager iceManager)
            : base(lobbyClient, container, patchClient, iceManager)
        {
            MapsDirectory = FaPaths.Maps;
            //MapsDirectory = new DirectoryInfo(mapsDirectory);

            Task.Run(async () =>
            {
                var maps = new List<LocalMap>();
                string[] scenarios = Directory.GetFiles(MapsDirectory, "*_scenario.lua", SearchOption.AllDirectories);
                var i = 0;
                SetSource(maps);
                foreach (var file in scenarios)
                {
                    var map = new LocalMap();
                    map.FolderName = file.Split('/', '\\')[^2];
                    map.Scenario = FA.Vault.MapScenario.FromFile(file);
                    var preview = file.Replace("_scenario.lua", ".png");
                    var scmapPath = file.Replace("_scenario.lua", ".scmap");
                    map.Preview = preview;
                    if (!File.Exists(preview))
                    {
                        var scmap = FA.Vault.Scmap.FromFile(scmapPath);
                    }
                    i++;
                    if (Disposed) break;
                    AddMap(map);
                    await Task.Delay(50);
                }
            });
        }
    }
}
