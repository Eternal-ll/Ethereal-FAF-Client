using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LobbyClient : NetCoreServer.TcpClient
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
        public event EventHandler<IceServersData> IceServersDataReceived;
        public event EventHandler<IceUniversalData> IceUniversalDataReceived;

        public event EventHandler<MatchmakingData> MatchMakingDataReceived;
        public event EventHandler<MatchCancelled> MatchCancelled;
        public event EventHandler<MatchConfirmation> MatchConfirmation;
        public event EventHandler<MatchFound> MatchFound;
        public event EventHandler<SearchInfo> SearchInfoReceived;

        public event EventHandler KickedFromParty;
        public event EventHandler<PartyUpdate> PartyUpdated;
        public event EventHandler<PartyInvite> PartyInvite;

        private readonly Dictionary<ServerCommand, Action<string>> ServerCommandHandlers;
        private readonly List<byte> Cache = new();
        private readonly ILogger Logger;

        public LobbyState State { get; private set; }

        public LobbyClient(IPAddress address, int port, ILogger logger) : base(address, port)
        {
            Logger = logger;
            logger.LogTrace("[Lobby] Initialized [{address}]:[{port}]", Address, port);
            base.OptionReceiveBufferSize = 4096;

            ServerCommandHandlers = new()
            {
                { ServerCommand.ping, (json) => SendAsync(ServerCommands.Pong) },

                { ServerCommand.session, (json) => SessionReceived?.Invoke(this, json.Split(':')[^1].Split('}')[0]) },
                { ServerCommand.authentication_failed, (json) => AuthentificationFailed?.Invoke(this, JsonSerializer.Deserialize<AuthentificationFailedData>(json)) },
                { ServerCommand.notice, (json) => NotificationReceived?.Invoke(this, JsonSerializer.Deserialize<Notification>(json)) },
                { ServerCommand.irc_password, (json) => IrcPasswordReceived?.Invoke(this, JsonSerializer.Deserialize<Dictionary<string,string>>(json)["password"]) },
                { ServerCommand.welcome, (json) =>
                    {
                        var welcome = JsonSerializer.Deserialize<Welcome>(json);
                        WelcomeDataReceived?.Invoke(this, welcome);
                        PlayerReceived?.Invoke(this, welcome.me);
                        UpdateState(LobbyState.Authorized);
                    }
                },
                { ServerCommand.social, (json) => SocialDataReceived?.Invoke(this, JsonSerializer.Deserialize<SocialData>(json)) },
                { ServerCommand.player_info, (json) =>
                    {
                        var player = JsonSerializer.Deserialize<Player>(json);
                        if (player.Players is null) PlayerReceived?.Invoke(this, player);
                        else PlayersReceived.Invoke(this, player.Players);
                    }
                },
                { ServerCommand.game_info, (json) =>
                    {
                        var game = JsonSerializer.Deserialize<Game>(json);
                        if (game.Games is null) GameReceived?.Invoke(this, game);
                        else GamesReceived.Invoke(this, game.Games);
                    }
                },
                { ServerCommand.game_launch, (json) => GameLaunchDataReceived?.Invoke(this, JsonSerializer.Deserialize<GameLaunchData>(json)) },
                { ServerCommand.party_invite, (json) => PartyInvite?.Invoke(this, JsonSerializer.Deserialize<PartyInvite>(json)) },
                { ServerCommand.update_party, (json) => PartyUpdated?.Invoke(this, JsonSerializer.Deserialize<PartyUpdate>(json)) },
                { ServerCommand.kicked_from_party, (json) => KickedFromParty?.Invoke(this, null) },
                { ServerCommand.matchmaker_info, (json) => MatchMakingDataReceived?.Invoke(this, JsonSerializer.Deserialize<MatchmakingData>(json)) },
                { ServerCommand.match_info, (json) => MatchConfirmation?.Invoke(this, JsonSerializer.Deserialize<MatchConfirmation>(json)) },
                { ServerCommand.match_found, (json) => MatchFound?.Invoke(this, JsonSerializer.Deserialize<MatchFound>(json)) },
                { ServerCommand.match_cancelled, (json) => MatchCancelled?.Invoke(this, JsonSerializer.Deserialize<MatchCancelled>(json)) },
                { ServerCommand.search_info, (json) => SearchInfoReceived?.Invoke(this, JsonSerializer.Deserialize<SearchInfo>(json)) },
                { ServerCommand.IceMsg, (json) => IceUniversalDataReceived?.Invoke(this, JsonSerializer.Deserialize<IceUniversalData>(json)) },
                { ServerCommand.JoinGame, (json) => IceUniversalDataReceived?.Invoke(this, JsonSerializer.Deserialize<IceUniversalData>(json)) },
                { ServerCommand.HostGame, (json) => IceUniversalDataReceived?.Invoke(this, JsonSerializer.Deserialize<IceUniversalData>(json)) },
                { ServerCommand.ConnectToPeer, (json) => IceUniversalDataReceived?.Invoke(this, JsonSerializer.Deserialize<IceUniversalData>(json)) },
                { ServerCommand.DisconnectFromPeer, (json) => IceUniversalDataReceived?.Invoke(this, JsonSerializer.Deserialize<IceUniversalData>(json)) },
            };
        }
        protected override void OnConnecting() => UpdateState(LobbyState.Connecting);
        protected override void OnConnected() => UpdateState(LobbyState.Connected);
        protected override void OnDisconnecting() => UpdateState(LobbyState.Disconnecting);
        protected override void OnDisconnected() => UpdateState(LobbyState.Disconnected);
        private void UpdateState(LobbyState state)
        {
            Logger.LogInformation("[Lobby] [{address}]:[{port}] state changed from [{old}] to [{to}]", Address, Port, State, state);
            State = state;
            StateChanged?.Invoke(this, state);
        }
        protected override void OnError(SocketError error)
        {
            Logger.LogError("[Lobby] [{address}]:[{port}] socket caught an error [{error}]", Address, Port, error);
        }
        public override bool SendAsync(string json)
        {
            var sent = base.SendAsync(json[^1] is not '\n' ? json + '\n' : json);
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
        /// <param name="uid">Generated system UID using session code <see cref="UidGenerator.GenerateUID(string, IProgress{string})"/></param>
        /// <param name="session">Generated session code from lobby server using <see cref="AskSession(string, string)"/></param>
        public void Authenticate(string accessToken, string uid, string session)
        {
            UpdateState(LobbyState.Authorizing);
            base.SendAsync(ServerCommands.PassAuthentication(accessToken, uid, session));
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
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            if (Cache.Count == 0 && buffer[0] == '{' && buffer[^1] == '\n')
            {
                ProcessData(Encoding.UTF8.GetString(buffer, 0, (int)size));
                return;
            }
            for (int i = (int)offset; i < (int)size; i++)
            {
                if (buffer[i] == '\n')
                {
                    ProcessData(Encoding.UTF8.GetString(Cache.ToArray(), 0, Cache.Count));
                    Cache.Clear();
                    continue;
                }
                Cache.Add(buffer[i]);
            }
        }


        private void ProcessData(string json)
        {
            var target = json[12..json.IndexOf('\"', 12)];
            if (!Enum.TryParse<ServerCommand>(target, out var command))
            {
                Logger.LogWarning("[{command}] Unsupported command", target);
                return;
            }
            if (command == ServerCommand.matchmaker_info)
            {

            }
            if (!(command is ServerCommand.game_info or ServerCommand.player_info or ServerCommand.matchmaker_info))
            {
                Logger.LogTrace("[Inbound messsage] {json}", json);
            }
            if (!ServerCommandHandlers.TryGetValue(command, out var handler))
            {
                Logger.LogWarning("[{command}] Handler not registered, ignored.", target);
                return;
            }
            try
            {
                handler.Invoke(json);
            }
            catch(Exception ex)
            {
                Logger.LogError("Caught an error when processed lobby message");
                Logger.LogError(ex.ToString());
            }
        }
    }
}
