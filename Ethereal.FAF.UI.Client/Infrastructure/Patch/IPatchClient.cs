using Ethereal.FAF.UI.Client.Models.Progress;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Patch
{
    /// <summary>
    /// Game patch client
    /// </summary>
    public interface IPatchClient
    {
        public Task EnsurePatchExist(string mod, string root, int version = 0, bool forceCheck = false,
            IProgress<ProgressReport> progress = null,
            CancellationToken cancellationToken = default);
    }
}
