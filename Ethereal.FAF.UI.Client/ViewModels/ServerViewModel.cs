using Ethereal.FAF.UI.Client.Infrastructure.IRC;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class ServerViewModel : Base.ViewModel
    {
        private readonly string Name;

        private readonly LobbyClient LobbyClient;
        private readonly IrcClient IrcClient;

        private readonly IConfiguration Configuration;
        private readonly IOptionsMonitor<Server> ServerOptionsMonitor;
        private readonly IDisposable ServerConfigurationListener;

        public ServerViewModel(string server, IConfiguration configuration, IOptionsMonitor<Server> serverOptionsMonitor)
        {
            Name = server;

            ServerConfigurationListener = serverOptionsMonitor.OnChange(OnServerConfigurationChange);


            Configuration = configuration;
            ServerOptionsMonitor = serverOptionsMonitor;
        }

        public void OnServerConfigurationChange(Server server, string data)
        {
            Server = server;
        }

        #region Server
        private Server _Server;
        public Server Server
        {
            get => _Server;
            set
            {
                if (Set(ref _Server, value))
                {

                }
            }
        }
        #endregion


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerConfigurationListener.Dispose();
            }
        }
    }
}
    