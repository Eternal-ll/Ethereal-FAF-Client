using beta.Models.Server;
using System;
using System.Collections.Generic;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGamesService
    {
        public event EventHandler<GameInfoMessage> NewGameReceived;
        public event EventHandler<GameInfoMessage> GameUpdated;
        public event EventHandler<GameInfoMessage> GameRemoved;
        public event EventHandler<GameInfoMessage> GameLaunched;
        public event EventHandler<long> GameRemovedByUid;

        public event EventHandler<string[]> PlayersLeftFromGame;
        public event EventHandler<KeyValuePair<GameInfoMessage, string[]>> PlayersJoinedToGame;

        public List<GameInfoMessage> Games { get; }
    }
    public interface INewGamesService<GameType, GameState> : IGamesService
    {

    }
}
