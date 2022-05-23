using beta.Models.Server;
using System;
using System.Collections.Generic;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Games service that provides lobby related data from server. 
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
        /// Logins of players that left from specific game
        /// </summary>
        public event EventHandler<string[]> PlayersLeftFromGame;
        /// <summary>
        /// Logins of players that joined to specific game
        /// </summary>
        public event EventHandler<KeyValuePair<GameInfoMessage, string[]>> PlayersJoinedToGame;
        
        /// <summary>
        /// Players that joined to specific game
        /// </summary>
        public event EventHandler<KeyValuePair<GameInfoMessage, PlayerInfoMessage[]>> PlayersJoinedGame;
        /// <summary>
        /// Players that left specific game
        /// </summary>
        public event EventHandler<KeyValuePair<GameInfoMessage, PlayerInfoMessage[]>> PlayersLeftGame;

        /// <summary>
        /// List of all games
        /// </summary>
        public List<GameInfoMessage> Games { get; }
        /// <summary>
        /// Get the game instance by uid
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public GameInfoMessage GetGame(long uid);
    }
    public interface INewGamesService<GameType, GameState> : IGamesService
    {

    }
}
