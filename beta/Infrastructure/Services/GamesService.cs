using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Server;
using beta.Models.Server.Enums;
using beta.ViewModels.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public class GamesService : ViewModel, IGamesService
    {
        public event EventHandler<GameInfoMessage[]> GamesReceived;
        public event EventHandler<GameInfoMessage> NewGameReceived;
        public event EventHandler<GameInfoMessage> GameUpdated;
        public event EventHandler<GameInfoMessage> GameRemoved;
        public event EventHandler<long> GameRemovedByUid;

        #region Properties

        #region Services

        private readonly ISessionService SessionService;
        private readonly IPlayersService PlayersService;
        private readonly IMapsService MapService;

        #endregion

        public ObservableCollection<GameInfoMessage> IdleGames { get; } = new();
        public ObservableCollection<GameInfoMessage> LiveGames { get; } = new();

        public List<GameInfoMessage> Games { get; } = new();

        /// <summary>
        /// Idle games without players. Bugged or have been created with 0 players
        /// </summary>
        private readonly List<GameInfoMessage> SuspiciousGames = new();

        private readonly ILogger Logger;

        #endregion

        public GamesService(
            ISessionService sessionService,
            IPlayersService playerService,
            IMapsService mapService, ILogger<GamesService> logger)
        {
            SessionService = sessionService;
            PlayersService = playerService;
            MapService = mapService;

            sessionService.GameReceived += OnGameReceived;
            sessionService.GamesReceived += OnGamesReceived;
            Logger = logger;
        }

        private void OnGamesReceived(object sender, GameInfoMessage[] e) => Task.Run(async () =>
        {
            //Logger.LogInformation($"Received {e.Length} games from lobby-server");
            foreach (var game in e) await HandleGameData(game);
        });

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

        private async Task<bool> UpdateGame(GameInfoMessage orig, GameInfoMessage newData)
        {
            //if (orig.num_players > newData.num_players && newData.num_players == 0)
            //    return false;

            if (orig.sim_mods.Count == 0 && orig.sim_mods.Count > 1)
            {
                // idk
            }

            // for visual UI notification, that players amount is changed
            orig.PlayersCountChanged = newData.num_players - orig.num_players;

            orig.title = newData.title;

            orig.num_players = newData.num_players;


            // players update area

            List<string> origPlayers = new();
            foreach (var team in orig.teams) origPlayers.AddRange(team.Value);

            List<string> newPlayers = new();
            foreach (var team in newData.teams) origPlayers.AddRange(team.Value);
            
            PlayersService.RemoveGameFromPlayers(origPlayers
                .Except(newPlayers)
                .ToArray());

            PlayersService.AddGameToPlayers(newPlayers
                .Except(origPlayers)
                .ToArray(), orig);

            orig.Teams = newData.Teams;


            // map area update

            if (orig.mapname != newData.mapname)
            {
                orig.map_file_path = newData.map_file_path;
                orig.max_players = newData.max_players;

                //orig.Map = await MapService.GetGameMap(newData.mapname);

                // should be updates latest, because it triggers UI updates for other map related fields
                orig.mapname = newData.mapname;
            }

            return true;
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

            PlayersService.RemoveGameFromPlayers(playersToClear.ToArray());
            GameRemovedByUid?.Invoke(this, game.uid);
            OnGameRemoved(game);
        }


        private async Task HandleGameData(GameInfoMessage newGame)
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

            if (newGame.sim_mods.Count > 0) return;

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
            
            //TODO rewrite for Task?
            newGame.Teams = GetInGameTeams(newGame);

            if (TryGetGame(newGame.uid, out var game))
            {
                //Logger.LogInformation($"Received updates on game {newGame.uid} by {newGame.host}");
                await UpdateGame(game, newGame);

                if (game.Host is IPlayer)
                {
                    game.Host = PlayersService.GetPlayer(newGame.host);
                }

                OnGameUpdated(game);
            }
            else
            {
                //Logger.LogInformation($"Received new game {newGame.uid} by {newGame.host} from lobby-server");
                // currently we are not supporting UI notification about new game
                if (newGame.num_players == 0 || newGame.State == GameState.Closed) return;

                newGame.Map = await MapService.GetGameMap(newGame.mapname);

                newGame.Host = PlayersService.GetPlayer(newGame.host);

                games.Add(newGame);
                OnNewGameReceived(newGame);
            }
        }
        private void OnGameReceived(object sender, GameInfoMessage e) => Task.Run(() => HandleGameData(e));

        public InGameTeam[] GetInGameTeams(GameInfoMessage game)
        {
            InGameTeam[] teams = new InGameTeam[game.teams.Count];

            int j = 0;

            // TODO: FIX ME Need to rework player game status

            //var playerStatus = GameState.Open;
            //if (game.launched_at is not null)
            //{
            //    var timeDifference = DateTime.UtcNow - DateTime.UnixEpoch.AddSeconds(game.launched_at.Value);
            //    playerStatus = timeDifference.TotalSeconds < 300 ? GameState.Playing5 : GameState.Playing;
            //}

            foreach (var valuePair in game.teams)
            {
                var players = new IPlayer[valuePair.Value.Length];

                for (int i = 0; i < valuePair.Value.Length; i++)
                {
                    var player = PlayersService.GetPlayer(valuePair.Value[i]);
                    if (player is null)
                    {
                        players[i] = new UnknownPlayer()
                        {
                            login = valuePair.Value[i]
                        };
                        continue;
                    }

                    //player.GameState = playerStatus;
                    if (player.Game is null)
                        player.Game = game;
                    else if (player.Game.uid != game.uid)
                        player.Game = game;

                    players[i] = player;
                }

                teams[j] = new(valuePair.Key, players);
                j++;
            }
            return teams;
        }
        public GameInfoMessage GetGame(long uid)
        {
            throw new NotImplementedException();
        }

        private void OnNewGameReceived(GameInfoMessage game) => NewGameReceived?.Invoke(this, game);
        private void OnGameUpdated(GameInfoMessage game) => GameUpdated?.Invoke(this, game);
        private void OnGameRemoved(GameInfoMessage game) => GameRemoved?.Invoke(this, game);

    }
}
