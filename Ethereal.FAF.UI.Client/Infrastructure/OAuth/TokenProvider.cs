using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.OAuth
{
    public sealed class TokenProvider : ITokenProvider
    {
        public event EventHandler<(string error, string description)> ErrorOccuried;
        public event EventHandler<string> OAuthRequired;
        public event EventHandler OAuthUrlExpired;

        private readonly FafOAuthClient FafOAuthClient;
        private readonly Server Server;

        public JwtSecurityToken JwtSecurityToken { get; private set; }

        private TokenBearer _Token;
        public TokenBearer Token
        {
            get => _Token;
            set
            {
                _Token = value;
                if (value is not null)
                {
                    ProcessJwtToken(value);
                }
            }
        }

        public TokenProvider(FafOAuthClient fafOAuthClient, Server server)
        {
            FafOAuthClient = fafOAuthClient;
            Server = server;
            if (server.OAuth.Token is not null && server.OAuth.Token.AccessToken is not null && server.OAuth.Token.RefreshToken is not null)
            {
                Token = server.OAuth.Token;
            };
            FafOAuthClient.OAuthLinkGenerated += (s, link) => OAuthRequired?.Invoke(this, link);
        }

        private void ProcessJwtToken(TokenBearer token)
        {
            if (token is null) return;
            if (token.AccessToken is null)
            {
                return;
            }
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadToken(token.AccessToken) as JwtSecurityToken;
            JwtSecurityToken = jwtSecurityToken;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var token = await GetTokenBearerAsync(cancellationToken);
            return token.AccessToken;
        }
        public async Task<TokenBearer> GetTokenBearerAsync(CancellationToken cancellationToken = default)
        {
            if (Token is not null && !Token.IsExpired())
            {
                return Token;
            }

            var task = Token is not null ?
                FafOAuthClient.RefreshToken(Token.RefreshToken, cancellationToken) :
                FafOAuthClient.AuthByBrowser(cancellationToken);
            try
            {
                var response = await task.WaitAsync(TimeSpan.FromSeconds(Server.OAuth.ResponseSeconds), cancellationToken);
                if (response.IsError)
                {
                    ErrorOccuried?.Invoke(this, (response.Error, response.ErrorDescription));
                    throw new Exception();
                }
                Token = response.TokenBearer;
                UserSettings.Update($"Server:OAuth:Token", new Dictionary<string, object>()
                {
                    { "AccessToken", Token.AccessToken },
                    { "RefreshToken", Token.RefreshToken },
                    { "IdToken", Token.IdToken },
                    { "TokenType", Token.TokenType },
                    { "Scope", Token.Scope },
                    { "Created", Token.Created },
                    { "ExpiresIn", Token.ExpiresIn },
                });
                return response.TokenBearer;
            }
            catch (OperationCanceledException canceled)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
                OAuthUrlExpired?.Invoke(this, null);
            }
            catch (Exception ex)
            {
                await Task.Delay(5000, cancellationToken);
            }
            if (Token is not null)
            {
                Token = null;
            }
            return await GetTokenBearerAsync(cancellationToken);
        }
    }
}
