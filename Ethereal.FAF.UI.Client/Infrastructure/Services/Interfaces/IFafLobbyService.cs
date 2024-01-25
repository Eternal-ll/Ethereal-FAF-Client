using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IFafLobbyService
    {
        public bool Connected { get; }
        /// <summary>
        /// Connect to lobby
        /// </summary>
        /// <returns></returns>
        public Task ConnectAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Disconnect from lobby
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task DisconnectAsync(CancellationToken cancellationToken = default);
        /// <summary>
        /// Join game
        /// </summary>
        /// <param name="uid">Lobby UID</param>
        /// <returns></returns>
        public Task JoinGameAsync(long uid, int port = 0);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task GameEndedAsync();
    }
}
