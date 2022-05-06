using beta.Models.Server;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Player avatar service
    /// </summary>
    public interface IAvatarService
    {
        /// <summary>
        /// Update player avatar image
        /// </summary>
        /// <param name="player"></param>
        /// <param name="avatar"></param>
        /// <returns></returns>
        public Task UpdatePlayerAvatarAsync(PlayerInfoMessage player, PlayerAvatar avatar);

    }
}
