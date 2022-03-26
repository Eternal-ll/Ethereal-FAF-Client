using beta.Models.Server;
using System.Collections.ObjectModel;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGamesService
    {
        /// <summary>
        /// Available idle games for joining
        /// </summary>
        public ObservableCollection<GameInfoMessage> IdleGames { get; }
        
        /// <summary>
        /// Launched games
        /// </summary>
        public ObservableCollection<GameInfoMessage> LiveGames { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public InGameTeam[] GetInGameTeams(GameInfoMessage game);
    }
}
