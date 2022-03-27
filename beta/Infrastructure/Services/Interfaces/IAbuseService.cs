using System;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Interface of moderator service
    /// 27.03.2022 - No needs
    /// </summary>
    public interface IAbuseService
    {
        /// <summary>
        /// Kill specific player game instance
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public Task KillPlayerGameInstance(int id);
        /// <summary>
        /// Kick specific plaeyr from lobby session
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public Task KillPlayerServerSession(int id);
        /// <summary>
        /// Ban player without the reason
        /// </summary>
        /// <param name="id">Player id</param>
        /// <param name="duration">Duration of bans</param>
        /// <returns></returns>
        public Task BanPlayer(int id, TimeSpan duration);
        /// <summary>
        /// Ban player with reason
        /// </summary>
        /// <param name="id">Player od</param>
        /// <param name="duration">Duration of ban</param>
        /// <param name="reason">Reason of ban</param>
        /// <returns></returns>
        public Task BanPlayer(int id, TimeSpan duration, string reason);
        /// <summary>
        /// Ban player forever without the reason
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public Task BanPlayerForever(int id);
        /// <summary>
        /// Ban player forever with attached reason
        /// </summary>
        /// <param name="id">Player id</param>
        /// <param name="reason">Reason of ban</param>
        /// <returns></returns>
        public Task BanPlayerForever(int id, string reason);
        /// <summary>
        /// Unban player, make him happy again
        /// </summary>
        /// <param name="id">Player id</param>
        /// <returns></returns>
        public Task UnbanPlayer(int id);
    }
}
