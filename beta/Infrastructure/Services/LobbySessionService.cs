using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Models.Server.Base;
using beta.Properties;
using beta.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public class LobbySessionService : ILobbySessionService, INotifyPropertyChanged
    {
        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region Events
        public event EventHandler<EventArgs<bool>> Authorization;
        public event EventHandler<EventArgs<PlayerInfoMessage>> NewPlayer;
        public event EventHandler<EventArgs<PlayerInfoMessage>> UpdatePlayer;
        public event EventHandler<EventArgs<GameInfoMessage>> NewGameInfo;
        public event EventHandler<EventArgs<GameInfoMessage>> UpdateGameInfo;

        public event EventHandler<IServerMessage> MessageReceived;
        #endregion

        #region Properties
        public readonly SimpleTcpClient Client = new();

        private readonly bool PreviewNewLobbies;
        private readonly IOAuthService OAuthService;

        public string Session => Settings.Default.session;
        private string GeneratedUID { get; set; }
        public bool AuthorizationRequested { get; set; }

        public Dictionary<int, PlayerInfoMessage> Players { get; } = new();
        public Dictionary<string, int> PlayerNameToId { get; } = new();

        public ObservableCollection<GameInfoMessage> AvailableLobbies { get; } = new();
        public ObservableCollection<GameInfoMessage> LaunchedLobbies { get; } = new();
        #endregion

        public LobbySessionService(IOAuthService oAuthService)
        {
            OAuthService = oAuthService;
            OAuthService.Result += OnAuthResult;
            Client.DelimiterDataReceived += OnDataReceived;
            PreviewNewLobbies = true;

            if (PreviewNewLobbies)
            {
                new Thread(() =>
                {
                    while (true)
                    {
                        for (int i = 0; i < AvailableLobbies.Count; i++)
                        {
                            var lobby = AvailableLobbies[i];
                            //if (lobby.LobbyCycle == 2)
                            //{
                            //    if (lobby.num_players == 0)
                            //    {
                            //        AvailableLobbies[i].LobbyState = LobbyState.Unknown;
                            //    }
                            //}
                            if (lobby.LobbyCycle == 3)
                            {
                                if (lobby.num_players == 0)
                                {
                                    AvailableLobbies[i].LobbyState = LobbyState.Broken;
                                }
                                else AvailableLobbies[i].LobbyState = LobbyState.Init;
                            }
                            if (lobby.LobbyCycle == 4)
                            {
                                if (lobby.num_players == 0)
                                {
                                    AvailableLobbies.RemoveAt(i);
                                    continue;
                                }
                            }
                            AvailableLobbies[i].LobbyCycle++;
                        }
                        Thread.Sleep(10000);
                    }
                }).Start();
            }
        }
        public void Connect(IPEndPoint ip)
        {
            Client.Connect(ip.Address.ToString(), ip.Port);
        }

        private void OnAuthResult(object sender, EventArgs<OAuthStates> e)
        {
            if (e.Arg == OAuthStates.AUTHORIZED)
            {
                AuthorizationRequested = true;
                Authorize();
            }
        }

        private void OnDataReceived(object sender, string json)
        {
            switch (json[12])
            {
                case 'g': //game_info
                    var gameInfoMessage = JsonSerializer.Deserialize<GameInfoMessage>(json);
                    if (gameInfoMessage.games?.Length > 0)
                        // payload with lobbies
                        for (int i = 0; i < gameInfoMessage.games.Length; i++)
                            OnNewGameInfo(gameInfoMessage.games[i]);
                    else OnNewGameInfo(gameInfoMessage);
                    break;
                case 'i': //irc_password
                    var ircPasswordMessage = JsonSerializer.Deserialize<MainView.IRCPasswordMessage>(json);
                    Properties.Settings.Default.irc_password = ircPasswordMessage.password;
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
                            break;
                        case 'o': // social
                            var socialMessage = JsonSerializer.Deserialize<SocialMessage>(json);
                            break;
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

            Process process = new ();
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
            if (Client.TcpClient == null)
                Client.Connect("116.202.155.226", 8002);
            string accessToken = Properties.Settings.Default.access_token;
            string uid = GeneratedUID;
            while (string.IsNullOrEmpty(GeneratedUID))
            {
                Thread.Sleep(50);
            }
            //Dictionary<string, string> auth = new()
            //{
            //    { "command", "auth" },
            //    { "token", Properties.Settings.Default.access_token },
            //    { "unique_id", uid },
            //    { "session", session }
            //};
            StringBuilder builder = new();
            var g = builder.Append("{\"command\":\"auth\",\"token\":\"").Append(accessToken)
                .Append("\",\"unique_id\":\"").Append(uid)
                .Append("\",\"session\":\"").Append(Session).Append("\"}\n").ToString();
            Client.Write(Encoding.UTF8.GetBytes(g));
        }

        public void AskSession()
        {
            if (Client.TcpClient == null)
                Client.Connect("116.202.155.226", 8002);
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

        protected virtual void OnNewPlayer(EventArgs<PlayerInfoMessage> e)
        {
            var newPlayer = e.Arg;
            if (Players.TryGetValue(newPlayer.id, out var originalPlayer))
            {
                foreach (var rating in originalPlayer.ratings)
                {
                    int gamesDifference = 0;
                    if(newPlayer.ratings.TryGetValue(rating.Key, out var rat))
                        gamesDifference = rat.number_of_games - rating.Value.number_of_games;
                    if (gamesDifference == 0)
                        continue;
                    
                    newPlayer.ratings[rating.Key].GamesDifference = gamesDifference;

                    var ratingDifference = new double[2];

                    ratingDifference[0] = newPlayer.ratings[rating.Key].rating[0] - rating.Value.rating[0];
                    ratingDifference[1] = newPlayer.ratings[rating.Key].rating[1] - rating.Value.rating[1];

                    newPlayer.ratings[rating.Key].RatingDifference = ratingDifference; 
                }
                newPlayer.Updated = DateTime.UtcNow;
                Players[newPlayer.id] = newPlayer;
                PlayerNameToId[newPlayer.login] = newPlayer.id;
                OnUpdatePlayer(newPlayer);
            }
            else
            {
                Players.Add(newPlayer.id, newPlayer);
                PlayerNameToId.Add(newPlayer.login, newPlayer.id);
                NewPlayer?.Invoke(this, e);
            }
        }
            
        protected virtual void OnUpdatePlayer(EventArgs<PlayerInfoMessage> e)
        {
            UpdatePlayer?.Invoke(this, e);
        }

        protected virtual void OnNewGameInfo(EventArgs<GameInfoMessage> e)
        {
            var newGame = e.Arg;

            if (!PreviewNewLobbies && newGame.num_players == 0)
                return;

            bool found = false;
            var lenght = AvailableLobbies.Count;
            for (int i = 0; i < lenght; i++)
            {
                if (AvailableLobbies[i].uid == newGame.uid)
                {
                    found = true;
                    if (newGame.launched_at != null)
                    {
                        AvailableLobbies.RemoveAt(i);
                        LaunchedLobbies.Add(newGame);
                        break;
                    }
                    AvailableLobbies[i] = newGame;
                    OnUpdateGameInfo(newGame);

                    found = true;
                    break;
                }
            }

            if (!found)
            {
                if (newGame.launched_at != null)
                {
                    LaunchedLobbies.Add(newGame);
                    return;
                }
                AvailableLobbies.Add(newGame);

                NewGameInfo?.Invoke(this, newGame);
            }
        }
        protected virtual void OnUpdateGameInfo(EventArgs<GameInfoMessage> e) => UpdateGameInfo?.Invoke(this, e);
    }
}