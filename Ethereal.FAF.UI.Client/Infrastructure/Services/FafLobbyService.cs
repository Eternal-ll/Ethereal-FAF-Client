using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Outgoing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class FafLobbyService : IFafLobbyService, IFafLobbyEventsService
    {
        #region IFafLobbyEventsService
        public event EventHandler<bool> OnConnection;
        public event EventHandler<string> IrcPasswordReceived;
        public event EventHandler<PlayerInfoMessage> PlayerReceived;
        public event EventHandler<PlayerInfoMessage[]> PlayersReceived;
        public event EventHandler<GameInfoMessage> GameReceived;
        public event EventHandler<GameInfoMessage[]> GamesReceived;
        public event EventHandler<AuthentificationFailedData> AuthentificationFailed;
        public event EventHandler<SocialData> SocialDataReceived;
        public event EventHandler<Welcome> WelcomeDataReceived;
        public event EventHandler<Notification> NotificationReceived;
        public event EventHandler<GameLaunchData> GameLaunchDataReceived;
        public event EventHandler<IceUniversalData2> IceUniversalDataReceived2;
        public event EventHandler<MatchmakingData> MatchMakingDataReceived;
        public event EventHandler<MatchCancelled> MatchCancelled;
        public event EventHandler<MatchConfirmation> MatchConfirmation;
        public event EventHandler<MatchFound> MatchFound;
        public event EventHandler<SearchInfo> SearchInfoReceived;
        public event EventHandler KickedFromParty;
        public event EventHandler<PartyUpdate> PartyUpdated;
        public event EventHandler<PartyInvite> PartyInvite;

        #endregion

        private readonly ILogger<FafLobbyService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFafAuthService _fafAuthService;
        private readonly ClientManager _clientManager;
        private readonly IUIDService _uidGenerator;
        private readonly IBackgroundQueue _queue;

        private static byte _delimeter = Encoding.UTF8.GetBytes("\n")[0];
        
        private MemoryStream _memoryStream;
        private ITransportClient _transportClient;
        private long _session;
        private SemaphoreSlim _sessionSemaphoreSlim = new(0);

        public bool Connected => _transportClient?.IsConnected == true;

        public FafLobbyService(IFafAuthService fafAuthService, ClientManager clientManager, IServiceProvider serviceProvider, ILogger<FafLobbyService> logger, IUIDService uidGenerator, LobbyBackgroundQueue queue)
        {
            _fafAuthService = fafAuthService;
            _clientManager = clientManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _uidGenerator = uidGenerator;
            _queue = queue;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            var server = _clientManager.GetServer() ?? throw new InvalidOperationException("Server not selected");
            if (_transportClient != null)
            {
                _transportClient.OnState -= _transportClient_ConnectionStateChange;
                _transportClient.OnData -= _transportClient_DataReceived;
                _transportClient.Dispose();
            }
            var transportClient = GetTransportClient(server);
            transportClient.OnState += _transportClient_ConnectionStateChange;
            transportClient.OnData += _transportClient_DataReceived;
            _transportClient = transportClient;

            await _transportClient.Connect(cancellationToken);

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var sessionReceived = await _sessionSemaphoreSlim
                .WaitAsync(cancellationTokenSource.Token)
                .ContinueWith(x => !x.IsFaulted, TaskScheduler.Default);
            if (!sessionReceived)
            {
                throw new InvalidOperationException("Failed to fetch lobby session id");
            }
            var session = _session;
            var uid = await _uidGenerator.GenerateAsync(session.ToString(), cancellationToken);
            var token = await _fafAuthService.GetActualAccessToken(cancellationToken);
            SendCommandToLobby(new AuthenticateCommand(token, uid, session));
        }
        private ITransportClient GetTransportClient(Server server)
        {
            if (server.Lobby.IsWss)
            {
                return _serviceProvider.GetRequiredService<WebSocketTransportClient>();
            }
            throw new NotImplementedException("TCP lobby client not implemented");
        }
        private void _transportClient_DataReceived(object sender, byte[] e)
        {
            if (e[^1] == _delimeter)
            {
                if (_memoryStream != null)
                {
                    _memoryStream.Write(e, 0, e.Length);
                    e = _memoryStream.ToArray();
                    _memoryStream.Dispose();
                    _memoryStream = null;
                }
                _queue.Enqueue(cancel => ProcessMessageAsync(e));
            }
            else
            {
                _memoryStream ??= new();
                _memoryStream.Write(e, 0, e.Length);
            }
        }

        private void _transportClient_ConnectionStateChange(object sender, ConnectionState e)
        {
            _logger.LogInformation("Connection state: [{lobbyConnectionState}]", e);
            if (e == ConnectionState.Connected)
            {
                OnConnection?.Invoke(this, true);
                var command = new AskSessionCommand(_clientManager.GetServer().OAuth.ClientId, VersionHelper.GetCurrentVersionInText());
                SendCommandToLobby(command);
            }
            else if (e == ConnectionState.Disconnected) 
            {
                OnConnection?.Invoke(this, false);
            }
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            await _transportClient?.Disconnect(cancellationToken);
        }

        public void SendCommandToLobby(OutgoingCommand command)
        {
            _logger.LogInformation("Outgoing command: [{command}]", JsonSerializer.Serialize<object>(command));
            _transportClient.SendData(JsonSerializer.SerializeToUtf8Bytes<object>(command));
        }

        private Task ProcessMessageAsync(byte[] e)
        {
            try
            {
                var command = JsonSerializer.Deserialize<ServerMessage>(e, JsonSerializerDefaults.FafLobbyCommandsJsonSerializerOptions);
                Handle(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return Task.CompletedTask;
        }
        private void Handle(ServerMessage data)
        {
            if (data is PingMessage) SendCommandToLobby(new PongCommand());
            else if (data is SessionMessage session) OnSession(session.Session);
            //AuthentificationFailedData msg => AuthentificationFailed?.Invoke(this, msg),
            else if (data is Notification notification) NotificationReceived?.Invoke(this, notification);
            else if (data is IrcPassword irc) IrcPasswordReceived?.Invoke(this, irc.Password);
            //WelcomeData msg =>
            //    {
            //        //var welcome = msg.MapToViewModel();
            //        //WelcomeDataReceived?.Invoke(this, welcome);
            //        //PlayerReceived?.Invoke(this, welcome.me);
            //        //UpdateState(LobbyState.Authorized);
            //        break;
            //    }
            else if (data is SocialData SocialData) SocialDataReceived?.Invoke(this, SocialData);
            else if (data is PlayerInfoMessage PlayerInfoMessage) PlayerReceived?.Invoke(this, PlayerInfoMessage);
            else if (data is LobbyPlayers LobbyPlayers) PlayersReceived?.Invoke(this, LobbyPlayers.Players);
            else if (data is GameInfoMessage GameInfoMessage) GameReceived?.Invoke(this, GameInfoMessage);
            else if (data is LobbyGames LobbyGames) GamesReceived?.Invoke(this, LobbyGames.Games);
            else if (data is GameLaunchData GameLaunchData) GameLaunchDataReceived?.Invoke(this, GameLaunchData);
            else if (data is PartyInvite invite) PartyInvite?.Invoke(this, invite);
            else if (data is PartyUpdate PartyUpdate) PartyUpdated?.Invoke(this, PartyUpdate);
            else if (data is PartyKick PartyKick) KickedFromParty?.Invoke(this, EventArgs.Empty);
            else if (data is MatchmakingData MatchmakingData) MatchMakingDataReceived?.Invoke(this, MatchmakingData);
            else if (data is MatchConfirmation matchConfirmation) MatchConfirmation?.Invoke(this, matchConfirmation);
            else if (data is MatchFound matchFound) MatchFound?.Invoke(this, matchFound);
            else if (data is MatchCancelled matchCancelled) MatchCancelled?.Invoke(this, matchCancelled);
            else if (data is SearchInfo SearchInfo) SearchInfoReceived?.Invoke(this, SearchInfo);
            else if (data is IceUniversalData2 IceUniversalData2) IceUniversalDataReceived2?.Invoke(this, IceUniversalData2);
            else _logger.LogWarning("Not handled command [{cmd}]", data.GetType());
        }


        #region Handlers

        private void OnSession(long session)
        {
            _session = session;
            _sessionSemaphoreSlim.Release();
        }

        #endregion

        public Task JoinGameAsync(long uid, string password = null, int port = 0)
        {
            SendCommandToLobby(new JoinGameCommand(uid, password, port));
            return Task.CompletedTask;
        }

        public Task GameEndedAsync()
        {
            SendCommandToLobby(new OutgoingArgsCommand("GameState", "Ended"));
            return Task.CompletedTask;
        }
    }
}
