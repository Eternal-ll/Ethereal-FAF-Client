using beta.Models.Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGamesService
    {
        public event EventHandler<GameInfoMessage[]> GamesReceived;
        public event EventHandler<GameInfoMessage> NewGameReceived;
        public event EventHandler<GameInfoMessage> GameUpdated;
        public event EventHandler<GameInfoMessage> GameRemoved;
        public event EventHandler<long> GameRemovedByUid;

        /// <summary>
        /// Available idle games for joining
        /// </summary>
        public ObservableCollection<GameInfoMessage> IdleGames { get; }
        
        /// <summary>
        /// Launched games
        /// </summary>
        public ObservableCollection<GameInfoMessage> LiveGames { get; }

        public List<GameInfoMessage> Games { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public InGameTeam[] GetInGameTeams(GameInfoMessage game);
    }
}
