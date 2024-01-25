using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class FafGamesService : IFafGamesService, IFafGamesEventsService
    {
        public event EventHandler<Game> GameAdded;
        public event EventHandler<(Game Cached, Game Incoming)> GameUpdated;
        public event EventHandler<Game> GameRemoved;
        public event EventHandler<(Game, GameState newest, GameState latest)> GameStateChanged;

        private readonly IFafLobbyEventsService _fafLobbyEventsService;
        private readonly IFafPlayersService _fafPlayersService;
        private readonly ILogger<FafGamesService> _logger;
        private readonly ClientManager _clientManager;
        private readonly ConcurrentDictionary<long, Game> _games;

        private bool _gamesInitialized;

        public FafGamesService(IFafLobbyEventsService fafLobbyEventsService, ClientManager clientManager, IFafPlayersService fafPlayersService, ILogger<FafGamesService> logger)
        {
            _games = new();
            _fafLobbyEventsService = fafLobbyEventsService;
            _clientManager = clientManager;
            _fafPlayersService = fafPlayersService;
            InitializeEvents();
            _logger = logger;
        }

        private void InitializeEvents()
        {
            _fafLobbyEventsService.OnConnection += _fafLobbyEventsService_Connected;
            _fafLobbyEventsService.GameReceived += _fafLobbyEventsService_GameReceived;
            _fafLobbyEventsService.GamesReceived += _fafLobbyEventsService_GamesReceived;
        }

        private void _fafLobbyEventsService_GamesReceived(object sender, Game[] e)
        {
            if (_gamesInitialized)
            {
                foreach (var game in e)
                {
                    _fafLobbyEventsService_GameReceived(sender, game);
                }
                return;
            }
            _gamesInitialized = true;
            _logger.LogInformation("Received [{gamesCount}] games", e.Length);
            foreach (var game in e)
            {
                if (!game.IsMapgen)
                {
                    game.SmallMapPreview = $"{_clientManager.GetServer().Content}maps/previews/small/{game.Mapname}.png";
                }
                game.UpdateTeams();
                FillPlayers(game);
                _games.TryAdd(game.Uid, game);
                GameAdded?.Invoke(this, game);
            }
        }

        private void _fafLobbyEventsService_GameReceived(object sender, Game e) => ProceedGame(e);

        private void _fafLobbyEventsService_Connected(object sender, bool e)
        {
            if (!e)
            {
                foreach (var game in _games.Values)
                {
                    GameRemoved?.Invoke(this, game);
                }
                _games.Clear();
                _gamesInitialized = false;
            }
        }

        public Game[] GetGames() => _games.Values.ToArray();


        #region Game

        private void FillPlayers(Game game)
        {
            foreach (var player in game.Players)
            {
                if (!_fafPlayersService.TryGetPlayer(player.Id, out var cache))
                {
                    continue;
                }
                if (player.Login == game.Host)
                {
                    game.HostPlayer = cache;
                    player.IsHost = true;
                }
                player.Player = cache;
                cache.Game = game;
            }
        }

        private void ProceedGame(Game e)
        {
            var games = _games;
            if (!games.TryGetValue(e.Uid, out var cached))
            {
                if (e.State == GameState.Closed)
                {
                    if (e.PlayersIds.Any())
                    {
                        // game is closing
                    }
                    else
                    {
                        // game fully closed
                    }
                    return;
                }
                if (e.State is GameState.Playing && e.NumPlayers == 0)
                {

                    return;
                }
                //if (e.State is GameState.Closed)
                //{
                //    // received new closed game
                //    //Logger.LogWarning("Received game [{uid}] that was [GameState.Closed] and not founded in cache", e.Uid);
                //    Logger.LogWarning($"Received [{e.Uid}] [{e.NumPlayers}] [{e.MaxPlayers}] [{e.State}]");
                //    return;
                //}
                //if (IsBadGame(e))
                //{
                //    if (e.State is GameState.Closed)
                //    {
                //        // received new closed game
                //        Logger.LogWarning("Received game [{uid}] that was [GameState.Closed] and not founded in cache", e.Uid);
                //        return;
                //    }
                //    return;
                //}
                //newGame.Host = PlayersService.GetPlayer(newGame.host);
                //HandleTeams(newGame);
                e.UpdateTeams();
                FillPlayers(e);
                if (games.TryAdd(e.Uid, e))
                {
                    if (e.State == GameState.Open)
                    {
                        _logger.LogInformation("Game [{gameId}] [{state}] new opened", e.Uid, e.State);
                    }
                    else if (e.State == GameState.Playing)
                    {
                        _logger.LogInformation("Game [{gameId}] [{state}] new launched game", e.Uid, e.State);
                    }
                    GameAdded?.Invoke(this, e);
                }
                //Logger.LogInformation(e.Title);
                //OnNewGameReceived(newGame);
                return;
            }

            var leftPlayers = cached.PlayersIds
                .Except(e.PlayersIds)
                .ToArray();

            if (e.State is GameState.Closed)
            {
                ProcessLeftPlayers(cached, leftPlayers);
                
                if (games.TryRemove(cached.Uid, out _))
                {
                    if (cached.State == GameState.Playing)
                    {
                        // game ended
                        _logger.LogInformation("Game [{gameId}] [{state}] finished",
                            e.Uid, e.State);
                    }
                    else if (cached.State == GameState.Open)
                    {
                        // lobby closed
                        _logger.LogInformation("Game [{gameId}] [{state}] closed by host",
                            e.Uid, e.State);
                        GameStateChanged?.Invoke(this, (cached, e.State, cached.State));
                    }
                    GameRemoved?.Invoke(this, cached);
                }
                
            }
            else if (e.State is GameState.Playing)
            {
                if (cached.State is GameState.Playing)
                {
                    //_logger.LogInformation("Game [{gameId}] [{state}] running, received update", e.Uid, e.State);
                    if (e.NumPlayers < cached.NumPlayers && e.NumPlayers == 0)
                    {
                        if (games.TryRemove(cached.Uid, out _))
                        {
                            GameRemoved?.Invoke(this, cached);
                        }
                        return;
                    }
                }
            }

            var joinedPlayers = e.PlayersIds
                .Except(cached.PlayersIds)
                .ToArray();

            // lets update cached game
            cached.Title = e.Title;
            cached.Mapname = e.Mapname;
            cached.MaxPlayers = e.MaxPlayers;
            cached.NumPlayers = e.NumPlayers;
            cached.LaunchedAt = e.LaunchedAt;
            //OnPropertyChanged(nameof(cached.LaunchedAtTimeSpan));

            if (e.State is GameState.Playing && cached.State is GameState.Playing)
            {
                // update playing game
                ProcessDiedPlayers(cached, leftPlayers);
            }
            else
            {
                // game updated
                ProcessLeftPlayers(cached, leftPlayers);
                UpdateTeams(cached, e);
                FillPlayers(cached);
                ProcessJoinedPlayers(cached, joinedPlayers);
            }

            if (e.State is GameState.Playing)
            {
                if (cached.State is GameState.Open)
                {
                    _logger.LogInformation("Game [{gameId}] [{state}] launched", e.Uid, e.State);
                    //Logger.LogTrace("Game [{title}] [{mod}] [{rating}] launched, updating list on index",
                    //    e.Title, e.FeaturedMod, e.RatingType);
                    e.GameTeams = cached.GameTeams;
                    if (cached.MapGeneratorState is MapGeneratorState.Generated)
                    {
                        e.SmallMapPreview = cached.SmallMapPreview;
                        e.MapGeneratorState = cached.MapGeneratorState;
                    }
                    e.HostPlayer = cached.HostPlayer;
                    //Games[index] = e;
                    GameStateChanged?.Invoke(this, (cached, e.State, cached.State));
                }
            }
            cached.State = e.State;
            GameUpdated?.Invoke(this, (cached, e));
        }
        private void UpdateTeams(Game oldGame, Game newGame)
        {
            oldGame.Teams = newGame.Teams;
            oldGame.TeamsIds = newGame.TeamsIds;
            oldGame.UpdateTeams();
        }

        private void ProcessJoinedPlayers(Game game, params long[] joinedPlayers)
        {
            foreach (var joined in joinedPlayers)
            {
                if (!game.TryGetPlayer(joined, out var player))
                {
                    continue;
                }
            }
        }
        private void ProcessLeftPlayers(Game game, params long[] leftPlayers)
        {
            foreach (var left in leftPlayers)
            {
                Player cached;
                if (!game.TryGetPlayer(left, out var player))
                {
                    if (!_fafPlayersService.TryGetPlayer(left, out cached))
                    {
                        // welp... GG
                    }
                }
                else
                {
                    cached = player.Player;
                }
                if (cached is not null)
                    cached.Game = null;
            }
        }
        private void ProcessDiedPlayers(Game game, params long[] diedPlayers)
        {
            foreach (var died in diedPlayers)
            {
                if (!game.TryGetPlayer(died, out var player))
                {
                    continue;
                }
                player.IsConnected = false;
            }
        }

        public Game GetGame(long id)
        {
            if (_games.TryGetValue(id, out var game)) return game;
            return null;
        }
        #endregion
    }
}
