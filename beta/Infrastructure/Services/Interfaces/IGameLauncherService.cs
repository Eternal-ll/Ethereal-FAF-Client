using beta.Models.Server;
using beta.Models.Server.Enums;
using System;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IGameLauncherService
    {
        public Task JoinGame(GameInfoMessage game);

        public Task Join(GameInfoMessage game);

        /// <summary>
        /// Sends command to lobby-server for hosting game
        /// </summary>
        /// <param name="title">Title of lobby</param>
        /// <param name="mod">Game mod</param>
        /// <param name="visibility">Is lobby for friends</param>
        /// <param name="mapName">Map name</param>
        /// <param name="password">Secure lobby with password</param>
        /// <param name="isRehost">If you are rehosting last game</param>
        /// <returns></returns>
        public Task HostAsync(string title, FeaturedMod mod, string visibility, string mapName, string password = null, bool isRehost = false);
    }
}
