using beta.Infrastructure.Extensions;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Models.Server.Enums;
using beta.Properties;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using beta.Models.Debugger;

namespace beta.Infrastructure.Services
{
    public class SessionService : ISessionService
    {
        #region Events
        public event EventHandler<EventArgs<bool>> Authorized;
        public event EventHandler<EventArgs<PlayerInfoMessage>> NewPlayer;
        public event EventHandler<EventArgs<GameInfoMessage>> NewGame;
        public event EventHandler<EventArgs<SocialMessage>> SocialInfo;
        public event EventHandler<EventArgs<WelcomeMessage>> WelcomeInfo;
        #endregion

        #region Properties

        private ManagedTcpClient Client;
        public ManagedTcpClient TcpClient => Client;

        private readonly IOAuthService OAuthService;

        private readonly Dictionary<ServerCommand, Action<string>> Operations = new();
        private bool WaitingPong = false;
        private Stopwatch Stopwatch = new();
        private Stopwatch TimeOutWatcher = new();

#if DEBUG
        private readonly ILogger Logger;
#endif

        #endregion

        #region CTOR
        public SessionService(IOAuthService oAuthService
#if DEBUG
            , ILogger<SessionService> logger
#endif
            )
        {
            OAuthService = oAuthService;
#if DEBUG
            Logger = logger;
#endif
            OAuthService.Result += OnAuthResult;

            #region Response actions for server
            //Operations.Add(ServerCommand.notice, OnNoticeData);
            
            Operations.Add(ServerCommand.irc_password, OnIrcPassowrdData);
            Operations.Add(ServerCommand.welcome, OnWelcomeData);
            Operations.Add(ServerCommand.social, OnSocialData);

            Operations.Add(ServerCommand.player_info, OnPlayerData);
            Operations.Add(ServerCommand.game_info, OnGameData);
            //Operations.Add(ServerCommand.matchmaker_info, OnMatchmakerData);

            Operations.Add(ServerCommand.ping, OnPing);
            Operations.Add(ServerCommand.pong, OnPong);

            Operations.Add(ServerCommand.invalid, OnInvalidData);
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
            if (string.IsNullOrWhiteSpace(session))
                return null;

            string result = null;

            // TODO: invoke some error events?
            if (!File.Exists(Environment.CurrentDirectory + "\\faf-uid.exe"))
                return null;

            Process process = new();
            process.StartInfo.FileName = "faf-uid.exe";
            process.StartInfo.Arguments = session;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {

                // TODO: i didnt get it, why it doesnt work on async. Looks like main dispatcher being busy and stucks
                result += process.StandardOutput.ReadLine();
            }
            process.Close();

            return result;
        }
        public void Authorize()
        {
#if DEBUG
            Logger.LogInformation($"Starting authorization process to lobby server");
            //Logger.LogInformation($"TCP client is connected? {Client.TcpClient.Connected}");
#endif
            // TODO Fix
            if (Client == null)
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
                    state = ManagedTcpClientState.Connected;
                };
                while (state == ManagedTcpClientState.Disconnected)
                {
                    Thread.Sleep(10);
                }
                if (state != ManagedTcpClientState.Connected)
                {
                    // TODO Raise events
                    OnAuthorization(false);
                    return;
                }
            }

            string session = GetSession();
            string accessToken = Settings.Default.access_token;
            string generatedUID = GenerateUID(session);

#if DEBUG
            Logger.LogInformation($"{nameof(accessToken)}");
            Logger.LogInformation($"{nameof(session)}");
            Logger.LogInformation($"{nameof(generatedUID)}");
#endif


            StringBuilder builder = new();

            /*
            {
                "command": "auth",
                "token": "......",
                "unique_id": "faf-uid.exe".
                "session": "...."

            */

            var command = builder
                .Append("{\"command\":\"auth\",\"token\":\"")
                .Append(accessToken)
                .Append("\",\"unique_id\":\"").Append(generatedUID)
                .Append("\",\"session\":\"").Append(session)
                .Append("\"}\n")
                .ToString();

#if DEBUG
            Logger.LogInformation($"Sending data for authorization on server: {@command}");
#endif

            Client.Write(Encoding.UTF8.GetBytes(command));
        }
        public string GetSession()
        {
            /*WRITE
            {
                "command": "ask_session",
                "version": "0.20.1+12-g2d1fa7ef.git",
                "user_agent": "faf-client"
            }*/

            var response = Client.WriteLineAndGetReply(new byte[] {123, 34, 99, 111, 109, 109, 97, 110, 100, 34, 58, 34, 97, 115, 107, 95, 115, 101, 115, 115, 105, 111,
                110, 34, 44, 34, 118, 101, 114, 115, 105, 111, 110, 34, 58, 34, 48, 46, 50, 48, 46, 49, 92, 117, 48, 48,
                50, 66, 49, 50, 45, 103, 50, 100, 49, 102, 97, 55, 101, 102, 46, 103, 105, 116, 34, 44, 34, 117, 115,
                101, 114, 95, 97, 103, 101, 110, 116, 34, 58, 34, 102, 97, 102, 45, 99, 108, 105, 101, 110, 116, 34,
                125, 10}, ServerCommand.session, new(0, 0, 10));

            return response.GetRequiredJsonRowValue(2);
        }
        public void Send(string command) => Client.Write(command);

        public void Ping()
        {
            WaitingPong = true;
            Stopwatch.Start();
            Client.Write("{\"command\":\"ping\"}");
        }

        private void OnAuthResult(object sender, EventArgs<OAuthState> e)
        {
            if (e.Arg == OAuthState.AUTHORIZED)
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

                    //App.DebugWindow.LOGLobby(json.ToJsonFormat());
                }

                else AppDebugger.LOGLobby($"-------------- WARNING! NO RESPONSE FOR COMMAND: {command} ----------------\n" + json.ToJsonFormat());

        }
#if DEBUG
            //else App.DebugWindow.LOGLobby($"-------------- WARNING! UNKNOWN COMMAND: {commandText} ----------------\n" + json.ToJsonFormat());
#endif
        }

        #region Events invokers
        protected virtual void OnAuthorization(EventArgs<bool> e) => Authorized?.Invoke(this, e);
        protected virtual void OnNewPlayer(EventArgs<PlayerInfoMessage> e) => NewPlayer?.Invoke(this, e);
        protected virtual void OnNewGame(EventArgs<GameInfoMessage> e) => NewGame?.Invoke(this, e);
        protected virtual void OnSocialInfo(EventArgs<SocialMessage> e) => SocialInfo?.Invoke(this, e);
        protected virtual void OnWelcomeInfo(EventArgs<WelcomeMessage> e) => WelcomeInfo?.Invoke(this, e);
        #endregion

        #region Server response actions
        private void OnNoticeData(string json)
        {
            // TODO
        }
        private void OnWelcomeData(string json)
        {
            var welcomeMessage = JsonSerializer.Deserialize<WelcomeMessage>(json);
            Settings.Default.PlayerId = welcomeMessage.id;
            Settings.Default.PlayerNick = welcomeMessage.login;
            OnAuthorization(true);
            OnWelcomeInfo(welcomeMessage);
        }

        private void OnIrcPassowrdData(string json)
        {
            string password = json.GetRequiredJsonRowValue(2);
            Settings.Default.irc_password = password;
        }
        private void OnSocialData(string json)
        {
            // Do i really need to invoke Event? 
            OnSocialInfo(JsonSerializer.Deserialize<SocialMessage>(json));
        }
        private void OnInvalidData(string json = null)
        {
            // TODO FIX ME???? ERROR UP?
            Settings.Default.access_token = null;
            OAuthService.Auth();
        }

        private void OnPlayerData(string json)
        {
            var playerInfoMessage = JsonSerializer.Deserialize<PlayerInfoMessage>(json);
            if (playerInfoMessage.players.Length > 0)
                // payload with players
                for (int i = 0; i < playerInfoMessage.players.Length; i++)
                    OnNewPlayer(playerInfoMessage.players[i]);
            else OnNewPlayer(playerInfoMessage);
        }

        private void OnGameData(string json)
        {
            var gameInfoMessage = JsonSerializer.Deserialize<GameInfoMessage>(json);
            if (gameInfoMessage.games?.Length > 0)
                // payload with lobbies
                for (int i = 0; i < gameInfoMessage.games.Length; i++)
                    OnNewGame(gameInfoMessage.games[i]);
            else OnNewGame(gameInfoMessage);
        }
        private void OnMatchmakerData(string json)
        {
            var matchmakerMessage = JsonSerializer.Deserialize<QueueMessage>(json);
            if (matchmakerMessage.queues?.Length > 0)
            {
                // payload with queues
            }
        }

        // VAULTS
        private void OnMapVaultData(string json)
        {

        }

        private void OnPing(string json = null)
        {
            Client.Write("{\"command\":\"pong\"}");
        }

        private void OnPong(string json = null)
        {
            WaitingPong = true;

            //Logger.LogInformation($"Received PONG, time elapsed: {Stopwatch.Elapsed.ToString("c")}");
            //Stopwatch.Stop();
            AppDebugger.LOGLobby($"\nTIME ELAPSED: {Stopwatch.Elapsed:c}");

            Stopwatch.Reset();
        } 
        #endregion
    }
}