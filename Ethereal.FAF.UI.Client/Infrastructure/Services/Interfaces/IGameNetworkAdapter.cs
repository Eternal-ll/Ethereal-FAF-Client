using Ethereal.FAF.UI.Client.Models.Progress;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Interface for Forged Alliance network adapter
    /// </summary>
    public interface IGameNetworkAdapter
    {
        /// <summary>
        /// Prepare adapter before run
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task PrepareAsync(
            IProgress<ProgressReport> progress = null,
            CancellationToken cancellationToken = default);
        /// <summary>
        /// Starts adapter and returns port for GPGnet
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="mode"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>GPGnet port</returns>
        public Task<int> Run(
            long gameId,
            string mode,
            IProgress<ProgressReport> progress = null,
            CancellationToken cancellationToken = default);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task Stop();
    }
}
