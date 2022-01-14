using System;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IOAuthService
    {
        public event EventHandler<EventArgs<OAuthStates>> Result;
        public Task<string> GetOAuthCode(string usernameOrEmail, string password);
        public Task<string> FetchOAuthToken(string code);
        public Task<string> RefreshOAuthToken(string refresh_token);
        public Task Auth();
        public Task Auth(string access_token);
        public Task Auth(string usernameOrEmail, string password);
        //public Task<string> GenerateUID(string session);
        public Task InitialLaunch();
    }

    public enum OAuthStates
    {
        EMPTY_FIELDS = -4,
        NO_CONNECTION = -3,
        NO_TOKEN = -2,
        INVALID = -1,
        TIMED_OUT = 0,
        AUTHORIZED = 1,
    }
}