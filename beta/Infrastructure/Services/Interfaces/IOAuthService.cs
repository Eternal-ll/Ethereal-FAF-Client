using beta.Models;
using beta.Models.OAuth;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// OAuth2 service
    /// </summary>
    public interface IOAuthService
    {
        /// <summary>
        /// Service state
        /// </summary>
        public event EventHandler<OAuthEventArgs> StateChanged;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="refresh_token"></param>
        /// <param name="id_token"></param>
        /// <param name="expiresIn"></param>
        public void SetToken(string access_token, string refresh_token, string id_token, double expiresIn);
        /// <summary>
        /// OAuthorization using saved token bearer
        /// </summary>
        /// <param name="progress">Progress of process</param>
        /// <returns>OAuth token bearer <seealso cref="TokenBearer"/></returns>
        public Task<TokenBearer> AuthAsync(IProgress<string> progress = null);
        /// <summary>
        /// OAuthorization by Username or Login and Password
        /// </summary>
        /// <param name="usernameOrEmail">Username or login</param>
        /// <param name="password">Password</param>
        /// <param name="progress">Progress of process</param>
        /// <returns>OAuth token bearer <seealso cref="TokenBearer"/></returns>
        public Task<TokenBearer> AuthAsync(string usernameOrEmail, string password, CancellationToken? token = null, IProgress<string> progress = null);
        /// <summary>
        /// OAutharization by browser
        /// </summary>
        /// <param name="progress">Progress of process</param>
        /// <returns>OAuth token bearer <seealso cref="TokenBearer"/></returns>
        public Task<TokenBearer> AuthByBrowser(CancellationToken token, IProgress<string> progress = null);
    }
}