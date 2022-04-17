using beta.Models.Enums;
using beta.Models.Server;
using beta.Models.Server.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IPlayersService : INotifyPropertyChanged
    {
        public event EventHandler<PlayerInfoMessage> MeReceived;
        public event EventHandler<PlayerInfoMessage[]> PlayersReceived;
        public event EventHandler<PlayerInfoMessage> PlayerReceived;

        public event EventHandler<PlayerInfoMessage> PlayerUpdated;
        
        public PlayerInfoMessage[] CachedPlayers { get; }

        public ObservableCollection<PlayerInfoMessage> Players { get; }
        public PlayerInfoMessage Me { get; }

        public PlayerInfoMessage GetPlayer(string login);
        public PlayerInfoMessage GetPlayer(int id);

        public bool TryGetPlayer(string login, out PlayerInfoMessage player);
        public bool TryGetPlayer(int id, out PlayerInfoMessage player);

        public IEnumerable<PlayerInfoMessage> GetPlayers(string filter = null, ComparisonMethod method = ComparisonMethod.STARTS_WITH,
            PlayerRelationShip? relationShip = null);


        /// <summary>
        /// Adds game to player instance
        /// </summary>
        /// <param name="login">Player login</param>
        /// <param name="game">Game instance</param>
        /// <returns>Returns true if game was added, otherwise false</returns>
        public bool AddGameToPlayer(string login, GameInfoMessage game);
        /// <summary>
        /// Adds game to players instance
        /// </summary>
        /// <param name="logins">Players login</param>
        /// <param name="game">Game instance</param>
        /// <returns>Returns true if game was added to all players, otherwise false</returns>
        public bool AddGameToPlayers(string[] logins, GameInfoMessage game);
        /// <summary>
        /// Removes game instance from player instance
        /// </summary>
        /// <param name="login">Player login</param>
        /// <param name="uid">Game uid</param>
        /// <returns>Returns true if game was removed from player, otherwise false</returns>
        public bool RemoveGameFromPlayer(string login, long? uid = null);
        /// <summary>
        /// Removes game instance from players
        /// </summary>
        /// <param name="logins">Players logins</param>
        /// <param name="uid">Game uid</param>
        /// <returns>Returns true if game was removed from all players, otherwise false</returns>
        public bool RemoveGameFromPlayers(string[] logins, long? uid = null);
    }
}
