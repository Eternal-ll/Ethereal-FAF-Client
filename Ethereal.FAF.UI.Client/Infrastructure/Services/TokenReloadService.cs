using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    public class ServerOauthTokenProvider
    {
        public event EventHandler<(Server server, string error, string description)> ErrorOccuried;
        public event EventHandler<(Server server, string OAuthUrl)> OAuthRequired;
        public event EventHandler<Server> OAuthUrlExpired;

        private readonly FafOAuthClient FafOAuthClient;

        private Server Server;
        private TokenBearer Token;

        public ServerOauthTokenProvider(FafOAuthClient fafOAuthClient)
        {
            FafOAuthClient = fafOAuthClient;
            FafOAuthClient.OAuthLinkGenerated += (s, link) => OAuthRequired?.Invoke(this, (Server, link));
        }

        public void SetServer(Server server)
        {
            Server = server;
            FafOAuthClient.Initialize(server.OAuth.ClientId, server.OAuth.Scope, server.OAuth.RedirectPorts, server.OAuth.BaseAddress);
            if (server.OAuth.Token is not null && server.OAuth.Token.AccessToken is not null && server.OAuth.Token.RefreshToken is not null)
            {
                Token = server.OAuth.Token;
            };
        }
        public Server GetServer()
        {
            return Server;
        }

        public async Task<TokenBearer> GetTokenAsync(CancellationToken cancellationToken)
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
                    ErrorOccuried?.Invoke(this, (Server, response.Error, response.ErrorDescription));
                    throw new Exception();
                }
                Token = response.TokenBearer;
                UserSettings.Update($"Servers:{Server.ShortName}:OAuth:Token", new Dictionary<string, object>()
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
                OAuthUrlExpired?.Invoke(this, Server);
            }
            catch (Exception ex)
            {
                await Task.Delay(5000, cancellationToken);
            }
            if (Token is not null)
            {
                Token = null;
            }
            return await GetTokenAsync(cancellationToken);
        }
    }
    internal class TokenReloadService : BackgroundService
    {
        private readonly ILogger Logger;
        private readonly IServiceProvider ServiceProvider;
        private readonly CancellationTokenSource CancellationTokenSource;
        private readonly TokenProvider TokenProvider;

        public TokenReloadService(ILogger<TokenReloadService> logger, IServiceProvider serviceProvider, ITokenProvider tokenProvider)
        {
            Logger = logger;
            Logger.LogTrace("Initialized");
            ServiceProvider = serviceProvider;
            CancellationTokenSource = new();
            TokenProvider = (TokenProvider)tokenProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            var token = TokenProvider.TokenBearer;
            if (token is not null && token.AccessToken is not null && token.RefreshToken is not null)
            {
                if (!token.IsExpired())
                {
                    var delay = token.ExpiresAt - DateTimeOffset.Now - TimeSpan.FromMinutes(4);
                    await Task.Delay(delay, cancellationToken);
                }
            }
            else
            {
                await Task.Delay(TimeSpan.FromMinutes(55), cancellationToken);
            }
            while (!cancellationToken.IsCancellationRequested)
            {
                await RefreshToken(cancellationToken);
                await Task.Delay(TimeSpan.FromMinutes(55), cancellationToken);
            }
        }

        private async Task RefreshToken(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Initializing token refreshing process");
            var token = TokenProvider.TokenBearer;
            if (token is null || token.RefreshToken is null)
            {
                Logger.LogTrace("Token is not installed");
                return;
            }
            using var scope = ServiceProvider.CreateScope();
            Logger.LogTrace("Current access token expires in [{seconds}]", token.ExpiresIn);
            var oauthClient = scope.ServiceProvider.GetRequiredService<FafOAuthClient>();
            var result = await oauthClient.RefreshToken(token.RefreshToken, cancellationToken);
            if (result.IsError)
            {
                Logger.LogError("Error on reloading access token [{error}]", result.ErrorDescription);
                return;
            }
            token = result.TokenBearer;
            //TokenProvider.Save(token);
            Logger.LogTrace("New access token expires in [{seconds}] seconds", token.ExpiresIn);
        }
    }
}