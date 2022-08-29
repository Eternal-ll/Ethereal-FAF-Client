using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class TokenReloadService : IHostedService
    {
        private readonly ILogger Logger;
        private readonly IServiceProvider ServiceProvider;
        private readonly CancellationTokenSource CancellationTokenSource;
        private readonly TokenProvider TokenProvider;

        public TokenReloadService(ILogger<TokenReloadService> logger, IServiceProvider serviceProvider, TokenProvider tokenProvider)
        {
            Logger = logger;
            Logger.LogTrace("Initialized");
            ServiceProvider = serviceProvider;
            CancellationTokenSource = new();
            TokenProvider = tokenProvider;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var token = TokenProvider.TokenBearer;
            if (token is not null && token.AccessToken is not null && token.RefreshToken is not null)
            {
                if (!token.IsExpired)
                {
                    var delay = token.ExpiresAt - DateTimeOffset.Now - TimeSpan.FromMinutes(4);
                    await Task.Delay(delay, cancellationToken);
                }
            }
            else
            {
                await Task.Delay(55000);
            }
            while (!cancellationToken.IsCancellationRequested)
            {
                await RefreshToken(cancellationToken);
                await Task.Delay(55000, cancellationToken);
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
            Logger.LogTrace("Current access token expires in [{}] seconds at [{}]", token.ExpiresIn, token.ExpiresAt);
            var oauthClient = scope.ServiceProvider.GetRequiredService<FafOAuthClient>();
            var result = await oauthClient.RefreshToken(token.RefreshToken, cancellationToken);
            if (result.IsError)
            {
                Logger.LogError("Error on reloading access token [{}]", result.ErrorDescription);
                return;
            }
            token = result.TokenBearer;
            Logger.LogTrace("New access token expires in [{}] seconds at [{}]", token.ExpiresIn, token.ExpiresAt);
            TokenProvider.Save(token);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Finished");
            return Task.CompletedTask;
        }
    }
}