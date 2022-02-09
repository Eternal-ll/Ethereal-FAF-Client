using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Models.Server.Base;
using beta.Properties;
using beta.ViewModels.Base;
using beta.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public class LobbySessionService : ViewModel, ILobbySessionService
    {
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

        public ObservableCollection<PlayerInfoMessage> Players { get; } = new();
        private readonly Dictionary<int, int> PlayerUIDToId = new();
        private readonly Dictionary<string, int> PlayerNameToId = new();

        public ObservableCollection<GameInfoMessage> AvailableLobbies { get; } = new();
        public ObservableCollection<GameInfoMessage> LaunchedLobbies { get; } = new();
        #endregion
        private readonly object _lock = new();
        public LobbySessionService(IOAuthService oAuthService)
        {
            OAuthService = oAuthService;
            OAuthService.Result += OnAuthResult;
            Client.DelimiterDataReceived += OnDataReceived;
            PreviewNewLobbies = false;
        }
        public IEnumerable<string> GetPlayersLogins(string filter)
        {
            var enumerator = PlayerNameToId.GetEnumerator();
            filter = filter.ToLower();

            if (string.IsNullOrWhiteSpace(filter))
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current.Key;
                }
            else
                while (enumerator.MoveNext())
                {
                    var player = enumerator.Current.Key;
                    if (player.Contains(filter))
                        yield return player;
                }
        }

        public PlayerInfoMessage GetPlayerInfo(string login)
        {
            login = login.ToLower();
            if (PlayerNameToId.TryGetValue(login, out var id))
            {
                return Players[id];
            }
            foreach (var item in PlayerNameToId.Keys)
            {
                if (item.StartsWith(login, StringComparison.OrdinalIgnoreCase))
                {

                }
            }
            return null;
        }
        public PlayerInfoMessage GetPlayerInfo(int uid)
        {
            if (PlayerUIDToId.TryGetValue(uid, out var id))
            {
                return Players[id];
            }
            return null;
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
                                {
                                    OnNewPlayer(playerInfoMessage.players[i]);
                                }
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
                            new Thread(async () => await GenerateUID(session.session.ToString())).Start();
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
            else if (!Client.TcpClient.Connected)
            {
                Client.Connect("116.202.155.226", 8002);
            }
            string accessToken = Properties.Settings.Default.access_token;
            //while (string.IsNullOrEmpty(GeneratedUID))
            //{
            //    Thread.Sleep(50);
            //}
            //Dictionary<string, string> auth = new()
            //{
            //    { "command", "auth" },
            //    { "token", Properties.Settings.Default.access_token },
            //    { "unique_id", uid },
            //    { "session", session }
            //};
            StringBuilder builder = new();
            var g = builder.Append("{\"command\":\"auth\",\"token\":\"").Append(accessToken)
                .Append("\",\"unique_id\":\"").Append(GeneratedUID)
                .Append("\",\"session\":\"").Append(Session).Append("\"}\n").ToString();
            Client.Write(Encoding.UTF8.GetBytes(g));
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

        protected virtual void OnNewPlayer(EventArgs<PlayerInfoMessage> e)
        {
            var newPlayer = e.Arg;
            if (PlayerUIDToId.TryGetValue(newPlayer.id, out var id))
            {
                var originalPlayer = Players[id];
                foreach (var rating in originalPlayer.ratings)
                {
                    int gamesDifference = 0;
                    if(newPlayer.ratings.TryGetValue(rating.Key, out var rat))
                        gamesDifference = rat.number_of_games - rating.Value.number_of_games;
                    if (gamesDifference == 0)
                        continue;
                    
                    newPlayer.ratings[rating.Key].GamesDifference = gamesDifference + originalPlayer.ratings[rating.Key].GamesDifference;

                    var ratingDifference = new double[2];

                    ratingDifference[0] = newPlayer.ratings[rating.Key].rating[0] - rating.Value.rating[0] + originalPlayer.ratings[rating.Key].rating[0];
                    ratingDifference[1] = newPlayer.ratings[rating.Key].rating[1] - rating.Value.rating[1] + originalPlayer.ratings[rating.Key].rating[1];

                    newPlayer.ratings[rating.Key].RatingDifference = ratingDifference; 
                }
                newPlayer.Updated = DateTime.UtcNow;
                Players[id] = newPlayer;
                OnUpdatePlayer(newPlayer);
            }
            else
            {                    
                int count = Players.Count;
                Players.Add(newPlayer);
                PlayerNameToId.Add(newPlayer.login.ToLower(), count);
                PlayerUIDToId.Add(newPlayer.id, count);
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

            foreach (var key in newGame.teams.Keys)
            {
                for (int i = 0; i < newGame.teams[key].Length; i++)
                {
                    var nick = newGame.teams[key][i];
                    if (PlayerNameToId.TryGetValue(nick, out var id)) 
                    {
                        var player = Players[id];
                        if (player.login == newGame.host)
                        {
                            player.GameState = newGame.password_protected ? GameState.PrivateHost : GameState.Host;
                            continue;
                        }

                        if (newGame.launched_at == null)
                            player.GameState = newGame.password_protected ? GameState.PrivateOpen : GameState.Open;
                        else
                        {
                            var time = DateTime.UnixEpoch.AddSeconds(newGame.launched_at.Value);
                            var difference = DateTime.UtcNow - time;
                            if (difference.TotalSeconds < 300)
                                player.GameState = newGame.password_protected ? GameState.PrivatePlaying5 : GameState.Playing5;
                            else player.GameState = newGame.password_protected ? GameState.PrivatePlaying : GameState.Playing;
                        }
                    }
                }
            }

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
        private void AddLaunchedLobby(GameInfoMessage game)
        {
            AvailableLobbies.Add(game);

        }
        protected virtual void OnUpdateGameInfo(EventArgs<GameInfoMessage> e) => UpdateGameInfo?.Invoke(this, e);
    }
}