using Ethereal.FAF.UI.Client.Models.Lobby;
using FAF.Domain.LobbyServer.Enums;
using System;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IFafGamesEventsService
    {
        public event EventHandler<Game> GameAdded;
        public event EventHandler<(Game Cached, Game Incoming)> GameUpdated;
        public event EventHandler<Game> GameRemoved;


        public event EventHandler<(Game, GameState newest, GameState latest)> GameStateChanged;
    }
}
