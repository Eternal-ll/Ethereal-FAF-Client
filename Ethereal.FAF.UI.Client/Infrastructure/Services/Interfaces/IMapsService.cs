using Ethereal.FAF.UI.Client.Models.Progress;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IMapsService
    {
        public Task EnsureMapExist(string map, IProgress<ProgressReport> progress = null, CancellationToken cancellationToken = default);
    }
}
