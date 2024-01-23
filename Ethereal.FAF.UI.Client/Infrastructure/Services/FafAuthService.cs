using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    public class FafAuthService : IFafAuthService
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ClientManager _clientManager;

        public FafAuthService(ClientManager clientManager, IServiceProvider serviceProvider, ISettingsManager settingsManager)
        {
            _clientManager = clientManager;
            _serviceProvider = serviceProvider;
            _settingsManager = settingsManager;
        }
        private Server GetServer() => _clientManager.GetServer();
        private string GetClientId()
        {
            return GetServer()?.OAuth?.ClientId;
        }

        private TokenBearer GetTokenData()
        {
            var provider = GetServer()?.OAuth?.BaseAddress.Host ?? throw new InvalidOperationException("Server not selected");
            var savedToken = _settingsManager.Settings.AuthTokens.FirstOrDefault(x => x.AuthProvider == provider);
            if (savedToken != null)
            {
                var jwt = GetJwtSecurityToken(savedToken.AccessToken);
                return new()
                {
                    AccessToken = savedToken.AccessToken,
                    IdToken = savedToken.IdToken,
                    RefreshToken = savedToken.RefreshToken,
                    Created = jwt.Payload.ValidFrom,
                    ExpiresIn = (jwt.Payload.ValidTo - DateTime.Now).TotalSeconds
                };
            }
            return null;
        }
        private void SaveTokenData(TokenBearer token)
        {
            var server = GetServer();
            _settingsManager.Settings.AuthTokens.Remove(_settingsManager.Settings.AuthTokens.FirstOrDefault(x => x.AuthProvider == server.OAuth.BaseAddress.Host));
            _settingsManager.Settings.AuthTokens.Add(new()
            {
                AccessToken = token.AccessToken,
                IdToken = token.IdToken,
                RefreshToken = token.RefreshToken,
                AuthProvider = server.OAuth.BaseAddress.Host,
            });
            _settingsManager.Settings.Save();
        }

        public async Task<TokenBearer> FetchTokenByCode(string code, string redirectUrl, CancellationToken cancellationToken = default)
        {
            var token = await _serviceProvider.GetService<IAuthApi>().GetTokenByCode(new(code, GetClientId(), redirectUrl), cancellationToken);
            SaveTokenData(token);
            return token;
        }
        public async Task<TokenBearer> RefreshToken(CancellationToken cancellationToken = default)
        {
            var model = new IAuthApi.FetchTokenByRefreshToken(GetTokenData().RefreshToken, GetClientId());
            var token = await _serviceProvider.GetService<IAuthApi>().GetTokenByRefreshToken(model);
            SaveTokenData(token);
            return token;
        }
        public async Task<string> GetActualAccessToken(CancellationToken cancellationToken = default)
        {
            if (IsExpired(TimeSpan.FromMinutes(new Random().Next(15, 60))))
            {
                var token = await RefreshToken(cancellationToken);
                return token.AccessToken;
            }
            return GetAccessToken();
        }
        public bool IsAuthorized()
        {
            var server = GetServer();
            if (server == null) return false;
            return GetTokenData() != null;
        }
        public bool IsExpired(TimeSpan delay)
        {
            if (!IsAuthorized()) return false;
            var data = GetTokenData();
            var jwt = GetJwtSecurityToken(data.AccessToken);
            return jwt.Payload.ValidTo < DateTime.UtcNow.Add(delay);
        }
        public int GetUserId()
        {
            if (!IsAuthorized()) return 0;
            var data = GetTokenData();
            var jwt = GetJwtSecurityToken(data.IdToken);
            return int.Parse(jwt.Payload.Sub);
        }
        public string GetUserName()
        {
            if (!IsAuthorized()) return null;
            var data = GetTokenData();
            var jwt = GetJwtSecurityToken(data.IdToken);
            return jwt.Claims.First(x => x.Type == "username")?.Value;

        }
        public string GetAccessToken()
        {
            if (!IsAuthorized()) return null;
            return GetTokenData()?.AccessToken;
        }
        private JwtSecurityToken GetJwtSecurityToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(token);
        }
    }
}
