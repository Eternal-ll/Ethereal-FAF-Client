using Ethereal.FAF.UI.Client.Models.Progress;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IJavaRuntime
    {
        public Task<string> EnsurJavaRuntimeExist(
            IProgress<ProgressReport> progress = null,
            CancellationToken cancellationToken = default);
    }
}
