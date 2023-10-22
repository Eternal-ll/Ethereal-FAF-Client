using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
	/// <summary>
	/// Interface to work with file caching
	/// </summary>
	public interface IFileCacheService
	{
		//public Task<string> IsCached(string url, CancellationToken cancellationToken);
		public Task<string> Cache(string url, CancellationToken cancellationToken = default);
		//public string GetFromCache(string url, CancellationToken cancellationToken);
	}
}
