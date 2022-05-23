using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Models.Server.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace beta.Infrastructure.Services
{
    public class GamesService : IGamesService
    {
        public event EventHandler<GameInfoMessage> NewGameReceived;
        public event EventHandler<GameInfoMessage> GameUpdated;
        //public event EventHandler<GameInfoMessage> GameRemoved;

        public event EventHandler<GameInfoMessage> GameLaunched;
        public event EventHandler<GameInfoMessage> GameEnd;
        public event EventHandler<GameInfoMessage> GameClosed;

        public event EventHandler<string[]> PlayersLeftFromGame;
        public event EventHandler<KeyValuePair<GameInfoMessage, string[]>> PlayersJoinedToGame;

        public event EventHandler<KeyValuePair<GameInfoMessage, PlayerInfoMessage[]>> PlayersJoinedGame;
        public event EventHandler<KeyValuePair<GameInfoMessage, PlayerInfoMessage[]>> PlayersLeftGame;
        public event EventHandler<KeyValuePair<GameInfoMessage, PlayerInfoMessage[]>> PlayersFinishedGame;

        private readonly ISessionService SessionService;
        private readonly IPlayersService PlayersService;
        private readonly ILogger Logger;
        
        public List<GameInfoMessage> Games { get; }

        public GamesService(ISessionService sessionService, ILogger<GamesService> logger, IPlayersService playersService)
        {
            SessionService = sessionService;
            PlayersService = playersService;
            Logger = logger;

            Games = new();

            sessionService.GameReceived += OnGameReceived;
            sessionService.GamesReceived += OnGamesReceived;

            sessionService.StateChanged += SessionService_StateChanged;

            DispatcherTimer = new(DispatcherPriority.Background, App.Current.Dispatcher)
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            DispatcherTimer.Tick += DispatcherTimer_Tick;
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            var games = Games;
            var now = DateTime.UtcNow;
            for (int i = 0; i < games.Count; i++)
            {
                var game = games[i];
                if (game.launched_at is null) continue;
                game.Duration = now - DateTime.UnixEpoch.AddSeconds(game.launched_at.Value);
            }
        }

        private DispatcherTimer DispatcherTimer;

        private void SessionService_StateChanged(object sender, SessionState e)
        {
            if (e == SessionState.Disconnected)
            {
                Logger.LogInformation("Clearing all games");
                Games.Clear();
            }
        }

        private void OnGamesReceived(object sender, GameInfoMessage[] e)
        {
            //Logger.LogInformation($"Received {e.Length} games from lobby-server");
            foreach (var game in e) HandleGameData(game);
            DispatcherTimer.Start();
        }

        private bool TryGetGame(long uid, out GameInfoMessage game)
        {
            var games = Games;
            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].uid == uid)
                {
                    game = games[i];
                    return true;
                }
            }
            game = null;
            return false;
        }

        private void HandleOnGameClose(GameInfoMessage game)
        {
            List<string> playersToClear = new();
            foreach (var team in game.teams) playersToClear.AddRange(team.Value);

            if (TryGetGame(game.uid, out var foundGame))
            {
                // optimize from dublicates
                foreach (var team in foundGame.teams)
                    playersToClear.AddRange(team.Value);
                //Games.Remove(foundGame);
                //foundGame.Dispose();
                //foundGame = null;
            }
            PlayersLeftFromGame?.Invoke(this, playersToClear.ToArray());
            OnGameEnd(game);
        }

        private void HandleTeams(GameInfoMessage game)
        {
            InGameTeam[] teams = new InGameTeam[game.teams.Count];

            int j = 0;

            foreach (var valuePair in game.teams)
            {
                var players = new IPlayer[valuePair.Value.Length];

                for (int i = 0; i < valuePair.Value.Length; i++)
                {
                    var player = PlayersService.GetPlayer(valuePair.Value[i]);
                    if (player is null)
                    {
                        Logger.LogWarning($"Player not found {valuePair.Value[i]}");
                        players[i] = new UnknownPlayer()
                        {
                            login = valuePair.Value[i],
                            RelationShip = PlayerRelationShip.None
                        };
                    }
                    else
                    {
                        player.Game = game;
                        players[i] = player;
                    }
                }

                teams[j] = new(valuePair.Key, players);
                j++;
            }

            game.Teams = teams;
        }

        private void HandleGameData(GameInfoMessage newGame)
        {
            if(newGame.host == "Eternal-")
            {

            }
            switch (newGame.FeaturedMod)
            {
                case FeaturedMod.FAF:
                //case FeaturedMod.FAFBeta:
                //case FeaturedMod.FAFDevelop:
                    break;
                default: return;
            }

            switch (newGame.GameType)
            {
                case GameType.Coop:
                //case GameType.MatchMaker:
                    return;
            }

            newGame.Updated = DateTime.UtcNow;
            var games = Games;

            if (TryGetGame(newGame.uid, out var game))
            {
                game.Updated = DateTime.UtcNow;
                if (newGame.State == GameState.Closed)
                {
                    if (game.State == GameState.Playing)
                    {
                        // game is end
                        OnGameEnd(game);
                    }
                    else
                    {
                        game.State = GameState.Closed;
                        // host closed game
                        OnGameClosed(game);
                    }

                    for (int i = 0; i < game.Players.Length; i++)
                    {
                        game.Players[i].Game = null;
                    }
                    return;
                }

                //Logger.LogInformation($"Updating game {newGame.uid} by {newGame.host}");
                var oldState = game.State;
                    
                // for visual UI notification, that players amount is changed
                game.PlayersCountChanged = newGame.num_players - game.num_players;

                game.title = newGame.title;
                game.num_players = newGame.num_players;

                var left = game.PlayersLogins
                    .Except(newGame.PlayersLogins)
                    .ToArray();

                if (left.Length > 0)
                {
                    PlayersLeftFromGame?.Invoke(this, left);
                    //Logger.LogInformation($"Left from game {newGame.uid} by {newGame.host}: {string.Join(',', left)}");
                    for (int i = 0; i < left.Length; i++)
                    {
                        var leftPlayer = PlayersService.GetPlayer(left[i]);
                        if (leftPlayer is null)
                        {
                            Logger.LogError($"Player {left[i]} left from game but not found");
                        }
                        else
                        {
                            leftPlayer.Game = null;
                        }
                    }
                }

                var joined = newGame.PlayersLogins
                    .Except(game.PlayersLogins)
                    .ToArray();

                if (joined.Length > 0)
                {
                    PlayersJoinedToGame?.Invoke(this, new(newGame, joined));
                    //Logger.LogInformation($"Joined to game {newGame.uid} by {newGame.host}: {string.Join(',', joined)}");
                    for (int i = 0; i < joined.Length; i++)
                    {
                        var joinedPlayer = PlayersService.GetPlayer(joined[i]);
                        if (joinedPlayer is null)
                        {
                            Logger.LogError("Player that joined to game not found");
                        }
                        else
                        {
                            joinedPlayer.Game = game;
                        }
                    }
                }

                // TODO Optimize to not load everytime
                game.teams = newGame.teams;
                HandleTeams(game);

                if (game.Host is null)
                {
                    game.Host = PlayersService.GetPlayer(game.host);
                }

                // map area update
                game.map_file_path = newGame.map_file_path;
                game.max_players = newGame.max_players;
                game.mapname = newGame.mapname;

                game.sim_mods = newGame.sim_mods;

                game.State = newGame.State;
                game.launched_at = newGame.launched_at;
                
                if (game.State == GameState.Playing && oldState == GameState.Open)
                {
                    // game launched
                    GameLaunched?.Invoke(this, game);
                }
                else
                {
                    OnGameUpdated(game);
                }
            }
            else
            {
                //Logger.LogInformation($"Received new game {newGame.uid} by {newGame.host}");

                newGame.OldState = GameState.None;
                if (newGame.State == GameState.Closed)
                {
                    //Logger.LogInformation($"Closed game {newGame.uid} by {newGame.host}: didnt pass");
                    return;
                }
                newGame.Host = PlayersService.GetPlayer(newGame.host);
                HandleTeams(newGame);
                games.Add(newGame);
                OnNewGameReceived(newGame);
            }
        }
        private void OnGameReceived(object sender, GameInfoMessage e) => HandleGameData(e);
        private void OnNewGameReceived(GameInfoMessage game) => NewGameReceived?.Invoke(this, game);
        private void OnGameUpdated(GameInfoMessage game) => GameUpdated?.Invoke(this, game);
        private void OnGameEnd(GameInfoMessage game) => GameEnd?.Invoke(this, game);
        private void OnGameClosed(GameInfoMessage game) => GameClosed?.Invoke(this, game);
        private void OnGameLaunched(GameInfoMessage game) => GameLaunched?.Invoke(this, game);

        public GameInfoMessage GetGame(long uid)
        {
            if (TryGetGame(uid, out var game)) return game;
            return null;
        }
    }
}
