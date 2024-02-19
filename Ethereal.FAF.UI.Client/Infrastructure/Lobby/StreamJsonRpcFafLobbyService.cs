using Ethereal.FAF.UI.Client.Infrastructure.Api;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nerdbank.Streams;
using StreamJsonRpc;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    internal class StreamJsonRpcFafLobbyService :
        IFafLobbyCallbacks,
        IFafLobbyEventsService,
        IFafLobbyService,
        IFafLobbyActionClient,
        IFafJavaIceAdapterCallbacks
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

        private readonly IFafAuthService _fafAuthService;
        private readonly IUIDService _uidGenerator;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly ILogger<IFafJavaIceAdapterCallbacks> _fafJavaIceAdapterCallbacksLogger;
        private IFafJavaIceAdapterClient _fafJavaIceAdapterClient;

        private ClientWebSocket _clientWebSocket;
        private JsonRpc _jsonRpc;
        public IFafLobbyClient _fafLobbyClient;

        public StreamJsonRpcFafLobbyService(
            ILogger<StreamJsonRpcFafLobbyService> logger,
            IServiceProvider serviceProvider,
            IFafAuthService fafAuthService,
            IUIDService uidGenerator,
            ILogger<IFafJavaIceAdapterCallbacks> fafJavaIceAdapterCallbacksLogger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _fafAuthService = fafAuthService;
            _uidGenerator = uidGenerator;
            _fafJavaIceAdapterCallbacksLogger = fafJavaIceAdapterCallbacksLogger;
        }

        #region IFafLobbyService
        public bool Connected { get; private set; }
        /// <summary>
        /// Connect to lobby
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            var fafUserApi = _serviceProvider.GetService<IFafUserApi>();
            var lobbyAccess = await fafUserApi.GetLobbyAccess(cancellationToken);

            if (_jsonRpc != null)
            {
                _jsonRpc.Dispose();
                _jsonRpc = null;
                _fafLobbyClient = null;
            }
            if (_clientWebSocket != null)
            {
                _clientWebSocket.Dispose();
                //_jsonRpc.Disconnected -= Rpc_Disconnected;
                //_jsonRpc.Dispose();
                //_fafLobbyClient = null;
            }
            //_clientWebSocket = new ClientWebSocket();

            _clientWebSocket = new();
            await _clientWebSocket.ConnectAsync(lobbyAccess.AccessUrl, CancellationToken.None);

            var rpcMessageFormatter = new FafLobbySystemTextJsonFormatter()
            {
                massagedUserDataSerializerOptions = Services.JsonSerializerDefaults.CyrillicJsonSerializerOptions
            };
            var pipe = _clientWebSocket.UsePipe(1024);
            var rpcMessageHandler = new NewLineDelimitedMessageHandler(pipe, rpcMessageFormatter)
            {
                NewLine = NewLineDelimitedMessageHandler.NewLineStyle.Lf
            };
            var rpc = new JsonRpc(rpcMessageHandler);
            _fafLobbyClient = rpc.Attach<IFafLobbyClient>(new()
            {
                ServerRequiresNamedArguments = true,
            });
            rpc.AddLocalRpcTarget<IFafLobbyCallbacks>(this, new()
            {
                ClientRequiresNamedArguments = true,
            });
            rpc.TraceSource = new("LobbyClientJsonRpc", SourceLevels.Warning);
            rpc.TraceSource.Listeners.Add(new ConsoleTraceListener());
            rpc.StartListening();
            _jsonRpc = rpc;

            _fafLobbyClient.AskSession("ethereal-faf-client", "2.4.3");
            var session = await WaitForSessionAsync(cancellationToken);
            _logger.LogInformation("Received session: {0}", session);
            var uid = await _uidGenerator.GenerateAsync(session.ToString(), cancellationToken);
            var token = await _fafAuthService.GetActualAccessToken(cancellationToken);
            _fafLobbyClient.Auth(token, uid, session);
            Connected = true;
            OnConnection?.Invoke(this, true);

            _fafJavaIceAdapterClient ??= _serviceProvider.GetService<IFafJavaIceAdapterClient>();
            rpc.Disconnected += Rpc_Disconnected;
        }

        private void Rpc_Disconnected(object sender, JsonRpcDisconnectedEventArgs e)
        {
            Connected = false;
            OnConnection?.Invoke(this, false);
        }

        /// <summary>
        /// Disconnect from lobby
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            if (_jsonRpc == null) return;
            _jsonRpc.Dispose();
            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
        }
        /// <summary>
        /// Join game
        /// </summary>
        /// <param name="uid">Lobby UID</param>
        /// <returns></returns>
        public Task JoinGameAsync(long uid, string password = null, int port = 0)
        {
            _fafLobbyClient.GameJoin(uid, password, port);
            return Task.CompletedTask;
        }
        /// <summary>
        /// Restore game session
        /// </summary>
        /// <param name="uid">Game id</param>
        /// <returns></returns>
        public Task RestoreGameSessionAsync(long uid)
        {
            _fafLobbyClient.RestoreGameSession(uid);
            return Task.CompletedTask;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task GameEndedAsync()
        {
            _fafLobbyClient.GameState("game", "Ended");
            return Task.CompletedTask;
        }
        #endregion

        #region IGpgNetSendProxyClient
        public async Task SendTargetActionAsync(string command, string target, params object[] args)
        {
            await _jsonRpc.NotifyAsync(command, new
            {
                target = target,
                args = args
            });
        }

        #endregion

        #region IFafLobbySessionCallback

        private SemaphoreSlim _sessionSemaphoreSlim;
        private Action<long> _onSession;
        //public IDisposable GetSessionSemaphoreSlim(Action<long> onSession, out SemaphoreSlim semaphore)
        //{
        //    _onSession = onSession;
        //    _sessionSemaphoreSlim = new(0);
        //    semaphore = _sessionSemaphoreSlim;
        //    return _sessionSemaphoreSlim;
        //}
        //public void RemoveSessionSemaphoreSlim()
        //{
        //    _sessionSemaphoreSlim = null;
        //    _onSession = null;
        //}
        public async Task<long> WaitForSessionAsync(CancellationToken cancellationToken = default)
        {
            long session = 0;
            using var _ = _sessionSemaphoreSlim = new(0);
            _onSession = x => session = x;
            await _sessionSemaphoreSlim.WaitAsync(cancellationToken);
            return session;
        }
        #endregion

        #region IFafJavaIceAdapterCallbacks
        public Task OnConnectedAsync(long localPlayerId, long remotePlayerId, bool connected)
        {
            if (connected)
            {
                _fafJavaIceAdapterCallbacksLogger.LogInformation(
                    "Connection between '{local}' and '{remote}' has been established",
                    localPlayerId,
                    remotePlayerId);
            }
            else
            {
                _fafJavaIceAdapterCallbacksLogger.LogInformation(
                    "Connection between '{local}' and '{remote}' has been lost",
                    localPlayerId,
                    remotePlayerId);
            }
            return Task.CompletedTask;
        }

        public Task OnConnectionStateChangedAsync(string newState)
        {
            _fafJavaIceAdapterCallbacksLogger.LogInformation(
                "ICE adapter connection state changed to: {state}",
                newState);
            return Task.CompletedTask;
        }

        public async Task OnGpgNetMessageReceivedAsync(string header, object[] chunks)
        {
            _fafJavaIceAdapterCallbacksLogger.LogInformation(
                "Message from game: '{header}' '{chunks}'",
                header,
                JsonSerializer.Serialize(chunks, Services.JsonSerializerDefaults.CyrillicJsonSerializerOptions));
            if (!(_clientWebSocket != null &&
                _clientWebSocket.State == WebSocketState.Open &&
                _fafLobbyClient != null))
            {
                _fafJavaIceAdapterCallbacksLogger.LogWarning("Lobby not connected");
                return;
            }
            await _jsonRpc.NotifyWithParameterObjectAsync(header, new
            {
                target = "game",
                args = chunks
            });
        }

        public Task OnIceConnectionStateChangedAsync(long localPlayerId, long remotePlayerId, string state)
        {
            _logger.LogInformation(
                "ICE connection state for peer '{remote}' changed to: {state}",
                remotePlayerId,
                state);
            return Task.CompletedTask;
        }

        public Task OnIceMsgAsync(long localPlayerId, long remotePlayerId, string message)
        {
            _logger.LogInformation(
                "ICE message for connection '{local}/{remote}': {msg}",
                localPlayerId,
                remotePlayerId,
                message);
            _fafLobbyClient.IceMsg("game", [remotePlayerId, message]);
            return Task.CompletedTask;
        }
        #endregion

        private void LogMethod([CallerMemberName] string method = null,
            params object[] args)
        {
            _logger.LogDebug(
                "Callback method: [{0}], args: [{1}]",
                method,
                JsonSerializer.Serialize(args));
        }

        public async Task OnGameInfoAsync(GameInfoMessage game)
        {
            //LogMethod(args: game);
            GameReceived?.Invoke(this, game);
        }

        public async Task OnGamesInfoAsync(GameInfoMessage[] games)
        {
            //LogMethod(args: games);
            GamesReceived?.Invoke(this, games);
        }

        public async Task OnGameLaunchAsync(GameLaunchData model)
        {
            LogMethod(args: model);
            GameLaunchDataReceived?.Invoke(this, model);
        }


        public async Task OnConnectToPeerAsync(string target, object[] args)
        {
            LogMethod(args: new { target, args });
            var remotePlayerLogin = args[0].ToString();
            var remotePlayerId = long.Parse(args[1].ToString());
            var offer = bool.Parse(args[2].ToString());
            await _fafJavaIceAdapterClient.ConnectToPeerAsync(remotePlayerLogin, remotePlayerId, offer);
            //IceUniversalDataReceived2?.Invoke(this, new()
            //{
            //    target = target,
            //    args = args.ToList()
            //});
        }

        public async Task OnDisconnectFromPeerAsync(string target, object[] args)
        {
            LogMethod(args: new { target, args });
            var remotePlayerId = long.Parse(args[0].ToString());
            await _fafJavaIceAdapterClient.DisconnectFromPeerAsync(remotePlayerId);
            //IceUniversalDataReceived2?.Invoke(this, new()
            //{
            //    target = target,
            //    args = args.ToList()
            //});
        }
        public async Task OnHostGameAsync(string target, object[] args)
        {
            LogMethod(args: new { target, args });
            var mapName = args[0].ToString();
            await _fafJavaIceAdapterClient.HostGameAsync(mapName);
            //IceUniversalDataReceived2?.Invoke(this, new()
            //{
            //    target = target,
            //    args = args.ToList()
            //});
        }

        public async Task OnIceMsgAsync(string target, object[] args)
        {
            LogMethod(args: new { target, args });
            var remotePlayerId = long.Parse(args[0].ToString());
            var msg = args[1].ToString();
            await _fafJavaIceAdapterClient.IceMsgAsync(remotePlayerId, msg);
            //IceUniversalDataReceived2?.Invoke(this, new()
            //{
            //    target = target,
            //    args = args.ToList()
            //});
        }

        public async Task OnJoinGameAsync(string target, object[] args)
        {
            LogMethod(args: new { target, args });
            var remotePlayerLogin = args[0].ToString();
            var remotePlayerId = long.Parse(args[1].ToString());
            await _fafJavaIceAdapterClient.JoinGameAsync(remotePlayerLogin, remotePlayerId);
            //IceUniversalDataReceived2?.Invoke(this, new()
            //{
            //    target = target,
            //    args = args.ToList()
            //});
        }

        public async Task OnInvalidAsync()
        {
            LogMethod();
            _logger.LogError("Received protocol error. Client being disconnected.");
            await OnNoticeAsync("error", "Received protocol error. Client being disconnected");
        }

        public async Task OnKickedFromPartyAsync()
        {
            LogMethod();
            KickedFromParty?.Invoke(this, EventArgs.Empty);
        }

        public async Task OnMatchCancelledAsync(long game_id, string queue_name)
        {
            LogMethod(args: new { game_id, queue_name });
            MatchFound?.Invoke(this, new() { GameId = game_id, Queue = queue_name });
        }

        public async Task OnMatchFoundAsync(long game_id, string queue_name)
        {
            LogMethod(args: new { game_id, queue_name });
        }

        public async Task OnMatchInfoAsync(DateTime expires_at, int players_total, int players_ready, bool ready)
        {

        }

        public async Task OnMatchmakerInfoAsync(QueueData[] queues)
        {
            //LogMethod(args: queues);
            MatchMakingDataReceived?.Invoke(this, new() { Queues = queues});
        }

        public async Task OnNoticeAsync(string style, string text)
        {
            LogMethod(args: new { style, text });
            NotificationReceived?.Invoke(this, new() { Style = style, Text = text });
        }

        public async Task OnPartyInviteAsync(long sender)
        {
            LogMethod(args: sender);
            PartyInvite?.Invoke(this, new() { SenderId = sender });
        }

        public async Task OnPartyUpdateAsync(long owner, PartyMember[] members)
        {
            LogMethod(args: new { owner, members });
            PartyUpdated?.Invoke(this, new() { OwnerId = owner, Members = members });
        }

        public async Task OnPingAsync()
        {
            await _jsonRpc.NotifyAsync("pong");
            //_fafLobbyClient.Pong();
            //LogMethod();
        }

        public async Task OnPlayerInfoAsync(PlayerInfoMessage player)
        {
            PlayerReceived?.Invoke(this, player);
            LogMethod(args: player);
        }

        public async Task OnPlayersInfoAsync(PlayerInfoMessage[] players)
        {
            PlayersReceived?.Invoke(this, players);
            //LogMethod(args: players);
        }

        public async Task OnPongAsync()
        {
            LogMethod();
        }

        public async Task OnSearchInfoAsync(string queue_name, string state)
        {
            LogMethod(args: new { queue_name, state });
            SearchInfoReceived?.Invoke(this, new()
            {
                Queue = queue_name,
                State = Enum.Parse<QueueSearchState>(state, true)
            });
        }

        public async Task OnSessionAsync(long session)
        {
            LogMethod(args: session);

            _onSession?.Invoke(session);
            _sessionSemaphoreSlim?.Release();
        }

        public async Task OnSocialAsync(string[] autojoin, string[] channels, int[] friends, int[] foes, int power)
        {
            LogMethod(args: new { autojoin, channels, friends, foes, power });
        }

        public async Task OnWelcomeAsync(PlayerInfoMessage me, int id, string login)
        {
            LogMethod(args: new { me, id, login });
        }

        public void AcceptPartyInvite(long senderId) => _fafLobbyClient.AcceptPartyInvite(senderId);
        public void InviteToParty(long recipientId) => _fafLobbyClient.InviteToParty(recipientId);
        public void KickFromParty(long playerId) => _fafLobbyClient.KickPlayerFromParty(playerId);
        public void LeaveParty() => _fafLobbyClient.LeaveParty();
        public void SetPartyFactions(params Faction[] factions)
            => _fafLobbyClient.SetPartyFactions(factions.Select(x => x.ToString()).ToArray());

        public void AskQueues() => _fafLobbyClient.AskMatchmakerInfo();
        public void MatchReady() => _fafLobbyClient.MatchReady();
        public void UpdateQueueState(string queue, QueueSearchState state)
            => _fafLobbyClient.UpdateMatchmakingQueueState(queue, state.ToString().ToLower());
    }
}
