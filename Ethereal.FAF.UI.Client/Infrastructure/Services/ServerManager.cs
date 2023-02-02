using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.IRC;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Refit;
using System;
using System.Linq;
using System.Net;
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

        private readonly IConfiguration Configuration;
        private readonly ILogger<ServerManager> Logger;
        private readonly PatchClient PatchClient;
        private readonly GameLauncher GameLauncher;
        private readonly IceManager IceManager;
        private readonly UidGenerator UidGenerator;
        private readonly TokenProvider TokenProvider;

        private readonly LobbyClient LobbyClient;
        private readonly IrcClient IrcClient;
        private IFafApiClient FafApiClient;
        private IFafContentClient FafContentClient;
        public Server Server { get; private set; }
        public Player Self { get; private set; }

        public ServerManager(PatchClient patchClient, IceManager iceManager, GameLauncher gameLauncher, UidGenerator uidGenerator, ILogger<ServerManager> logger, IConfiguration configuration, IrcClient ircClient, LobbyClient lobbyClient, TokenProvider tokenProvider)
        {
            PatchClient = patchClient;
            IceManager = iceManager;
            GameLauncher = gameLauncher;
            UidGenerator = uidGenerator;
            Logger = logger;
            Configuration = configuration;
            IrcClient = ircClient;
            LobbyClient = lobbyClient;


            LobbyClient.WelcomeDataReceived += LobbyClient_WelcomeDataReceived1;
            LobbyClient.GameLaunchDataReceived += LobbyClient_GameLaunchDataReceived;
            LobbyClient.MatchCancelled += LobbyClient_MatchCancelled;
            LobbyClient.IrcPasswordReceived += LobbyClient_IrcPasswordReceived;
            LobbyClient.WelcomeDataReceived += LobbyClient_WelcomeDataReceived;
            TokenProvider = tokenProvider;
        }

        public string GetApiDomain() => Server?.Api?.Host;
        public Server GetServer() => Server;
        public LobbyClient GetLobbyClient() => LobbyClient;
        public IrcClient GetIrcClient() => IrcClient;
        public IceManager GetIceManager() => IceManager;
        public PatchClient GetPatchClient() => PatchClient;
        public IFafApiClient GetApiClient() => FafApiClient;
        public IFafContentClient GetContentClient() => FafContentClient;


        public void SetServer(Server server)
        {
            Server = server;
            //OAuthTokenProvider.SetServer(server);
            //LobbyClient = new(DetermineIPAddress(server.Lobby.Host), server.Lobby.Port, lobbyLogger);
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

        private void LobbyClient_MatchCancelled(object sender, global::FAF.Domain.LobbyServer.MatchCancelled e)
        {
            GameLauncher.LobbyClient_MatchCancelled(sender, e);
        }

        private void LobbyClient_GameLaunchDataReceived(object sender, global::FAF.Domain.LobbyServer.GameLaunchData e)
        {
            GameLauncher.LobbyClient_GameLaunchDataReceived(e, this);
        }

        public async Task AuthorizeAndConnectAsync(Server server, CancellationToken cancellationToken = default)
        {
            server.ServerState = ServerState.Authorizing;
            var token = await TokenProvider.GetTokenBearerAsync(cancellationToken);
            if (token is null) return;
            server.ServerState = ServerState.Authorized;
            LobbyClient.ConnectAsync();
        }

        public void Dispose()
        {

        }
    }
}
