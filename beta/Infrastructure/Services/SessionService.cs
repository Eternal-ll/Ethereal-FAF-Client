using beta.Infrastructure.Extensions;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Models.Server.Enums;
using beta.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using beta.Models.Debugger;
using beta.Models.Enums;
using beta.Infrastructure.Utils;
using beta.Models.Server.Base;

namespace beta.Infrastructure.Services
{
    public class SessionService : ISessionService
    {
        #region Events
        public event EventHandler<bool> Authorized;
        public event EventHandler<PlayerInfoMessage> NewPlayerReceived;
        public event EventHandler<GameInfoMessage> NewGameReceived;
        public event EventHandler<SocialData> SocialDataReceived;
        public event EventHandler<WelcomeData> WelcomeDataReceived;
        public event EventHandler<NotificationData> NotificationReceived;
        //public event EventHandler<QueueData> QueueDataReceived;
        public event EventHandler<MatchMakerData> MatchMakerDataReceived;
        public event EventHandler<GameLaunchData> GameLaunchDataReceived;
        public event EventHandler<IceServersData> IceServersDataReceived;
        public event EventHandler<IceUniversalData> IceUniversalDataReceived;
        #endregion

        #region Properties

        private ManagedTcpClient Client;

        private readonly IOAuthService OAuthService;
        private readonly IIrcService IrcService;

        private readonly Dictionary<ServerCommand, Action<string>> Operations = new();
        private bool WaitingPong = false;
        private Stopwatch Stopwatch = new();
        private Stopwatch TimeOutWatcher = new();

        private bool _IsAuthorized;
        public bool IsAuthorized
        {
            get => _IsAuthorized;
            set
            {
                if (!Equals(value, _IsAuthorized))
                {
                    _IsAuthorized = value;
                    OnAuthorized(value);
                }
            }
        }

        private readonly ILogger Logger;

        #endregion

        #region CTOR
        public SessionService(IOAuthService oAuthService, IIrcService ircService, ILogger<SessionService> logger
            )
        {
            OAuthService = oAuthService;
            IrcService = ircService;
            Logger = logger;
            OAuthService.StateChanged += OnAuthResult;

            #region Response actions for server
            Operations.Add(ServerCommand.notice, OnNoticeData);

            Operations.Add(ServerCommand.irc_password, OnIrcPassowrdData);
            Operations.Add(ServerCommand.welcome, OnWelcomeData);
            Operations.Add(ServerCommand.social, OnSocialData);

            Operations.Add(ServerCommand.player_info, OnPlayerData);
            Operations.Add(ServerCommand.game_info, OnGameData);
            Operations.Add(ServerCommand.matchmaker_info, OnMatchmakerData);

            Operations.Add(ServerCommand.ping, OnPing);
            Operations.Add(ServerCommand.pong, OnPong);

            Operations.Add(ServerCommand.ice_servers, OnIceServersData);
            Operations.Add(ServerCommand.game_launch, OnGameLaunchData);

            Operations.Add(ServerCommand.invalid, OnInvalidData);


            // Ice/Game/GpgNet related commands
            Operations.Add(ServerCommand.JoinGame, OnIceUniversalData);
            Operations.Add(ServerCommand.HostGame, OnIceUniversalData);
            Operations.Add(ServerCommand.ConnectToPeer, OnIceUniversalData);
            Operations.Add(ServerCommand.DisconnectFromPeer, OnIceUniversalData);
            Operations.Add(ServerCommand.IceMsg, OnIceUniversalData);
            #endregion

            //new Thread(() =>
            //{
            //    Stopwatch.Start();
            //    while (true)
            //    {
            //        if (TimeOutWatcher.Elapsed.TotalSeconds > 180)
            //            Ping();
            //        Thread.Sleep(6000);
            //    }
            //}).Start();
        }
        #endregion
        public void Connect()
        {
            Client = new(threadName: "TCP Lobby Client", port: 8002);
            Client.DataReceived += OnDataReceived;
        }
        public string GenerateUID(string session)
        {
            Logger.LogInformation($"Generating UID for session: {session}");

            if (string.IsNullOrWhiteSpace(session))
            {
                Logger.LogWarning("Passed session value is empty");
                return null;
            }

            string result = null;

            Logger.LogInformation("Getting path to faf-uid.exe");

            var file = Tools.GetFafUidFileInfo();
            Logger.LogInformation($"Got the path: {file}");
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = file.FullName,
                    Arguments = session,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            process.Start();

            Logger.LogInformation("Starting process of faf-uid.exe");
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                Logger.LogInformation($"Output line: {line}");
                result += line;
            }
            process.Close();
            Logger.LogInformation($"faf-uid.exe process is closed");
            return result;
        }
        public void Authorize()
        {
            Logger.LogInformation($"Starting authorization process to lobby server");

            // TODO Fix
            if (Client is null)
            {
                Client = new(port: 8002)
                {
                    ThreadName = "TCP Lobby Server"
                };
                Client.DataReceived += OnDataReceived;
                // TODO Requires some better logic maybe
                ManagedTcpClientState state = ManagedTcpClientState.Disconnected;
                Client.StateChanged += (s, e) =>
                {
                    state = e;
                    if (e == ManagedTcpClientState.CantConnect || e == ManagedTcpClientState.TimedOut)
                    {
                        // TODO Raise events
                        //OnAuthorization(false);
                        IsAuthorized = false;
                        return;
                    }
                };
                while (state != ManagedTcpClientState.Connected)
                {
                    Thread.Sleep(10);
                }
            }
            else
            {
                if (Client.TcpClient is null)
                {
                    ManagedTcpClientState state = ManagedTcpClientState.Disconnected;
                    Client.StateChanged += (s, e) =>
                    {
                        state = e;
                        if (e == ManagedTcpClientState.CantConnect || e == ManagedTcpClientState.TimedOut)
                        {
                            // TODO Raise events
                            //OnAuthorization(false);
                            IsAuthorized = false;
                            return;
                        }
                    };
                    while (state != ManagedTcpClientState.Connected)
                    {
                        Thread.Sleep(10);
                    }
                    Client.Connect();
                }
            }

            string session = GetSession();
            string accessToken = Settings.Default.access_token;
            string generatedUID = GenerateUID(session);


            string authJson = ServerCommands.PassAuthentication(accessToken, generatedUID, session);

            Logger.LogInformation($"Sending data for authentication to lobby-server...");

            Client.Write(authJson);
        }
        public string GetSession()
        {
            /*WRITE
            {
                "command": "ask_session",
                "version": "0.20.1+12-g2d1fa7ef.git",
                "user_agent": "faf-client"
            }*/

            // just a joke
            var response = Client.WriteLineAndGetReply(new byte[] {123, 34, 99, 111, 109, 109, 97, 110, 100, 34, 58, 34, 97, 115, 107, 95, 115, 101, 115, 115, 105, 111,
                110, 34, 44, 34, 118, 101, 114, 115, 105, 111, 110, 34, 58, 34, 48, 46, 50, 48, 46, 49, 92, 117, 48, 48,
                50, 66, 49, 50, 45, 103, 50, 100, 49, 102, 97, 55, 101, 102, 46, 103, 105, 116, 34, 44, 34, 117, 115,
                101, 114, 95, 97, 103, 101, 110, 116, 34, 58, 34, 102, 97, 102, 45, 99, 108, 105, 101, 110, 116, 34,
                125, 10}, ServerCommand.session, new(0, 0, 10));

            return response.GetRequiredJsonRowValue(2);
        }
        public void Send(string command)
        {
            //Logger.LogInformation($"Sent to lobby-server:\n{command}");
            AppDebugger.LOGLobby($"Sent to lobby-server:\n {command.ToJsonFormat()}");
            Client.Write(command);
        }

        public void Ping()
        {
            WaitingPong = true;
            Stopwatch.Start();
            Client.Write(ServerCommands.Ping);
        }

        private void OnAuthResult(object sender, OAuthEventArgs e)
        {
            if (e.State == OAuthState.AUTHORIZED)
                Authorize();
        }

#if DEBUG
        private readonly List<ServerCommand> AllowedToDebugCommands = new()
        {
            //ServerCommand.notice,
            //ServerCommand.session,
        };
#endif

        private void OnDataReceived(object sender, string json)
        {
            //TimeOutWatcher.Restart();
            var commandText = json.GetRequiredJsonRowValue();
            if (Enum.TryParse<ServerCommand>(commandText, out var command))
            {
                if (Operations.TryGetValue(command, out var response))
                {
                    response.Invoke(json);

                    //AppDebugger.LOGLobby(json.ToJsonFormat());
                }

                else AppDebugger.LOGLobby($"-------------- WARNING! NO RESPONSE FOR COMMAND: {command} ----------------\n" + json.ToJsonFormat());

            }
            else AppDebugger.LOGLobby($"-------------- WARNING! UNKNOWN COMMAND: {commandText} ----------------\n" + json.ToJsonFormat());
        }

        #region Events invokers
        protected virtual void OnAuthorized(bool e) => Authorized?.Invoke(this, e);
        protected virtual void OnNewPlayerReceived(PlayerInfoMessage e) => NewPlayerReceived?.Invoke(this, e);
        protected virtual void OnNewGameReceived(GameInfoMessage e) => NewGameReceived?.Invoke(this, e);
        protected virtual void OnSocialDataReceived(SocialData e) => SocialDataReceived?.Invoke(this, e);
        protected virtual void OnWelcomeDataReceived(WelcomeData e) => WelcomeDataReceived?.Invoke(this, e);
        protected virtual void OnNotificationReceived(NotificationData e) => NotificationReceived?.Invoke(this, e);
        protected virtual void OnMatchMakerDataReceived(MatchMakerData e) => MatchMakerDataReceived?.Invoke(this, e);
        protected virtual void OnGameLaunchDataReceived(GameLaunchData e) => GameLaunchDataReceived?.Invoke(this, e);
        protected virtual void OnIceServersDataReceived(IceServersData e) => IceServersDataReceived?.Invoke(this, e);
        protected virtual void OnIceUniversalDataReceived(IceUniversalData e) => IceUniversalDataReceived?.Invoke(this, e);
        #endregion

        #region Server response actions
        private void OnIceUniversalData(string json)
        {
            var t = JsonSerializer.Deserialize<IceUniversalData>(json);
            OnIceUniversalDataReceived(t);
        }

        private void OnNoticeData(string json) => OnNotificationReceived(JsonSerializer.Deserialize<NotificationData>(json));
        private void OnWelcomeData(string json)
        {
            var welcomeMessage = JsonSerializer.Deserialize<WelcomeData>(json);
            Settings.Default.PlayerId = welcomeMessage.id;
            Settings.Default.PlayerNick = welcomeMessage.login;

            OnWelcomeDataReceived(welcomeMessage);

            //OnAuthorization(true);
            if (!IsAuthorized) IsAuthorized = true;
        }

        private void OnIrcPassowrdData(string json)
        {
            string password = json.GetRequiredJsonRowValue(2);
            Settings.Default.irc_password = password;

            if (Settings.Default.ConnectIRC)
            {
                //IrcService.Authorize(Settings.Default.PlayerNick, Settings.Default.irc_password);
            }

            //if (!IsAuthorized) IsAuthorized = true;
        }
        private void OnIceServersData(string json) => OnIceServersDataReceived(JsonSerializer.Deserialize<IceServersData>(json));
        private void OnSocialData(string json) => OnSocialDataReceived(JsonSerializer.Deserialize<SocialData>(json));
        private void OnInvalidData(string json = null)
        {
            // TODO FIX ME???? ERROR UP?
            Settings.Default.access_token = null;
            OAuthService.AuthAsync();
        }

        private void OnPlayerData(string json)
        {
            var playerInfoMessage = JsonSerializer.Deserialize<PlayerInfoMessage>(json);
            if (playerInfoMessage.players.Length > 0)
                // payload with players
                for (int i = 0; i < playerInfoMessage.players.Length; i++)
                    OnNewPlayerReceived(playerInfoMessage.players[i]);
            else OnNewPlayerReceived(playerInfoMessage);
        }

        private void OnGameData(string json)
        {
            var gameInfoMessage = JsonSerializer.Deserialize<GameInfoMessage>(json);
            if (gameInfoMessage.games?.Length > 0)
                // payload with lobbies
                for (int i = 0; i < gameInfoMessage.games.Length; i++)
                    OnNewGameReceived(gameInfoMessage.games[i]);
            else OnNewGameReceived(gameInfoMessage);

            //AppDebugger.LOGLobby(json.ToJsonFormat());
        }
        private void OnMatchmakerData(string json) => OnMatchMakerDataReceived(JsonSerializer.Deserialize<MatchMakerData>(json));

        // VAULTS
        private void OnMapVaultData(string json)
        {

        }

        private void OnGameLaunchData(string json) => OnGameLaunchDataReceived(JsonSerializer.Deserialize<GameLaunchData>(json));

        private void OnPing(string json = null) => Client.Write(ServerCommands.Pong);

        private void OnPong(string json = null)
        {
            WaitingPong = false;

            //Logger.LogInformation($"Received PONG, time elapsed: {Stopwatch.Elapsed.ToString("c")}");
            //Stopwatch.Stop();
            AppDebugger.LOGLobby($"Received PONG. TIME ELAPSED: {Stopwatch.Elapsed:c}");

            Stopwatch.Reset();
        } 
        #endregion
    }
}