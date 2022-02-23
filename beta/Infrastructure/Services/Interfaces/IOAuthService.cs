using System;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IOAuthService
    {
        public event EventHandler<EventArgs<OAuthState>> Result;
        public void FetchOAuthToken(string code);
        public void RefreshOAuthToken(string refresh_token);
        public void Auth();
        public void Auth(string access_token);
        public void Auth(string usernameOrEmail, string password);
    }

    public enum OAuthState
    {
        EMPTY_FIELDS = -4,
        NO_CONNECTION = -3,
        NO_TOKEN = -2,
        INVALID = -1,
        TIMED_OUT = 0,
        AUTHORIZED = 1,
    }
}