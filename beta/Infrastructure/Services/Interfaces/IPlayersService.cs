using beta.Models.Enums;
using beta.Models.Server;
using beta.Models.Server.Enums;
using System;
using System.Collections.Generic;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Players service that controls data from lobby-server
    /// </summary>
    public interface IPlayersService
    {
        /// <summary>
        /// Self player instance received
        /// </summary>
        public event EventHandler<PlayerInfoMessage> SelfReceived;

        /// <summary>
        /// Lobby-server players received
        /// </summary>
        public event EventHandler<PlayerInfoMessage[]> PlayersReceived;
        /// <summary>
        /// New player received
        /// </summary>
        public event EventHandler<PlayerInfoMessage> PlayerReceived;
        /// <summary>
        /// Player data updated
        /// </summary>
        public event EventHandler<PlayerInfoMessage> PlayerUpdated;
        /// <summary>
        /// Player disconnected
        /// </summary>
        public event EventHandler<PlayerInfoMessage> PlayerLeft;
        /// <summary>
        /// Saved players instances
        /// </summary>
        public PlayerInfoMessage[] CachedPlayers { get; }
        /// <summary>
        /// Players
        /// </summary>
        public PlayerInfoMessage[] Players { get; }
        /// <summary>
        /// Self instance
        /// </summary>
        public PlayerInfoMessage Self { get; }
        /// <summary>
        /// Get player from cache
        /// </summary>
        /// <param name="idOrLogin"></param>
        /// <returns></returns>
        public PlayerInfoMessage GetPlayer(string idOrLogin);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool TryGetPlayer(string login, out PlayerInfoMessage player);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool TryGetPlayer(int id, out PlayerInfoMessage player);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="method"></param>
        /// <param name="relationShip"></param>
        /// <returns></returns>
        public IEnumerable<PlayerInfoMessage> GetPlayers(string filter = null, ComparisonMethod method = ComparisonMethod.STARTS_WITH,
            PlayerRelationShip? relationShip = null);
    }
}
