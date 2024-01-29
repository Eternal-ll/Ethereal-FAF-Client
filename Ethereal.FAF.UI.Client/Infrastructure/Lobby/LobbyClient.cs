using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Converters;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LobbyClient
    {
        public event EventHandler<LobbyState> StateChanged;

        public event EventHandler<string> SessionReceived;
        public event EventHandler<string> IrcPasswordReceived;
        public event EventHandler<Player> PlayerReceived;
        public event EventHandler<Player[]> PlayersReceived;
        public event EventHandler<Game> GameReceived;
        public event EventHandler<Game[]> GamesReceived;

        public event EventHandler<AuthentificationFailedData> AuthentificationFailed;
        public event EventHandler<SocialData> SocialDataReceived;
        public event EventHandler<Welcome> WelcomeDataReceived;
        public event EventHandler<Notification> NotificationReceived;
        public event EventHandler<GameLaunchData> GameLaunchDataReceived;

        public event EventHandler<MatchmakingData> MatchMakingDataReceived;
        public event EventHandler<MatchCancelled> MatchCancelled;
        public event EventHandler<MatchConfirmation> MatchConfirmation;
        public event EventHandler<MatchFound> MatchFound;
        public event EventHandler<SearchInfo> SearchInfoReceived;

        public event EventHandler KickedFromParty;
        public event EventHandler<PartyUpdate> PartyUpdated;
        public event EventHandler<PartyInvite> PartyInvite;

        private ITransportClient _transportClient;
        private readonly List<byte> _cache = new();
        private readonly ILogger Logger;

        public LobbyState State { get; private set; }
        public string Authorization { get; set; }

		JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions();
        public LobbyClient()
        {
            _jsonSerializerOptions.Converters.Add(new LobbyMessageJsonConverter());
        }
        public void Initialize(ITransportClient transportClient)
        {
            _transportClient = transportClient;
            _transportClient.OnData += _transportClient_DataReceived;
            _transportClient.OnState += _transportClient_ConnectionStateChange;
        }

        private void _transportClient_ConnectionStateChange(object sender, ConnectionState e)
        {
            LobbyState state = e switch
            {
                ConnectionState.NotConnected => LobbyState.None,
                ConnectionState.Connecting => LobbyState.Connecting,
                ConnectionState.Connected => LobbyState.Connected,
                ConnectionState.Disconnected => LobbyState.Disconnected,
                ConnectionState.Disconnecting => LobbyState.Disconnecting,
            };
            StateChanged?.Invoke(this, state);
        }

        public bool Disconnect()
        {
            throw new NotImplementedException();
        }
        public bool Connect()
        {
            throw new NotImplementedException();
        }

        //public override void OnWsConnected(HttpResponse response) => UpdateState(LobbyState.Connected);
        //public override void OnWsDisconnecting() => UpdateState(LobbyState.Disconnecting);
        //protected override void OnDisconnected() => UpdateState(LobbyState.Disconnected);
        //public override void OnWsDisconnected() => UpdateState(LobbyState.Disconnected);
        private void UpdateState(LobbyState state)
        {
            Logger.LogInformation("[Lobby] Connection state changed from [{old}] to [{to}]", State, state);
            State = state;
            StateChanged?.Invoke(this, state);
        }
        public bool SendAsync(string json)
        {
            if (!json.EndsWith('\n')) json += '\n';
            var sent = _transportClient.Send(Encoding.UTF8.GetBytes(json));
            if (!sent) Logger.LogError($"[Outbound message:Not send] {json}");
            else Logger.LogTrace($"[Outbound message] {json}");
            return sent;
        }
        /// <summary>
        /// Ask session to process authentification in lobby
        /// </summary>
        /// <param name="agent">Client agent</param>
        /// <param name="agentVersion">Client version</param>
        public void AskSession(string agent, string agentVersion) => SendAsync(ServerCommands.AskSession(agent, agentVersion));
        /// <summary>
        /// Process authentification in lobby
        /// </summary>
        /// <param name="accessToken">OAuth api key <see cref="OAuth.TokenBearer.AccessToken"/></param>
        /// <param name="uid">Generated system UID using session code </param>
        /// <param name="session">Generated session code from lobby server using <see cref="AskSession(string, string)"/></param>
        public void Authenticate(string accessToken, string uid, string session)
        {
            UpdateState(LobbyState.Authorizing);
            SendAsync(ServerCommands.PassAuthentication(accessToken, uid, session));
            Logger.LogTrace($"[Outbound messsage] {ServerCommands.PassAuthentication("*********", "*********", "*********")}");
        }
        #region Matchmaking
        /// <summary>
        /// Request update on queue 
        /// </summary>
        public void RequestUpdateOnQueue() => SendAsync(ServerCommands.RequestMatchMakerInfo);
        /// <summary>
        /// Update queue information
        /// </summary>
        /// <param name="queue">Queue</param>
        /// <param name="state"></param>
        public void UpdateQueue(MatchmakingType queue, QueueSearchState state)
            => SendAsync(ServerCommands.UpdateQueue(
                queue.ToString(),
                state.ToString().ToLower()));
        /// <summary>
        /// Accept invite to party from player
        /// </summary>
        /// <param name="id">Player id</param>
        public void AcceptPartyInviteFromPlayer(long id) => SendAsync(ServerCommands.AcceptPartyInvite(id));
        /// <summary>
        /// Invite player to party
        /// </summary>
        /// <param name="id">Player id</param>
        public void InvitePlayerToParty(long id) => SendAsync(ServerCommands.InviteToParty(id));
        /// <summary>
        /// Kick player from party
        /// </summary>
        /// <param name="id">Player id</param>
        public void KickPlayerFromParty(long id) => SendAsync(ServerCommands.KickPlayerFromParty(id));
        /// <summary>
        /// Set party factions
        /// </summary>
        /// <param name="factions">Factions</param>
        public void SetPartyFactions(params Faction[] factions) => SendAsync(ServerCommands.SetPartyFactions(factions.Select(f => f.ToString()).ToArray()));
        /// <summary>
        /// Set ready to join matchmaking match
        /// </summary>
        public void ReadyToJoinMatch() => SendAsync(ServerCommands.MatchReady);
        #endregion
        /// <summary>
        /// Notify lobby server that we left from game
        /// </summary>
        public void GameEnded() => SendAsync(ServerCommands.UniversalGameCommand("GameState", "[\"Ended\"]"));
        /// <summary>
        /// Request Ice servers for FAF Ice adapter
        /// </summary>
        [Obsolete("Use API instead")]
        public void RequestIceServers() => SendAsync(ServerCommands.RequestIceServers);
        /// <summary>
        /// Restore game session
        /// </summary>
        /// <param name="gameId">Game id</param>
        public void RestoreGameSession(long gameId) => SendAsync(ServerCommands.RestoreGameSession(gameId.ToString()));
        /// <summary>
        /// Join game
        /// </summary>
        /// <param name="gameId">Game id</param>
        public void JoinGame(long gameId) => SendAsync(ServerCommands.JoinGame(gameId.ToString()));
        /// <summary>
        /// Join game
        /// </summary>
        /// <param name="gameId">Game id</param>
        /// <param name="password">Password</param>
        public void JoinGame(long gameId, string password) => SendAsync(ServerCommands.JoinGame(gameId.ToString(), password: password));
        /// <summary>
        /// Host game
        /// </summary>
        /// <param name="title"></param>
        /// <param name="mod"></param>
        /// <param name="mapName"></param>
        /// <param name="minRating"></param>
        /// <param name="maxRating"></param>
        /// <param name="visibility"></param>
        /// <param name="isRatingResctEnforced"></param>
        /// <param name="password"></param>
        /// <param name="isRehost"></param>
        public void HostGame(string title, FeaturedMod mod, string mapName, double? minRating, double? maxRating, GameVisibility visibility = GameVisibility.Public, bool isRatingResctEnforced = false, string password = null, bool isRehost = false)
        {
            StringBuilder sb = new();
            sb.Append("{\"command\":\"game_host\",");
            sb.Append($"\"title\":\"{title}\",");
            sb.Append($"\"mod\":\"{mod.ToString().ToLower()}\",");
            sb.Append($"\"mapname\":\"{mapName}\",");
            sb.Append($"\"visibility\":\"{visibility.ToString().ToLower()}\",");

            if (isRatingResctEnforced && (minRating.HasValue || maxRating.HasValue))
            {
                sb.Append($"\"enforce_rating_range\":{isRatingResctEnforced},");
                if (minRating.HasValue)
                {
                    sb.Append($"\"rating_min\":{minRating.Value},");
                }
                if (maxRating.HasValue)
                {
                    sb.Append($"\"rating_max\":{maxRating.Value},");
                }
            }
            if (!string.IsNullOrWhiteSpace(password))
            {
                sb.Append($"\"password\":\"{password}\",");
            }
            sb[^1] = '}';
            var command = sb.ToString();
            SendAsync(command);
        }

        private void _transportClient_DataReceived(object sender, byte[] e)
        {
            if (_cache.Count == 0 && e[^1] == '\n')
            {
                ProcessData(e);
                return;
            }
            for (int i = 0; i < e.Length; i++)
            {
                if (e[i] == '\n')
                {
                    ProcessData(_cache.ToArray());
                    _cache.Clear();
                    continue;
                }
                _cache.Add(e[i]);
            }
        }
        private void ProcessData(byte[] data)
        {
            try
            {
				var command = JsonSerializer.Deserialize<ServerMessage>(data, _jsonSerializerOptions);
                switch (command)
                {
                    case PingMessage msg: SendAsync(ServerCommands.Pong); break;
                    case SessionMessage msg: SessionReceived?.Invoke(this, msg.Session.ToString()); break;
                    case AuthentificationFailedData msg: AuthentificationFailed?.Invoke(this, msg); break;
                    case Notification msg: NotificationReceived?.Invoke(this, msg); break;
                    case IrcPassword msg: IrcPasswordReceived?.Invoke(this, msg.Password); break;
                    case WelcomeData msg:
                        {
                            var welcome = msg.MapToViewModel();
							WelcomeDataReceived?.Invoke(this, welcome);
							PlayerReceived?.Invoke(this, welcome.me);
							UpdateState(LobbyState.Authorized);
							break;
                        }
                    case SocialData msg: SocialDataReceived?.Invoke(this, msg); break;
                    case PlayerInfoMessage msg: PlayerReceived?.Invoke(this, msg.MapToViewModel()); break;
                    case LobbyPlayers msg: PlayersReceived?
                            .Invoke(this, msg.Players.Select(x => x.MapToViewModel()).ToArray()); break;
                    case GameInfoMessage msg: GameReceived?.Invoke(this, msg.MapToViewModel()); break;
                    case LobbyGames msg: GamesReceived?
                            .Invoke(this, msg.Games.Select(x => x.MapToViewModel()).ToArray()); break;
                    case GameLaunchData msg: GameLaunchDataReceived?.Invoke(this, msg); break;
                    case PartyInvite msg: PartyInvite?.Invoke(this, msg); break;
                    case PartyUpdate msg: PartyUpdated?.Invoke(this, msg); break;
					case PartyKick msg: KickedFromParty?.Invoke(this, null); break;
                    case MatchmakingData msg: MatchMakingDataReceived?.Invoke(this, msg); break;
                    case MatchConfirmation msg: MatchConfirmation?.Invoke(this, msg); break;
                    case MatchFound msg: MatchFound?.Invoke(this, msg); break;
                    case MatchCancelled msg: MatchCancelled?.Invoke(this, msg); break;
                    case SearchInfo msg: SearchInfoReceived?.Invoke(this, msg); break;
                    default:
                        {
                            Logger.LogWarning("Not handled command [{cmd}]", command.GetType());
                            break;
                        }
				}
			}
            catch(Exception ex)
			{
                Logger.LogError(ex.ToString());
			}
		}
    }
}
