using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IImageCacheService
    {
        public Task<string> EnsureCachedAsync(string url, CancellationToken cancellationToken);
    }
}
