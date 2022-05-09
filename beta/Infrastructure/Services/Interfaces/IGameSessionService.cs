using beta.Models.Server;
using beta.Models.Server.Enums;
using System;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public enum GameSessionState : byte
    {
        /// <summary>
        /// Game services are entirely off
        /// </summary>
        OFF,
        /// <summary>
        /// We're listening on the game port
        /// </summary>
        LISTENING,
        /// <summary>
        /// We've verified our connectivity and are idle
        /// </summary>
        IDLE = 2,
        /// <summary>
        /// Game has been launched but we're not connected yet
        /// </summary>
        LAUNCHED = 3,
        /// <summary>
        /// Game is running and we're relaying gpgnet commands
        /// </summary>
        RUNNING = 4
    }
    internal interface IGameSessionService
    {
        public event EventHandler<GameInfoMessage> GameFilled;

        public GameSessionState State { get; }

        public bool GameIsRunning { get; }

        public Task Close();

        public Task JoinGame(GameInfoMessage game);
        public Task WatchGame(long replayId, string mapName, int playerId, FeaturedMod featuredMod, bool isLive = true);

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
        public Task HostGame(string title, FeaturedMod mod, string mapName, double? minRating, double? maxRating,
            GameVisibility visibility = GameVisibility.Public, bool isRatingResctEnforced = false, string password = null, bool isRehost = false);

        public Task ResetPatch();
    }
}
