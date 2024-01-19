using Ethereal.FAF.UI.Client.Models.Lobby;
using FAF.Domain.LobbyServer.Enums;
using System;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IFafGamesEventsService
    {
        public event EventHandler<Game[]> GamesAdded;
        public event EventHandler<Game[]> GamesUpdated;
        public event EventHandler<Game[]> GamesRemoved;


        public event EventHandler<(Game, GameState newest, GameState latest)> GameStateChanged;
    }
}
