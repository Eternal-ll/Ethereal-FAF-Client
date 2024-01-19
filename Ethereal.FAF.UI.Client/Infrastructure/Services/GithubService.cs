using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Octokit;
using System.Linq;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class GithubService : IGithubService
    {
        private readonly IGitHubClient _gitHubClient;
        private readonly string _repositoryOwner = GithubHelper.RepositoryOwner;
        private readonly string _repositoryName = GithubHelper.RepositoryName;

        public GithubService(IGitHubClient gitHubClient)
        {
            _gitHubClient = gitHubClient;
        }

        public async Task<Release> GetLatestRelease()
            => await _gitHubClient.Repository.Release.GetLatest(_repositoryOwner, _repositoryName);

        public async Task<Release> GetRelease(string version)
            => await _gitHubClient.Repository.Release.Get(_repositoryOwner, _repositoryName, version);
    }
}
