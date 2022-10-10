﻿using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Markup;
using TcpClient = NetCoreServer.TcpClient;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    public class LobbyClient : TcpClient
    {
        public event EventHandler<bool> Authorized;
        public event EventHandler<Player> PlayerReceived;
        public event EventHandler<Player[]> PlayersReceived;
        public event EventHandler<Game> GameReceived;
        public event EventHandler<Game[]> GamesReceived;

        public event EventHandler<AuthentificationFailedData> AuthentificationFailed;
        public event EventHandler<SocialData> SocialDataReceived;
        public event EventHandler<Welcome> WelcomeDataReceived;
        public event EventHandler<NotificationData> NotificationReceived;
        //public event EventHandler<QueueData> QueueDataReceived;   
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

        private readonly ILogger Logger;
        private readonly UidGenerator UidGenerator;
        private readonly string Host;
        private readonly string UserAgent;
        private readonly string UserAgentVersion;

        private string AccessToken;
        private string Uid;
        private string Session;

        private long? LastGameUid;

        public Player Self;

        private IProgress<string> SplashProgress;

        public LobbyClient(string host, int port, ILogger logger, UidGenerator uidGenerator, string userAgent, string userAgentVersion)
            : base(address: Dns.GetHostEntry(host).AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork),
                  port: port)
        {
            Host = host;
            Logger = logger;
            logger.LogTrace("Initialized with host [{host}] on port [{port}]", host, port);
            UidGenerator = uidGenerator;
            UserAgent = userAgent;
            UserAgentVersion = userAgentVersion;
        }

        public void AskSession() => SendAsync(ServerCommands.AskSession(UserAgent, UserAgentVersion));
        public override bool SendAsync(string text)
        {
            var sent = base.SendAsync(text[^1] == '\n' ? text : text + '\n');
            if (!sent)
            {
                Logger.LogError($"[Outbound message:Not sent] {text}");
            }
            Logger.LogTrace($"[Outbound message] {text}");
            return sent;
        }

        private bool _stop;
        protected override void OnConnecting()
        {
            Logger.LogTrace($"Connecting to {Host}:{Port}");
            SplashProgress?.Report($"Connecting to {Host}:{Port}");
            base.OnConnecting();
        }
        protected override void OnConnected()
        {
            Logger.LogTrace("Connected to [{host}:{port}]", Host, Port);
            SplashProgress?.Report($"Connected to {Host}:{Port}");
            AskSession();
            //Task.Run(async() =>
            //{
            //    await Task.Delay(10000);
            //    DisconnectAsync();
            //});
        }
        Stopwatch sw = new();
        internal void DisconnectAsync(bool reconnect)
        {
            sw.Restart();
            _stop = reconnect;
            DisconnectAsync();
        }

        protected override void OnDisconnected()
        {
            Logger.LogTrace("Disconnected from [{host}:{port}]", Host, Port);
            SplashProgress?.Report($"Disconnected from [{Host}:{Port}]");
            Authorized?.Invoke(this, false);

            // Wait for a while...
            Thread.Sleep(1000);
            // Try to connect again

            if (!_stop)
                ConnectAsync();
        }
        string cache;
        private bool TryParseLines(ref string data, out string message)
        {
            message = string.Empty;
            if (data is null) return false;
            var index = data.IndexOf('\n');
            if (index != -1)
            {
                message += cache;
                message += data[..(index + 1)];
                data = data[(index + 1)..];
                cache = null;
            }
            else
            {
                cache += data;
            }
            return message.Length != 0;
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            var data = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            while (TryParseLines(ref data, out var message))
            {
                ProcessData(message);
            }
        }
        private void SendAuth(string accessToken, string uid, string session)
        {
            base.SendAsync(ServerCommands.PassAuthentication(accessToken, uid, session) + '\n');
            Logger.LogTrace($"[Outbound messsage] {ServerCommands.PassAuthentication("*********", "*********", "*********")}");
        }
        private async void ProcessData(string data)
        {
            var target = data[(data.IndexOf(':') + 2)..];
            target = target[..target.IndexOf('\"')];
            if (Enum.TryParse<ServerCommand>(target, out var command))
            {
                switch (command)
                {
                    case ServerCommand.game_info:
                    case ServerCommand.player_info:
                        break;
                    default: Logger.LogTrace($"[Inbound message] {data.TrimEnd()}"); break;
                }
                switch (command)
                {
                    case ServerCommand.auth:
                        break;
                    case ServerCommand.authentication_failed:
                        Authorized?.Invoke(this, false);
                        AuthentificationFailed?.Invoke(this, JsonSerializer.Deserialize<AuthentificationFailedData>(data));
                        break;
                    case ServerCommand.notice:
                        if (data.Contains("You are using")) return;
                        var notice = JsonSerializer.Deserialize<NotificationData>(data);
                        NotificationReceived?.Invoke(this, notice);
                        break;
                    case ServerCommand.session:
                        if (Session is not null && Uid is not null) return;
                        var session = data.Split(':')[^1].Split('}')[0];
                        var uid = await UidGenerator.GenerateUID(session, SplashProgress);
                        Session = session;
                        Uid = uid;
                        if (AccessToken is null) return;
                        SplashProgress?.Report($"Processing authentification on [{Host}:{Port}]");
                        SendAuth(AccessToken, uid, session);
                        Uid = null;
                        Session = null;
                        break;
                    case ServerCommand.irc_password:
                        break;
                    case ServerCommand.welcome:
                        var welcome = JsonSerializer.Deserialize<Welcome>(data);
                        Self = welcome.me;
                        WelcomeDataReceived?.Invoke(this, welcome);
                        SplashProgress?.Report("Welcome to FAForever lobby!");

                        if (LastGameUid.HasValue)
                        {
                            SplashProgress?.Report("Restoring session: " + LastGameUid.Value);
                            RestoreGameSession(LastGameUid.Value);
                        }
                        RequestIceServers();
                        AskQueueInfo();
                        break;
                    case ServerCommand.social:
                        SplashProgress?.Report("Preparing app for you");
                        break;
                    case ServerCommand.player_info:
                        var player = JsonSerializer.Deserialize<Player>(data);
                        if (player.Players is not null)
                        {
                            if (player.Players.Length < 50)
                                foreach (var p in player.Players) PlayerReceived?.Invoke(this, p);
                            else PlayersReceived?.Invoke(this, player.Players);
                            return;
                        }
                        if (player.Id == Self.Id) Self = player;
                        PlayerReceived?.Invoke(this, player);
                        break;
                    case ServerCommand.game_info:
                        var game = JsonSerializer.Deserialize<Game>(data);
                        if (game.Games is null)
                        {
                            GameReceived?.Invoke(this, game);
                            return;
                        }
                        GamesReceived?.Invoke(this, game.Games);
                        Authorized?.Invoke(this, true);
                        if (sw.IsRunning) Logger.LogInformation("Reconnected in {elapsed}", sw.Elapsed);
                        break;
                    case ServerCommand.game:
                        break;
                    case ServerCommand.matchmaker_info:
                        MatchMakingDataReceived?.Invoke(this, JsonSerializer.Deserialize<MatchmakingData>(data));
                        break;
                    case ServerCommand.mapvault_info:
                        break;
                    case ServerCommand.ping:
                        SendAsync(ServerCommands.Pong);
                        break;
                    case ServerCommand.pong:
                        break;
                    case ServerCommand.game_launch:
                        GameLaunchDataReceived?.Invoke(this, JsonSerializer.Deserialize<GameLaunchData>(data));
                        break;
                    case ServerCommand.party_invite:
                        PartyInvite?.Invoke(this, JsonSerializer.Deserialize<PartyInvite>(data));
                        break;
                    case ServerCommand.update_party:
                        PartyUpdated?.Invoke(this, JsonSerializer.Deserialize<PartyUpdate>(data));
                        break;
                    case ServerCommand.kicked_from_party:
                        KickedFromParty?.Invoke(this, null);
                        break;
                    case ServerCommand.match_info:
                        MatchConfirmation?.Invoke(this, JsonSerializer.Deserialize<MatchConfirmation>(data));
                        break;
                    case ServerCommand.match_found:
                        MatchFound?.Invoke(this, JsonSerializer.Deserialize<MatchFound>(data));
                        break;
                    case ServerCommand.match_cancelled:
                        MatchCancelled?.Invoke(this, JsonSerializer.Deserialize<MatchCancelled>(data));
                        break;
                    case ServerCommand.search_info:
                        SearchInfoReceived?.Invoke(this, JsonSerializer.Deserialize<SearchInfo>(data));
                        break;
                    case ServerCommand.ice_servers:
                        var ice = JsonSerializer.Deserialize<IceServersData>(data);
                        IceServersDataReceived?.Invoke(this, ice);
                        break;
                    case ServerCommand.invalid:
                        break;
                    case ServerCommand.JoinGame:
                    case ServerCommand.HostGame:
                    case ServerCommand.ConnectToPeer:
                    case ServerCommand.DisconnectFromPeer:
                    case ServerCommand.IceMsg:
                        IceUniversalDataReceived?.Invoke(this, JsonSerializer.Deserialize<IceUniversalData>(data));
                        break;
                    default:
                        break;
                }
            }
        }

        #region Matchmaking
        public void AskQueueInfo() =>
            SendAsync(ServerCommands.RequestMatchMakerInfo);
        public void UpdateQueue(MatchmakingType queue, SearchInfoState state, params Faction[] factions) =>
            SendAsync(ServerCommands.UpdateQueue(
                queue.ToString(),
                state.ToString().ToLower()));
                //factions.Select(f => f.ToString()).ToArray()));
        public void AcceptPartyInviteFromPlayer(long id) =>
            SendAsync(ServerCommands.AcceptPartyInvite(id));
        public void InvitePlayerToParty(long id) =>
            SendAsync(ServerCommands.InviteToParty(id));
        public void KickPlayerFromParty(long id) => 
            SendAsync(ServerCommands.KickPlayerFromParty(id));
        public void SetPartyFactions(params Faction[] factions) => 
            SendAsync(ServerCommands.SetPartyFactions(factions.Select(f => f.ToString()).ToArray()));
        public void ReadyToJoinMatch() =>
            SendAsync(ServerCommands.MatchReady);
        #endregion

        public void RequestIceServers() =>
            SendAsync(ServerCommands.RequestIceServers);
        public void RestoreGameSession(long gameId) =>
            SendAsync(ServerCommands.RestoreGameSession(gameId.ToString()));

        public bool CanJoinGame() => LastGameUid.HasValue;

        public void JoinGame(long uid)
        {
            if (SendAsync(ServerCommands.JoinGame(uid.ToString())))
            {
                LastGameUid = uid;
            }
        }

        protected override void OnError(SocketError error)
        {
            Logger.LogError("Client caught an error with code [{error}]", @error);
        }

        public void ConnectAndAuthorizeAsync(string accessToken, IProgress<string> progress = null)
        {
            SplashProgress = progress;
            AccessToken = accessToken;
            if (IsConnected && Uid is not null && Session is not null) SendAuth(accessToken, Uid, Session);
            else ConnectAsync();
        }


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
            }
            if (minRating.HasValue)
            {
                sb.Append($"\"rating_min\":{minRating.Value},");
            }
            if (maxRating.HasValue)
            {
                sb.Append($"\"rating_max\":{maxRating.Value},");
            }
            if (!string.IsNullOrWhiteSpace(password))
            {
                sb.Append($"\"password\":\"{password}\",");
            }
            sb[^1] = '}';
            var command = sb.ToString();
            Logger.LogTrace("Hosting game with args: [{}]", command);
            SendAsync(command);
        }
    }
}
