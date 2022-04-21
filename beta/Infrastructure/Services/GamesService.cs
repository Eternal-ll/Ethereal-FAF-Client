using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.Models.Server.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace beta.Infrastructure.Services
{
    public class GamesService : IGamesService
    {
        public event EventHandler<GameInfoMessage> NewGameReceived;
        public event EventHandler<GameInfoMessage> GameUpdated;
        public event EventHandler<GameInfoMessage> GameRemoved;
        public event EventHandler<GameInfoMessage> GameLaunched;
        public event EventHandler<long> GameRemovedByUid;
        public event EventHandler<string[]> PlayersLeftFromGame;
        public event EventHandler<KeyValuePair<GameInfoMessage, string[]>> PlayersJoinedToGame;

        private readonly ISessionService SessionService;
        private readonly ILogger Logger;
        
        public List<GameInfoMessage> Games { get; }

        public GamesService(ISessionService sessionService, ILogger<GamesService> logger)
        {
            SessionService = sessionService;
            Logger = logger;

            Games = new();

            sessionService.GameReceived += OnGameReceived;
            sessionService.GamesReceived += OnGamesReceived;

            sessionService.StateChanged += SessionService_StateChanged;
        }

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
                Games.Remove(foundGame);
                foundGame.Dispose();
                foundGame = null;
            }
            PlayersLeftFromGame?.Invoke(this, playersToClear.ToArray());
            GameRemovedByUid?.Invoke(this, game.uid);
            OnGameRemoved(game);
        }

        private void HandleGameData(GameInfoMessage newGame)
        {
            switch (newGame.FeaturedMod)
            {
                case FeaturedMod.FAF:
                case FeaturedMod.FAFBeta:
                case FeaturedMod.FAFDevelop:
                    break;
                default: return;
            }

            switch (newGame.GameType)
            {
                case GameType.Coop:
                case GameType.MatchMaker:
                    return;
            }

            switch (newGame.State)
            {
                case GameState.Open:
                    break;
                case GameState.Playing:
                    break;
                case GameState.Closed:
                    HandleOnGameClose(newGame);
                    return;
                default:
                    // LOG
                    return;
            }

            var games = Games;

            if (TryGetGame(newGame.uid, out var game))
            {
                //Logger.LogInformation($"Updating game {newGame.uid} by {newGame.host}");
                var oldState = game.State;

                // for visual UI notification, that players amount is changed
                game.PlayersCountChanged = game.num_players - newGame.num_players;

                game.title = newGame.title;
                game.num_players = newGame.num_players;

                // players update area
                List<string> originalPlayers = new();
                foreach (var team in game.teams) originalPlayers.AddRange(team.Value);

                List<string> newPlayers = new();
                foreach (var team in newGame.teams) newPlayers.AddRange(team.Value);

                var left = originalPlayers
                    .Except(newPlayers)
                    .ToArray();

                if (left.Length > 0)
                {
                    PlayersLeftFromGame?.Invoke(this, left);
                    //Logger.LogInformation($"Left from game {newGame.uid} by {newGame.host}: {string.Join(',', left)}");
                }

                var joined = newPlayers
                    .Except(originalPlayers)
                    .ToArray();

                if (joined.Length > 0)
                {
                    PlayersJoinedToGame?.Invoke(this, new(newGame, joined));
                    //Logger.LogInformation($"Joined to game {newGame.uid} by {newGame.host}: {string.Join(',', joined)}");
                }

                // map area update
                game.map_file_path = newGame.map_file_path;
                game.max_players = newGame.max_players;
                //newGame.Map = await MapService.GetGameMap(game.mapname);
                // should be updates latest, because it triggers UI updates for other map related fields
                game.mapname = newGame.mapname;


                game.State = newGame.State;
                
                OnGameUpdated(game);

                if (game.State != oldState && game.State == GameState.Playing)
                {
                    GameLaunched?.Invoke(this, game);
                }
            }
            else
            {
                //Logger.LogInformation($"Received new game {newGame.uid} by {newGame.host}");
                // currently we are not supporting UI notification about new game
                if (newGame.num_players == 0)
                {
                    Logger.LogInformation($"Game didnt pass {newGame.uid} by {newGame.host}: pp ({newGame.num_players}) / state ({newGame.State})");
                    return;
                }
                games.Add(newGame);
                OnNewGameReceived(newGame);
            }
        }
        private void OnGameReceived(object sender, GameInfoMessage e) => HandleGameData(e);
        private void OnNewGameReceived(GameInfoMessage game) => NewGameReceived?.Invoke(this, game);
        private void OnGameUpdated(GameInfoMessage game) => GameUpdated?.Invoke(this, game);
        private void OnGameRemoved(GameInfoMessage game) => GameRemoved?.Invoke(this, game);

    }


}
