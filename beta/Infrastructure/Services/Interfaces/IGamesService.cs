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
        /// <summary>
        /// New game of any instances received
        /// </summary>
        public event EventHandler<GameInfoMessage> NewGameReceived;
        /// <summary>
        /// Any game of any instances updated
        /// </summary>
        public event EventHandler<GameInfoMessage> GameUpdated;
        //public event EventHandler<GameInfoMessage> GameRemoved;

        /// <summary>
        /// Idle lobby launched
        /// </summary>
        public event EventHandler<GameInfoMessage> GameLaunched;
        /// <summary>
        /// Active match is end, everyone is left
        /// </summary>
        public event EventHandler<GameInfoMessage> GameEnd;
        /// <summary>
        /// Host left from idle lobby
        /// </summary>
        public event EventHandler<GameInfoMessage> GameClosed;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<string[]> PlayersLeftFromGame;
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<KeyValuePair<GameInfoMessage, string[]>> PlayersJoinedToGame;

        public event EventHandler<KeyValuePair<GameInfoMessage, PlayerInfoMessage[]>> PlayersJoinedGame;
        public event EventHandler<KeyValuePair<GameInfoMessage, PlayerInfoMessage[]>> PlayersLeftGame;

        public List<GameInfoMessage> Games { get; }
    }
    public interface INewGamesService<GameType, GameState> : IGamesService
    {

    }
}
