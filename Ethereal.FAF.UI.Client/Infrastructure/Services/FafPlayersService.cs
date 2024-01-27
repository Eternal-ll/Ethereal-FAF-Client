using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.ViewModels;
using FAF.Domain.LobbyServer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class FafPlayersService : IFafPlayersService, IFafPlayersEventsService
    {
        #region IFafPlayersEventsService
        public event EventHandler<Player[]> PlayersAdded;
        public event EventHandler<Player[]> PlayersUpdated;
        public event EventHandler<Player[]> PlayersRemoved;
        public event EventHandler<Player> PlayerConnected;
        public event EventHandler<Player> PlayerDisconnected; 
        #endregion

        private readonly IFafLobbyEventsService _fafLobbyEventsService;
        private readonly ILogger<FafPlayersService> _logger;
        private readonly ConcurrentDictionary<long, Player> _players;

        private bool _playersInitialized;

        public FafPlayersService(IFafLobbyEventsService fafLobbyEventsService, ILogger<FafPlayersService> logger)
        {
            _players = new();
            _fafLobbyEventsService = fafLobbyEventsService;
            _logger = logger;

            InitializeEvents();
        }
        private void InitializeEvents()
        {
            _fafLobbyEventsService.OnConnection += _fafLobbyEventsService_OnConnection;
            _fafLobbyEventsService.PlayersReceived += _fafLobbyEventsService_PlayersReceived;
            _fafLobbyEventsService.PlayerReceived += _fafLobbyEventsService_PlayerReceived;
        }

        private void _fafLobbyEventsService_PlayerReceived(object sender, PlayerInfoMessage serverPlayer)
        {
            var e = serverPlayer.MapToViewModel();
            PrepareRatings(e);
            if (_players.TryGetValue(e.Id, out var player))
            {
                if (e.IsOffline)
                {
                    _logger.LogInformation("Player [{playerId}] disconnected", e.Id);
                    if (_players.TryRemove(e.Id, out player))
                    {
                        PlayersRemoved?.Invoke(this, new[] { player });
                    }
                }
                return;
            }
            if (e.IsOffline)
            {
                _logger.LogWarning("Player [{playerId}] disconnected and is missing in storage", e.Id);
                return;
            }
            _logger.LogInformation("Player [{playerId}] connected", e.Id);
            _players.TryAdd(e.Id, e);
            PlayersAdded?.Invoke(this, new[] { e });
        }

        private void _fafLobbyEventsService_PlayersReceived(object sender, PlayerInfoMessage[] data)
        {
            if (_playersInitialized)
            {
                foreach (var player in data)
                {
                    _fafLobbyEventsService_PlayerReceived(sender, player);
                }
                return;
            }
            var e = data.Select(x => x.MapToViewModel()).ToArray();
            foreach (var player in e)
            {
                _players.TryAdd(player.Id, player);
                PrepareRatings(player);
            }
            PlayersAdded?.Invoke(this, e);
            _playersInitialized = true;
        }
        private void _fafLobbyEventsService_OnConnection(object sender, bool e)
        {
            if (e)
            {
                _playersInitialized = false;
            }
            else
            {
                PlayersRemoved?.Invoke(this, _players.Values.ToArray());
                _players.Clear();
            }
        }

        public Player[] GetPlayers() => _players.Values.ToArray();

        public bool TryGetPlayer(string login, out Player player)
        {
            player = _players.Values.ToArray().FirstOrDefault(x => x.Login.ToLower() == login.ToLower());
            return player != null;
        }

        public bool TryGetPlayer(long id, out Player player) => _players.TryGetValue(id, out player);
        private void PrepareRatings(params Player[] players)
        {
            foreach (var player in players)
            {
                if (player.Ratings.Global is not null) player.Ratings.Global.name = "Global";
                if (player.Ratings.Ladder1V1 is not null) player.Ratings.Ladder1V1.name = "1 vs 1";
                if (player.Ratings.Tmm2V2 is not null) player.Ratings.Tmm2V2.name = "2 vs 2";
                if (player.Ratings.Tmm4V4FullShare is not null) player.Ratings.Tmm4V4FullShare.name = "4 vs 4";
            }
        }
    }
}
