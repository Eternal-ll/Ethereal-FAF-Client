using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class LocalMapsVM : MapsHostingVM
    {
        public LocalMapsVM(LobbyClient lobbyClient, ContainerViewModel container, PatchClient patchClient,
            IceManager iceManager, NotificationService notificationService, IConfiguration configuration)
            : base(lobbyClient, container, patchClient, iceManager, configuration, notificationService)
        {
            Task.Run(async () =>
            {
                var maps = new List<LocalMap>();
                string[] scenarios = Directory.GetFiles(Configuration.GetMapsFolder(), "*_scenario.lua", SearchOption.AllDirectories);
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
