using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IFafAuthService
    {
        public int GetUserId();
        public string GetUserName();
        public bool IsAuthorized();
        public bool IsExpired(TimeSpan delay = default);
        public string GetAccessToken();
        public Task<TokenBearer> FetchTokenByCode(string code, string redirectUrl, CancellationToken cancellationToken = default);
        public Task<TokenBearer> RefreshToken(CancellationToken cancellationToken = default);
        public Task<string> GetActualAccessToken(CancellationToken cancellationToken = default);
    }
}
