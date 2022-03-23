using beta.Models;
using System;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IOAuthService
    {
        public event EventHandler<OAuthEventArgs> StateChanged;
        public void FetchOAuthToken(string code);
        public void RefreshOAuthToken(string refresh_token);
        public void Auth();
        public void Auth(string access_token);
        /// <summary>
        /// Аутентификация с помощью схемы OAuth2 с использованием логина (почты) и пароля
        /// </summary>
        /// <param name="usernameOrEmail"></param>
        /// <param name="password"></param>
        public void Auth(string usernameOrEmail, string password);
        //public void DoAuth(string usernameOrEmail, string password);
    }
}