using beta.Infrastructure.Extensions;
using beta.Infrastructure.Services.Interfaces;
using beta.Infrastructure.Utils;
using beta.Models;
using beta.Models.Debugger;
using beta.Models.Server;
using beta.Models.Server.Base;
using beta.Models.Server.Enums;
using beta.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public class SessionService : ISessionService
    {
        #region Events
        public event EventHandler<AuthentificationFailedData> AuthentificationFailed;
        public event EventHandler<SessionState> StateChanged;
        public event EventHandler<bool> Authorized;
        public event EventHandler<PlayerInfoMessage> PlayerReceived;
        public event EventHandler<PlayerInfoMessage[]> PlayersReceived;
        public event EventHandler<GameInfoMessage> GameReceived;
        public event EventHandler<GameInfoMessage[]> GamesReceived;
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
                    if (value)
                    {
                        if (Settings.Default.ConnectIRC)
                        {
                            IrcService.Authorize(Settings.Default.PlayerNick, Settings.Default.irc_password);
                        }
                    }
                }
            }
        }

        private readonly ILogger Logger;

        #endregion

        #region CTOR
        public SessionService(IOAuthService oAuthService, IIrcService ircService, ILogger<SessionService> logger)
        {
            OAuthService = oAuthService;
            IrcService = ircService;
            Logger = logger;
            OAuthService.StateChanged += OnAuthResult;

            #region Response actions for server
            Operations.Add(ServerCommand.authentication_failed, OnAuthentificationFailedData);
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
        }
        #endregion

        public async Task<string> GenerateUID(string session)
        {
            Logger.LogInformation($"Generating UID for session: {session}");

            if (string.IsNullOrWhiteSpace(session))
            {
                Logger.LogWarning("Passed session value is empty");
                return null;
            }

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

            string result = await process.StandardOutput.ReadLineAsync();
            Logger.LogInformation($"Output line: {result}");

            process.Close();
            Logger.LogInformation($"faf-uid.exe process is closed");
            return result;
        }
        public async Task AuthorizeAsync(string accessToken, CancellationToken token)
        {
            OnSessionStateChanged(SessionState.PendingConnection);
            Logger.LogInformation($"Starting authorization process to lobby server");

            if (Client is not null)
            {
                Client.TcpClient?.Close();
            }

            Client = new()
            {
                Host = "lobby.faforever.com",
                Port = 8002,
                ThreadName = "TCP Lobby Server"
            };
            Client.DataReceived += OnDataReceived;
            
            var reply = (await Client.ConnectAndGetReplyAsync("{\"command\": \"ask_session\", \"version\": \"0.20.1+12-g2d1fa7ef.git\", \"user_agent\": \"ethereal-faf-client\"}\n",
                "session")).Split('\"');

            //:1058334349}
            string session = reply[^1][1..reply[^1].IndexOf('}')];

            string generatedUID = await GenerateUID(session);
            string authJson = ServerCommands.PassAuthentication(accessToken, generatedUID, session);

            Logger.LogInformation($"Sending data for authentication to lobby-server...");
            Client.StateChanged += Client_StateChanged;
            await Client.WriteAsync(authJson);
        }

        private void Client_StateChanged(object sender, ManagedTcpClientState e)
        {
            if (e == ManagedTcpClientState.Disconnected)
            {
                OnSessionStateChanged(SessionState.Disconnected);
                IsAuthorized = false;
            }
        }

        public async Task<string> GetSession()
        {
            /* WRITE
            {
                "command": "ask_session",
                "version": "0.20.1+12-g2d1fa7ef.git",
                "user_agent": "faf-client"
            }*/

            var response = await Client.WriteLineAndGetReply(
                "{\"command\": \"ask_session\", \"version\": \"0.20.1+12-g2d1fa7ef.git\", \"user_agent\": \"faf-client\"}", ServerCommand.session);

            return response.GetRequiredJsonRowValue(2);
        }
        public void Send(string command)
        {
            //Logger.LogInformation($"Sent to lobby-server:\n{command}");
            AppDebugger.LOGLobby($"Sent to lobby-server:\n {command.ToJsonFormat()}");
            Client.Write(command);
        }


        public async Task SendAsync(string command)
        {
            AppDebugger.LOGLobby($"Sent to lobby-server:\n {command.ToJsonFormat()}");
            await Client.WriteAsync(command);
        }

        public void Ping()
        {
            WaitingPong = true;
            Stopwatch.Start();
            Client.Write(ServerCommands.Ping);
        }

        private void OnAuthResult(object sender, OAuthEventArgs e)
        {
            //if (e.State == OAuthState.AUTHORIZED)
            //    Task.Run(() => Authorize());
        }

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
        protected virtual void OnSessionStateChanged(SessionState e) => StateChanged?.Invoke(this, e);
        protected virtual void OnAuthorized(bool e) => Authorized?.Invoke(this, e);
        protected virtual void OnPlayerReceived(PlayerInfoMessage e) => PlayerReceived?.Invoke(this, e);
        protected virtual void OnPlayersReceived(PlayerInfoMessage[] e) => PlayersReceived?.Invoke(this, e);
        protected virtual void OnGameReceived(GameInfoMessage e) => GameReceived?.Invoke(this, e);
        protected virtual void OnGamesReceived(GameInfoMessage[] e) => GamesReceived?.Invoke(this, e);
        protected virtual void OnSocialDataReceived(SocialData e) => SocialDataReceived?.Invoke(this, e);
        protected virtual void OnWelcomeDataReceived(WelcomeData e) => WelcomeDataReceived?.Invoke(this, e);
        protected virtual void OnNotificationReceived(NotificationData e) => NotificationReceived?.Invoke(this, e);
        protected virtual void OnMatchMakerDataReceived(MatchMakerData e) => MatchMakerDataReceived?.Invoke(this, e);
        protected virtual void OnGameLaunchDataReceived(GameLaunchData e) => GameLaunchDataReceived?.Invoke(this, e);
        protected virtual void OnIceServersDataReceived(IceServersData e) => IceServersDataReceived?.Invoke(this, e);
        protected virtual void OnIceUniversalDataReceived(IceUniversalData e) => IceUniversalDataReceived?.Invoke(this, e);
        #endregion

        #region Server response actions
        private void OnAuthentificationFailedData(string json)
        {
            IsAuthorized = false;
            AuthentificationFailed?.Invoke(this, JsonSerializer.Deserialize<AuthentificationFailedData>(json));
            //OnSessionStateChanged(SessionState.AuthentificationFailed);
        }
        private void OnIceUniversalData(string json) => OnIceUniversalDataReceived(JsonSerializer.Deserialize<IceUniversalData>(json));

        private void OnNoticeData(string json)
        {
            var model = JsonSerializer.Deserialize<NotificationData>(json);
            Logger.LogInformation($"Notification: {model.text}");
            OnNotificationReceived(model);
        }
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
                IrcService.Authorize(Settings.Default.PlayerNick, Settings.Default.irc_password);
            }

            //if (!IsAuthorized) IsAuthorized = true;
        }
        private void OnIceServersData(string json) => OnIceServersDataReceived(JsonSerializer.Deserialize<IceServersData>(json));
        private void OnSocialData(string json) => OnSocialDataReceived(JsonSerializer.Deserialize<SocialData>(json));
        private void OnInvalidData(string json = null)
        {
            // TODO FIX ME???? ERROR UP?
            Settings.Default.access_token = null;
            //OAuthService.AuthAsync();
        }

        private void OnPlayerData(string json)
        {
            var playerInfoMessage = JsonSerializer.Deserialize<PlayerInfoMessage>(json);
            if (playerInfoMessage.players is not null)
                OnPlayersReceived(playerInfoMessage.players);
            else OnPlayerReceived(playerInfoMessage);
        }

        private void OnGameData(string json)
        {
            var gameInfoMessage = JsonSerializer.Deserialize<GameInfoMessage>(json);
            if (gameInfoMessage.games is not null)
                OnGamesReceived(gameInfoMessage.games);
            else OnGameReceived(gameInfoMessage);
            //AppDebugger.LOGLobby(json.ToJsonFormat());
        }
        private void OnMatchmakerData(string json) => OnMatchMakerDataReceived(JsonSerializer.Deserialize<MatchMakerData>(json));

        // VAULTS
        private void OnMapVaultData(string json)
        {

        }

        private void OnGameLaunchData(string json)
        {
            OnGameLaunchDataReceived(JsonSerializer.Deserialize<GameLaunchData>(json));
        }

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