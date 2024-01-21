using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    /// <summary>
    /// System UID generator
    /// </summary>
    public interface IUIDService
    {
        /// <summary>
        /// Generate system UID
        /// </summary>
        /// <param name="session">Session code</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> GenerateAsync(string session, CancellationToken cancellationToken = default);
    }
}
