using beta.Models;
using System;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IOAuthService
    {
        public event EventHandler<OAuthEventArgs> StateChanged;
        public void FetchOAuthToken(string code);
        public void RefreshOAuthToken(string refresh_token);
        public void Auth();
        public void Auth(string access_token);
        public void Auth(string usernameOrEmail, string password);
    }
}