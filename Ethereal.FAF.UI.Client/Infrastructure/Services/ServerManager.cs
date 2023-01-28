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
        private readonly IServiceProvider ServiceProvider;
        private readonly ILogger<ServerManager> Logger;
        private readonly ServerOauthTokenProvider OAuthTokenProvider;
        private readonly PatchClient PatchClient;
        private readonly GameLauncher GameLauncher;
        private readonly IceManager IceManager;
        private readonly UidGenerator UidGenerator;

        private readonly MatchmakingViewModel MatchmakingViewModel;
        private readonly PartyViewModel PartyViewModel;

        private LobbyClient LobbyClient;
        private IrcClient IrcClient;
        private IFafApiClient FafApiClient;
        private IFafContentClient FafContentClient;
        public Server Server { get; private set; }
        public Player Self { get; private set; }

        public ServerManager(ServerOauthTokenProvider oauthTokenProvider, IServiceProvider serviceProvider, PatchClient patchClient, IceManager iceManager, GameLauncher gameLauncher, UidGenerator uidGenerator, ILogger<ServerManager> logger, IConfiguration configuration, MatchmakingViewModel matchmakingViewModel, PartyViewModel partyViewModel)
        {
            OAuthTokenProvider = oauthTokenProvider;
            ServiceProvider = serviceProvider;
            PatchClient = patchClient;
            IceManager = iceManager;
            GameLauncher = gameLauncher;
            UidGenerator = uidGenerator;
            Logger = logger;
            Configuration = configuration;
            this.MatchmakingViewModel = matchmakingViewModel;
            PartyViewModel = partyViewModel;
        }

        public string GetApiDomain() => Server?.Api?.Host;
        public ServerOauthTokenProvider GetOAuthProvider() => OAuthTokenProvider;
        public Server GetServer() => Server;
        public LobbyClient GetLobbyClient() => LobbyClient;
        public IrcClient GetIrcClient() => IrcClient;
        public PatchClient GetPatchClient() => PatchClient;
        public IFafApiClient GetApiClient() => FafApiClient;
        public IFafContentClient GetContentClient() => FafContentClient;
        public MatchmakingViewModel GetMatchmakingViewModel() => MatchmakingViewModel;


        public void SetServer(Server server)
        {
            Server = server;
            OAuthTokenProvider.SetServer(server);
            var lobbyLogger = ServiceProvider.GetRequiredService<ILogger<LobbyClient>>();
            var ircLogger = ServiceProvider.GetRequiredService<ILogger<IrcClient>>();
            var uidGenerator = ServiceProvider.GetRequiredService<UidGenerator>();
            LobbyClient = new(DetermineIPAddress(server.Lobby.Host), server.Lobby.Port, lobbyLogger);
            IrcClient = new(server.Irc.Host, server.Irc.Port, ircLogger);
            FafApiClient = RestService.For<IFafApiClient>(
                new System.Net.Http.HttpClient(new AuthHeaderHandler(ServiceProvider.GetRequiredService<ITokenProvider>()))
                {
                    BaseAddress = server.Api
                });
            FafContentClient = RestService.For<IFafContentClient>(
                new System.Net.Http.HttpClient(new AuthHeaderHandler(ServiceProvider.GetRequiredService<ITokenProvider>()))
                {
                    BaseAddress = server.Content
                });
            PatchClient.Initialize(server.Api.Host, FafApiClient, FafContentClient);
            IceManager.Initialize(server, LobbyClient, FafApiClient);

            PartyViewModel.Initialize(this, LobbyClient);
            MatchmakingViewModel.Initialize(LobbyClient, PatchClient, PartyViewModel);

            LobbyClient.WelcomeDataReceived += LobbyClient_WelcomeDataReceived1;
            LobbyClient.StateChanged += LobbyClient_StateChanged;
            LobbyClient.SessionReceived += LobbyClient_Session;
            LobbyClient.GameLaunchDataReceived += LobbyClient_GameLaunchDataReceived;
            LobbyClient.MatchCancelled += LobbyClient_MatchCancelled;
            LobbyClient.IrcPasswordReceived += LobbyClient_IrcPasswordReceived;
            LobbyClient.WelcomeDataReceived += LobbyClient_WelcomeDataReceived;
        }

        private void LobbyClient_WelcomeDataReceived1(object sender, Welcome e)
        {
            Self = e.me;
        }

        private void LobbyClient_StateChanged(object sender, LobbyState e)
        {
            switch (e)
            {
                case LobbyState.None:
                    break;
                case LobbyState.Connecting:
                    break;
                case LobbyState.Connected:
                    LobbyClient.AskSession(Server.OAuth.ClientId, Configuration.GetClientVersion());
                    break;
                case LobbyState.Authorizing:
                    break;
                case LobbyState.Authorized:
                    break;
                case LobbyState.Disconnecting:
                    break;
                case LobbyState.Disconnected:
                    break;
            };
            Server.ServerState = e switch
            {
                LobbyState.Connecting => ServerState.Connecting,
                //LobbyState.Connected => throw new NotImplementedException(),
                //LobbyState.Authorizing => throw new NotImplementedException(),
                LobbyState.Authorized => ServerState.Connected,
                //LobbyState.Disconnecting => throw new NotImplementedException(),
                LobbyState.Disconnected => ServerState.Idle,
                _ => Server.ServerState
            };
        }

        private IPAddress DetermineIPAddress(string host)
        {
            Logger.LogInformation("Determining IPv4 using [{host}]", host);
            if (IPAddress.TryParse(host, out var address))
            {
                Logger.LogInformation("IP address [{host}] recognized", host);
                return address;
            }
            Logger.LogInformation("Searching for IP address using Dns host [{host}]", host);
            var addresses = Dns.GetHostEntry(host).AddressList;
            Logger.LogInformation("For [{host}] founded next addresses: [{address}]", host, string.Join(',', addresses.Select(a => a.ToString())));
            address = Dns.GetHostEntry(host).AddressList.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            if (address is null)
            {
                Logger.LogError("IPv4 address not found");
                throw new ArgumentOutOfRangeException("host", host, "Can`t determine IP4v address for this host");
            }
            Logger.LogInformation("Found IPv4 address [{ipv4}]", address.ToString());
            return address;
        }

        /// <summary>
        /// <see cref="ServerCommand.session"/> handler, generating UID using session and passing authorization data
        /// </summary>
        /// <param name="session"></param>
        private async void LobbyClient_Session(object sender, string session)
        {
            var uid = await UidGenerator.GenerateAsync(session);
            var token = await GetOauthTokenAsync();
            LobbyClient.Authenticate(token.AccessToken, uid, session);
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
            GameLauncher.LobbyClient_GameLaunchDataReceived(e, Self, IceManager, Server, LobbyClient);
        }

        public Task<TokenBearer> GetOauthTokenAsync(CancellationToken cancellationToken = default) =>
            OAuthTokenProvider.GetTokenAsync(cancellationToken);

        public async Task AuthorizeAndConnectAsync(Server server, CancellationToken cancellationToken = default)
        {
            server.ServerState = ServerState.Authorizing;
            var token = await GetOauthTokenAsync(cancellationToken);
            if (token is null) return;
            server.ServerState = ServerState.Authorized;
            LobbyClient.ConnectAsync();
        }

        public void Dispose()
        {

        }
    }
}
