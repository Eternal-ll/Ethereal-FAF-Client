using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Configuration;
using System;
using System.Linq;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    public class ClientManager
    {
        private readonly ISettingsManager _settingsManager;

        public ClientManager(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            if (_settingsManager.Settings.RememberSelectedFaServer)
            {
                SelectedServer = _settingsManager.Settings.Servers.FirstOrDefault(x => x.Name == _settingsManager.Settings.SelectedFaServer);
            }
        }

        public event EventHandler<Server> ServerChanged;
        private Server SelectedServer { get; set; }
        public void SetServer(Server server)
        {
            if (_settingsManager.Settings.RememberSelectedFaServer)
            {
                _settingsManager.Settings.SelectedFaServer = server.Name;
            }
            SelectedServer = server;
            ServerChanged?.Invoke(this, server);
        }
        public Server GetServer() => SelectedServer;
    }
}
