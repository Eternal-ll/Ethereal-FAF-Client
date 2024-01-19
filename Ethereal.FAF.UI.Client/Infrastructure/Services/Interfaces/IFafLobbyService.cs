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
    }
}
