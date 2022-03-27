using beta.Models;
using System;
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
        
        //public void FetchOAuthToken(string code);
        
        public Task<bool> RefreshOAuthTokenAsync(string refresh_token);
        
        public Task AuthAsync();
        
        /// <summary>
        /// Аутентификация с помощью схемы OAuth2 с использованием логина (почты) и пароля
        /// </summary>
        /// <param name="usernameOrEmail"></param>
        /// <param name="password"></param>
        public Task AuthAsync(string usernameOrEmail, string password);
        //public void DoAuth(string usernameOrEmail, string password);
    }
}