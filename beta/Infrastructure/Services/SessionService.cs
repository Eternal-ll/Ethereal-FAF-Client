using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Properties;
using beta.ViewModels.Base;
using beta.Views;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public class SessionService : ISessionService
    {
        #region Events
        public event EventHandler<EventArgs<bool>> Authorization;

        public event EventHandler<EventArgs<PlayerInfoMessage>> NewPlayer;

        public event EventHandler<EventArgs<GameInfoMessage>> NewGame;

        public event EventHandler<EventArgs<SocialMessage>> SocialInfo;
        #endregion

        #region Properties

        public readonly SimpleTcpClient Client = new();

        private readonly IOAuthService OAuthService;

#if DEBUG
        private readonly ILogger Logger;
#endif
        private string GeneratedUID;

        #endregion

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
            Client.DelimiterDataReceived += OnDataReceived;
        }

        public void Connect(IPEndPoint ip)
        {
            Client.Connect(ip.Address.ToString(), ip.Port);
        }

        private void OnAuthResult(object sender, EventArgs<OAuthStates> e)
        {
            if (e.Arg == OAuthStates.AUTHORIZED)
            {
                Authorize();
            }
        }

        private void OnDataReceived(object sender, string json)
        {
#if DEBUG
            Logger.LogInformation("New JSON data from Server", json);
#endif
            switch (json[12])
            {
                case 'g': //game_info
                    var gameInfoMessage = JsonSerializer.Deserialize<GameInfoMessage>(json);
                    if (gameInfoMessage.games?.Length > 0)
                        // payload with lobbies
                        for (int i = 0; i < gameInfoMessage.games.Length; i++)
                            OnNewGame(gameInfoMessage.games[i]);
                    else OnNewGame(gameInfoMessage);
                    break;
                case 'i':
                    switch (json[13])
                    {
                        case 'r':
                            //irc_password
                            var ircPasswordMessage = JsonSerializer.Deserialize<MainView.IRCPasswordMessage>(json);
                            Properties.Settings.Default.irc_password = ircPasswordMessage.password;
                            return;
                        case 'n': //invalid
                            Settings.Default.access_token = null;
                            OAuthService.Auth();
                            return;
                    }
                    break;
                case 'm':
                    switch (json[14])
                    {
                        case 't':
                            // matchmaker_info
                            var queueMessage = JsonSerializer.Deserialize<QueueMessage>(json);
                            if (queueMessage.queues?.Length > 0)
                            {
                                // payload with queues
                            }
                            break;
                        case 'p':
                            // mapVault_info
                            break;
                    }
                    break;
                case 'n': // notice
                    break;
                case 'p':
                    switch (json[13])
                    {
                        case 'l': // player_info
                            var playerInfoMessage = JsonSerializer.Deserialize<PlayerInfoMessage>(json);
                            if (playerInfoMessage.players.Length > 0)
                                // payload with players
                                for (int i = 0; i < playerInfoMessage.players.Length; i++)
                                    OnNewPlayer(playerInfoMessage.players[i]);
                            else OnNewPlayer(playerInfoMessage);
                            break;
                        case 'i': // ping
                            break;
                        case 'o': // pong
                            break;
                    }
                    break;
                case 's':
                    switch (json[13])
                    {
                        case 'e': // session
                            var session = JsonSerializer.Deserialize<SessionMessage>(json);
                            Settings.Default.session = session.session.ToString();
                            new Thread(() => _ = GenerateUID(session.session.ToString())).Start();
                            break;
                        case 'o': // social
                            // Do i really need to invoke Event? 
                            OnSocialInfo(JsonSerializer.Deserialize<SocialMessage>(json));
                            return;
                    }
                    break;
                case 'w': //welcome
                    var welcomeMessage = JsonSerializer.Deserialize<WelcomeMessage>(json);
                    Settings.Default.PlayerId = welcomeMessage.id;
                    Settings.Default.PlayerNick = welcomeMessage.login;
                    OnAuthorization(true);
                    break;
            }
        }

        public async Task<string> GenerateUID(string session)
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
                result += await process.StandardOutput.ReadLineAsync();
            }
            process.Close();

            GeneratedUID = result;
            return result;
        }

        public void Authorize()
        {
#if DEBUG
            Logger.LogInformation($"SessionService starting authorization process to lobby.faforever.com on {nameof(Authorize)} function.");
            Logger.LogInformation($"TCP client is connected? {Client.TcpClient.Connected}");
#endif
            if (Client.TcpClient == null)
                Client.Connect("116.202.155.226", 8002);
            else if (!Client.TcpClient.Connected)
            {
                Client.Connect("116.202.155.226", 8002);
            }

            string accessToken = Settings.Default.access_token;
            string session = Settings.Default.session;
            string generatedUID = GeneratedUID;

#if DEBUG
            Logger.LogInformation($"{nameof(accessToken)}", accessToken);
            Logger.LogInformation($"{nameof(session)}", session);
            Logger.LogInformation($"{nameof(generatedUID)}", generatedUID);
#endif


            StringBuilder builder = new();

            var command = builder.Append("{\"command\":\"auth\",\"token\":\"").Append(accessToken)
                .Append("\",\"unique_id\":\"").Append(generatedUID)
                .Append("\",\"session\":\"").Append(session).Append("\"}\n").ToString();

#if DEBUG
            Logger.LogInformation($"Sending message to Server", JsonSerializer.Serialize(command, typeof(string)));
#endif

            Client.Write(Encoding.UTF8.GetBytes(command));
            //Dictionary<string, string> auth = new()
            //{
            //    { "command", "auth" },
            //    { "token", Properties.Settings.Default.access_token },
            //    { "unique_id", uid },
            //    { "session", session }
            //};
        }

        public void AskSession()
        {
            if (Client.TcpClient == null)
                Client.Connect("116.202.155.226", 8002);
            else if (!Client.TcpClient.Connected)
            {
                Client.Connect("116.202.155.226", 8002);
            }
            //var result =
            //    Client.WriteLineAndGetReply("command=ask_session&version=0.20.1+12-g2d1fa7ef.git&user_agent=faf-client",
            //        TimeSpan.Zero);
            Client.Write(new byte[]
            {
                /* WRITE
                {
                    "command": "ask_session",
                    "version": "0.20.1+12-g2d1fa7ef.git",
                    "user_agent": "faf-client"
                }*/

                123, 34, 99, 111, 109, 109, 97, 110, 100, 34, 58, 34, 97, 115, 107, 95, 115, 101, 115, 115, 105, 111,
                110, 34, 44, 34, 118, 101, 114, 115, 105, 111, 110, 34, 58, 34, 48, 46, 50, 48, 46, 49, 92, 117, 48, 48,
                50, 66, 49, 50, 45, 103, 50, 100, 49, 102, 97, 55, 101, 102, 46, 103, 105, 116, 34, 44, 34, 117, 115,
                101, 114, 95, 97, 103, 101, 110, 116, 34, 58, 34, 102, 97, 102, 45, 99, 108, 105, 101, 110, 116, 34,
                125, 10
            });
            //string session = string.Empty;
            //return session;
        }

        protected virtual void OnAuthorization(EventArgs<bool> e) => Authorization?.Invoke(this, e);
        protected virtual void OnNewPlayer(EventArgs<PlayerInfoMessage> e) => NewPlayer?.Invoke(this, e);
        protected virtual void OnNewGame(EventArgs<GameInfoMessage> e) => NewGame?.Invoke(this, e);
        protected virtual void OnSocialInfo(EventArgs<SocialMessage> e) => SocialInfo?.Invoke(this, e);
    }
}