﻿using beta.Infrastructure.Extensions;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Models.Server.Base;
using beta.Properties;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace beta.Infrastructure.Services
{
    public class SessionService : ISessionService
    {
        #region Events
        public event EventHandler<EventArgs<bool>> Authorized;
        public event EventHandler<EventArgs<PlayerInfoMessage>> NewPlayer;
        public event EventHandler<EventArgs<GameInfoMessage>> NewGame;
        public event EventHandler<EventArgs<SocialMessage>> SocialInfo;
        #endregion

        #region Properties

        private readonly ManagedTcpClient Client = new();
        public ManagedTcpClient TcpClient => Client;

        private readonly IOAuthService OAuthService;

        private readonly Dictionary<ServerCommand, Action<string>> Operations = new();

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
            Client.DataReceived += OnDataReceived;

            #region Response actions for server
            //Operations.Add(ServerCommand.notice, OnNoticeData);
            Operations.Add(ServerCommand.session, OnSessionData);

            Operations.Add(ServerCommand.irc_password, OnIrcPassowrdData);
            Operations.Add(ServerCommand.welcome, OnWelcomeData);
            Operations.Add(ServerCommand.social, OnSocialData);

            Operations.Add(ServerCommand.player_info, OnPlayerData);
            Operations.Add(ServerCommand.game_info, OnGameData);
            //Operations.Add(ServerCommand.matchmaker_info, OnMatchmakerData);

            //Operations.Add(ServerCommand.ping, OnPing);
            //Operations.Add(ServerCommand.pong, OnPong);

            Operations.Add(ServerCommand.invalid, OnInvalidData); 
            #endregion
        }
        #endregion


        public void Connect(IPEndPoint ip)
        {
            Client.Connect(ip.Address.ToString(), ip.Port);
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

            if (Client.TcpClient == null)
                Client.Connect();
            else if (!Client.TcpClient.Connected)
            {
                Client.Connect();
            }

            string session = Settings.Default.session;
            string accessToken = Settings.Default.access_token;
            string generatedUID = GenerateUID(session);

#if DEBUG
            Logger.LogInformation($"{nameof(accessToken)}");
            Logger.LogInformation($"{nameof(session)}");
            Logger.LogInformation($"{nameof(generatedUID)}");
#endif


            StringBuilder builder = new();
            // $"{\"command\": \"social_add\", \"friend|foe\": \"player_id\"}\n"
            var command = builder.Append("{\"command\":\"auth\",\"token\":\"").Append(accessToken)
                .Append("\",\"unique_id\":\"").Append(generatedUID)
                .Append("\",\"session\":\"").Append(session).Append("\"}\n").ToString();

#if DEBUG
            Logger.LogInformation($"Sending data for authorization on server: {@command}");
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
                Client.Connect();
            else if (!Client.TcpClient.Connected)
            {
                Client.Connect();
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
        }
        public void Send(string command) => Client.WriteLine(command);
        private void OnAuthResult(object sender, EventArgs<OAuthStates> e)
        {
            if (e.Arg == OAuthStates.AUTHORIZED)
                Authorize();
        }
        
        private void OnDataReceived(object sender, string json)
        {
            if (Enum.TryParse<ServerCommand>(json.GetRequiredJsonRowValue(), out var command))
            {
                if (Operations.TryGetValue(command, out var response))
                    response.Invoke(json);
#if DEBUG
                else App.DebugWindow.LOG("--------------WARNING! UNKNOWN COMMAND----------------\n" + json.ToJsonFormat());

                if (true)
                    App.DebugWindow.LOG(json.ToJsonFormat());
#endif
            }
        }

        #region Events invokers
        protected virtual void OnAuthorization(EventArgs<bool> e) => Authorized?.Invoke(this, e);
        protected virtual void OnNewPlayer(EventArgs<PlayerInfoMessage> e) => NewPlayer?.Invoke(this, e);
        protected virtual void OnNewGame(EventArgs<GameInfoMessage> e) => NewGame?.Invoke(this, e);
        protected virtual void OnSocialInfo(EventArgs<SocialMessage> e) => SocialInfo?.Invoke(this, e); 
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
        }
        private void OnSessionData(string json)
        {
            Settings.Default.session = json.GetRequiredJsonRowValue(2);
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
#if DEBUG
            //Logger.LogInformation($"Received ping, starting timer...");
            //Client.Write("{\"command\":\"pong\"}");
            //Stopwatch.Start();
#endif
        }

        private void OnPong(string json = null)
        {
#if DEBUG
            //Logger.LogInformation($"Received PONG, time elapsed: {Stopwatch.Elapsed.ToString("c")}");
            //Stopwatch.Stop();
#endif
        } 
        #endregion
    }
}