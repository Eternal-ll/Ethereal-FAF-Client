using Octokit;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IGithubService
    {
        public Task<Release> GetLatestRelease();
        public Task<Release> GetRelease(string version);
    }
}
