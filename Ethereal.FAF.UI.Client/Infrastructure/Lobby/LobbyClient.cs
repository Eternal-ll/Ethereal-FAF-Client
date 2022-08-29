using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Base;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TcpClient = NetCoreServer.TcpClient;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    public class LobbyClient : TcpClient
    {
        public event EventHandler<bool> Authorized;
        public event EventHandler<PlayerInfoMessage> PlayerReceived;
        public event EventHandler<PlayerInfoMessage[]> PlayersReceived;
        public event EventHandler<GameInfoMessage> GameReceived;
        public event EventHandler<GameInfoMessage[]> GamesReceived;

        public event EventHandler<AuthentificationFailedData> AuthentificationFailed;
        public event EventHandler<SocialData> SocialDataReceived;
        public event EventHandler<WelcomeData> WelcomeDataReceived;
        public event EventHandler<NotificationData> NotificationReceived;
        //public event EventHandler<QueueData> QueueDataReceived;   
        public event EventHandler<MatchMakerData> MatchMakerDataReceived;
        public event EventHandler<GameLaunchData> GameLaunchDataReceived;
        public event EventHandler<IceServersData> IceServersDataReceived;
        public event EventHandler<IceUniversalData> IceUniversalDataReceived;
        public event EventHandler<MatchCancelledData> MatchCancelledDataReceived;
        public event EventHandler<MatchFoundData> MatchFoundDataReceived;

        private readonly ILogger Logger;
        private readonly string Host;

        private string AccessToken;
        private string Uid;
        private string Session;

        private IProgress<string> SplashProgress;

        public LobbyClient(string host, int port, ILogger logger)
            : base(address: Dns.GetHostEntry(host).AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork),
                  port: port)
        {
            Host = host;
            Logger = logger;
            logger.LogTrace("Initialized with host [{host}] on port [{port}]", host, port);
        }

        public void AskSession() => SendAsync(ServerCommands.AskSession("ethereal-faf-client", "0.0.1"));
        public override bool SendAsync(string text)
        {
            Logger.LogTrace(text);
            var sent = base.SendAsync(text[^1] == '\n' ? text : text + '\n');
            if (!sent)
            {
                Logger.LogError("NOT SEND");
            }
            return sent;
        }

        private bool _stop;
        protected override void OnConnecting()
        {
            Logger.LogTrace("Connecting to [{host}:{port}]", Host, Port);
            SplashProgress?.Report($"Connecting to [{Host}:{Port}]");
            base.OnConnecting();
        }
        protected override void OnConnected()
        {
            Logger.LogInformation("Reconnected in [{}]", sw.Elapsed);
            Logger.LogTrace("Connected to [{host}:{port}]", Host, Port);
            SplashProgress?.Report($"Connected to [{Host}:{Port}]");
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
            string auth = ServerCommands.PassAuthentication(AccessToken, uid, session);
            SendAsync(auth);
        }
        private async void ProcessData(string data)
        {
            var target = data[(data.IndexOf(':') + 2)..];
            target = target[..target.IndexOf('\"')];
            if (Enum.TryParse<ServerCommand>(target, out var command))
            {
                switch (command)
                {
                    case ServerCommand.auth:
                        break;
                    case ServerCommand.authentication_failed:
                        break;
                    case ServerCommand.notice:
                        var notice = JsonSerializer.Deserialize<NotificationData>(data);
                        Logger.LogTrace("[{host}:{port}]: Notification [{session}]", Host, Port, notice.text);
                        NotificationReceived?.Invoke(this, notice);
                        break;
                    case ServerCommand.session:
                        var session = data.Split(':')[^1].Split('}')[0];
                        var uid = await GenerateUID(session);
                        Session = session;
                        Uid = uid;
                        if (AccessToken is null) return;
                        Logger.LogTrace("Processing authentification on [{host}:{port}]", Host, Port);
                        SplashProgress?.Report($"Processing authentification on [{Host}:{Port}]");
                        SendAuth(AccessToken, uid, session);
                        Uid = null;
                        Session = null;
                        break;
                    case ServerCommand.irc_password:
                        break;
                    case ServerCommand.welcome:
                        SendAsync(ServerCommands.RequestIceServers);
                        SplashProgress?.Report("Welcome to FAForever lobby!");
                        break;
                    case ServerCommand.social:
                        SplashProgress?.Report("Preparing app for you");
                        break;
                    case ServerCommand.player_info:
                        var player = JsonSerializer.Deserialize<PlayerInfoMessage>(data);
                        if (player.Players is not null)
                        {
                            PlayersReceived?.Invoke(this, player.Players);
                        }
                        else
                        {
                            PlayerReceived?.Invoke(this, player);
                        }
                        break;
                    case ServerCommand.game_info:
                        var game = JsonSerializer.Deserialize<GameInfoMessage>(data);
                        if (game.Games is not null)
                        {
                            GamesReceived?.Invoke(this, game.Games);
                            Authorized.Invoke(this, true);
                        }
                        else
                        {
                            GameReceived?.Invoke(this, game);
                        }
                        break;
                    case ServerCommand.game:
                        break;
                    case ServerCommand.matchmaker_info:
                        break;
                    case ServerCommand.mapvault_info:
                        break;
                    case ServerCommand.ping:
                        break;
                    case ServerCommand.pong:
                        break;
                    case ServerCommand.game_launch:
                        GameLaunchDataReceived?.Invoke(this, JsonSerializer.Deserialize<GameLaunchData>(data));
                        break;
                    case ServerCommand.party_invite:
                        break;
                    case ServerCommand.update_party:
                        break;
                    case ServerCommand.invite_to_party:
                        break;
                    case ServerCommand.kicked_from_party:
                        break;
                    case ServerCommand.set_party_factions:
                        break;
                    case ServerCommand.match_found:
                        break;
                    case ServerCommand.match_cancelled:
                        break;
                    case ServerCommand.search_info:
                        break;
                    case ServerCommand.restore_game_session:
                        break;
                    case ServerCommand.game_matchmaking:
                        break;
                    case ServerCommand.game_host:
                        break;
                    case ServerCommand.game_join:
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
                        Logger.LogTrace("RECEIVED [{}]", data);
                        IceUniversalDataReceived?.Invoke(this, JsonSerializer.Deserialize<IceUniversalData>(data));
                        break;
                    default:
                        break;
                }
            }
        }

        protected override void OnError(SocketError error)
        {
            Logger.LogError("Client caught an error with code [{error}]", @error);
        }

        public async Task<string> GenerateUID(string session, IProgress<string> progress = null)
        {
            Logger.LogTrace("Generating UID for session [{session}]", session);
            SplashProgress?.Report($"Generating UID for session: {session}");
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = "Third-party applications/faf-uid.exe",
                    Arguments = session,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            Logger.LogTrace("Launching UID generator on [{fafuid}]", process.StartInfo.FileName);
            process.Start();
            Logger.LogTrace("Reading output...");
            string result = await process.StandardOutput.ReadLineAsync();
            Logger.LogTrace("Done reading ouput");
            Logger.LogTrace("Generated UID: [{uid}]", result);
            SplashProgress?.Report($"Generated UID: {result[..10]}...");
            Logger.LogTrace("Closing UID generator...");
            process.Close();
            Logger.LogTrace("UID closed");
            return result;
        }

        public void ConnectAndAuthorizeAsync(string accessToken, IProgress<string> progress = null)
        {
            SplashProgress = progress;
            AccessToken = accessToken;
            if (IsConnected && Uid is not null && Session is not null) SendAuth(accessToken, Uid, Session);
            else ConnectAsync();
        }
    }
}
