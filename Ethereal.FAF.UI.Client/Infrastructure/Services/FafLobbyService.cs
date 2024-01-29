using AsyncAwaitBestPractices;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;
using FAF.Domain.LobbyServer.Outgoing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public event EventHandler<IceUniversalData> IceUniversalDataReceived;
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
        private readonly List<byte> _cache = new();

        private ITransportClient _transportClient;

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
            var server = _clientManager.GetServer();
            if (server == null) throw new InvalidOperationException("Server not selected");
            
            var transportClient = GetTransportClient(server);
            transportClient.OnState += _transportClient_ConnectionStateChange;
            transportClient.OnData += _transportClient_DataReceived;

            if (_transportClient != null)
            {
                _transportClient.OnState -= _transportClient_ConnectionStateChange;
                _transportClient.OnData -= _transportClient_DataReceived;
                _transportClient.Dispose();
            }

            _transportClient = transportClient;

            await _transportClient.Connect(cancellationToken);
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
            if (_cache.Count == 0 && e[^1] == '\n')
            {
                _queue.Enqueue(cancel =>
                {
                    ProcessMessage(e);
                    return Task.CompletedTask;
                });
                return;
            }
            for (int i = 0; i < e.Length; i++)
            {
                if (e[i] == '\n')
                {
                    var data = _cache.ToArray();
                    _cache.Clear();
                    _queue.Enqueue(cancel =>
                    {
                        ProcessMessage(data);
                        return Task.CompletedTask;
                    });
                    continue;
                }
                _cache.Add(e[i]);
            }
        }

        private void _transportClient_ConnectionStateChange(object sender, ConnectionState e)
        {
            _logger.LogInformation("Connection state: [{lobbyConnectionState}]", e);
            if (e == ConnectionState.Connected)
            {
                OnConnection?.Invoke(this, true);
                var command = new AskSessionCommand(_clientManager.GetServer().OAuth.ClientId, "2.2.0");
                var json = JsonSerializer.SerializeToUtf8Bytes(command);
                _transportClient.Send(json);
            }
            else if(e == ConnectionState.Disconnected)
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
            //_logger.LogInformation("Outgoing command: [{command}]", JsonSerializer.Serialize<object>(command));
            _transportClient.Send(JsonSerializer.SerializeToUtf8Bytes<object>(command));
        }
        public void SendCommandToLobby(string raw)
        {
            _logger.LogInformation("Outgoing command: [{command}]", raw);
            _transportClient.Send(Encoding.UTF8.GetBytes(raw));
        }

        private void ProcessMessage(byte[] e)
        {
            try
            {
                var command = JsonSerializer.Deserialize<ServerMessage>(e, JsonSerializerDefaults.FafLobbyCommandsJsonSerializerOptions);

                var ignore = new[] { ServerCommand.game_info, ServerCommand.player_info, ServerCommand.matchmaker_info, ServerCommand.ping };
                if (!ignore.Contains(command.Command))
                {
                    //var raw = Encoding.UTF8.GetString(e);
                    //if (command is GameInfoMessage game)
                    //{
                    //    //_logger.LogInformation("[{id}]-[{state}] raw {raw}", game.Uid, game.State, JsonSerializer.Serialize(game));
                    //}
                    //else if (command is LobbyGames games)
                    //{
                    //    foreach (var ga in games.Games)
                    //    {
                    //        _logger.LogInformation("[{id}]-[{state}] raw {raw}", ga.Uid, ga.State, JsonSerializer.Serialize(ga));
                    //    }
                    //}
                    _logger.LogInformation("Incoming [{data}]", Encoding.UTF8.GetString(e).Replace("\n", null));
                }
                else
                {
                    //_logger.LogTrace("Incoming [{data}]", Encoding.UTF8.GetString(e).Replace("\n", null));
                }

                Action action = command switch
                {
                    PingMessage msg => () => SendCommandToLobby(new PongCommand()),
                    SessionMessage msg => () => OnSession(msg.Session),
                    //AuthentificationFailedData msg => AuthentificationFailed?.Invoke(this, msg),
                    Notification msg => () => NotificationReceived?.Invoke(this, msg),
                    IrcPassword msg => () => IrcPasswordReceived?.Invoke(this, msg.Password),
                    //WelcomeData msg =>
                    //    {
                    //        //var welcome = msg.MapToViewModel();
                    //        //WelcomeDataReceived?.Invoke(this, welcome);
                    //        //PlayerReceived?.Invoke(this, welcome.me);
                    //        //UpdateState(LobbyState.Authorized);
                    //        break;
                    //    }
                    SocialData msg => () => SocialDataReceived?.Invoke(this, msg),
                    PlayerInfoMessage msg => () => PlayerReceived?.Invoke(this, msg),
                    LobbyPlayers msg => () => PlayersReceived?.Invoke(this, msg.Players),
                    GameInfoMessage msg => () => GameReceived?.Invoke(this, msg),
                    LobbyGames msg => () => GamesReceived?.Invoke(this, msg.Games.ToArray()),
                    GameLaunchData msg => () => GameLaunchDataReceived?.Invoke(this, msg),
                    PartyInvite msg => () => PartyInvite?.Invoke(this, msg),
                    PartyUpdate msg => () => PartyUpdated?.Invoke(this, msg),
                    PartyKick msg => () => KickedFromParty?.Invoke(this, null),
                    MatchmakingData msg => () => MatchMakingDataReceived?.Invoke(this, msg),
                    MatchConfirmation msg => () => MatchConfirmation?.Invoke(this, msg),
                    MatchFound msg => () => MatchFound?.Invoke(this, msg),
                    MatchCancelled msg => () => MatchCancelled?.Invoke(this, msg),
                    SearchInfo msg => () => SearchInfoReceived?.Invoke(this, msg),
                    IceUniversalData msg => () => IceUniversalDataReceived?.Invoke(this, msg),
                    IceUniversalData2 msg => () => IceUniversalDataReceived2?.Invoke(this, msg),
                    _ => () =>
                    {
                        _logger.LogWarning("Not handled command [{cmd}]", command.GetType());
                    }
                };
                action.Invoke();
            }
            catch (Exception ex)
            {   
                //_logger.LogInformation("Incoming [{data}]", Encoding.UTF8.GetString(e).Replace("\n", null));
                _logger.LogError(ex.ToString());
            }
        }


        #region Handlers

        private void OnSession(long session) => Task
            .Run(async () =>
            {
                var uid = await _uidGenerator.GenerateAsync(session.ToString());
                var token = await _fafAuthService.GetActualAccessToken();
                SendCommandToLobby(new AuthenticateCommand(token, uid, session));
            })
            .SafeFireAndForget();

        #endregion

        public Task JoinGameAsync(long uid, int port = 0)
        {
            SendCommandToLobby(new JoinGameCommand(uid, port));
            return Task.CompletedTask;
        }

        public Task GameEndedAsync()
        {
            SendCommandToLobby(new OutgoingArgsCommand("GameState", "Ended"));
            return Task.CompletedTask;
        }
    }
}
