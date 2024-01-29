using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Api;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.IRC;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    public class ServerManager : IDisposable
    {
        #region LobbyClient proxy event handlres
        public EventHandler<bool> LobbyAuthorized { get; set; }
        public EventHandler<Game> GameReceived;
        public EventHandler<Game[]> GamesReceived;

        public EventHandler<Welcome> WelcomeDataReceived;
        public EventHandler<Player> PlayerReceived;
        public EventHandler<Player[]> PlayersReceived;
        #endregion

        #region IrcClient proxy event handlers
        public EventHandler<bool> IrcAuthorized;
        public EventHandler<string> IrcUserConnected;
        public EventHandler<(string user, string id)> IrcUserDisconnected;
        public EventHandler<(string channel, string user)> IrcUserJoined;
        public EventHandler<(string channel, string user)> IrcUserLeft;
        public EventHandler<(string user, string to)> IrcUserChangedName;
        public EventHandler<(string channel, string from, string message)> IrcChannelMessageReceived;
        public EventHandler<(string channel, string topic, string by)> IrcChannelTopicUpdated;
        public EventHandler<(string channel, string user, string at)> IrcChannelTopicChangedBy;
        public EventHandler<(string channel, string[] users)> IrcChannelUsersReceived;
        public EventHandler<(string channel, int users)[]> IrcAvailableChannels;
        public EventHandler<string> IrcNotificationMessageReceived;
        #endregion

        private readonly ILogger<ServerManager> Logger;
        private readonly PatchClient PatchClient;
        private readonly ClientManager _clientManager;
        private readonly IServiceProvider _serviceProvider;

        private readonly LobbyClient LobbyClient;
        private readonly IrcClient IrcClient;
        private IFafApiClient FafApiClient;
        private IFafContentClient FafContentClient;
        public Server Server { get; private set; }
        public Player Self { get; private set; }

        public ServerManager(PatchClient patchClient, ILogger<ServerManager> logger, IrcClient ircClient, LobbyClient lobbyClient, ClientManager clientManager, IServiceProvider serviceProvider)
        {
            PatchClient = patchClient;
            Logger = logger;
            IrcClient = ircClient;
            LobbyClient = lobbyClient;

            LobbyClient.WelcomeDataReceived += LobbyClient_WelcomeDataReceived1;
            LobbyClient.IrcPasswordReceived += LobbyClient_IrcPasswordReceived;
            LobbyClient.WelcomeDataReceived += LobbyClient_WelcomeDataReceived;
            _clientManager = clientManager;
            _serviceProvider = serviceProvider;
        }

        public string GetApiDomain() => Server?.Api?.Host;
        public Server GetServer() => Server;
        public LobbyClient GetLobbyClient() => LobbyClient;
        public IrcClient GetIrcClient() => IrcClient;
        public PatchClient GetPatchClient() => PatchClient;
        public IFafApiClient GetApiClient() => FafApiClient;
        public IFafContentClient GetContentClient() => FafContentClient;


        public async Task SetServer(Server server)
        {
            Server = server;
            _clientManager.SetServer(server);
            //if (server.Lobby.IsWss) await InitializeWssLobbyClient();
            //else InitializeTcpLobbyClient(Server);
            //IrcClient = new(server.Irc.Host, server.Irc.Port, ircLogger);
            //FafApiClient = RestService.For<IFafApiClient>(
            //    new System.Net.Http.HttpClient(new AuthHeaderHandler(new TokenProvider(this)))
            //    {
            //        BaseAddress = server.Api
            //    });
            //FafContentClient = RestService.For<IFafContentClient>(
            //    new System.Net.Http.HttpClient(new AuthHeaderHandler(new TokenProvider(this)))
            //    {
            //        BaseAddress = server.Content
            //    });
            //PatchClient.Initialize(server.Api.Host, FafApiClient, FafContentClient);
            //IceManager.Initialize(server, LobbyClient, FafApiClient);
        }

        private void LobbyClient_WelcomeDataReceived1(object sender, Welcome e)
        {
            Self = e.me;
        }

        private void LobbyClient_WelcomeDataReceived(object sender, Welcome e)
        {
            IrcClient.SetCredentials(e.login, e.id.ToString());
        }

        private void LobbyClient_IrcPasswordReceived(object sender, string e)
        {
            IrcClient.SetPassword(e);
        }

        public void Dispose()
        {

        }
    }
}
