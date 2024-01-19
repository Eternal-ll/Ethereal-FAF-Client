using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class TokenReloadService : BackgroundService
    {
        private readonly ILogger Logger;
        private readonly IServiceProvider ServiceProvider;
        private readonly IFafAuthService _fafAuthService;

        public TokenReloadService(ILogger<TokenReloadService> logger, IServiceProvider serviceProvider, IFafAuthService fafAuthService)
        {
            Logger = logger;
            ServiceProvider = serviceProvider;
            _fafAuthService = fafAuthService;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return Handle(cancellationToken);
        }
        private async Task Handle(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            //var token = TokenProvider.Token;
            //if (token is not null && token.AccessToken is not null && token.RefreshToken is not null)
            //{
            //    if (!token.IsExpired())
            //    {
            //        var delay = token.ExpiresAt - DateTimeOffset.Now - TimeSpan.FromMinutes(4);
            //        await Task.Delay(delay, cancellationToken);
            //    }
            //}
            //else
            //{
            //    await Task.Delay(TimeSpan.FromMinutes(55), cancellationToken);
            //}
            //while (!cancellationToken.IsCancellationRequested)
            //{
            //    await RefreshToken(cancellationToken);
            //    await Task.Delay(TimeSpan.FromMinutes(55), cancellationToken);
            //}
        }

        private async Task RefreshToken(CancellationToken cancellationToken)
        {
            //Logger.LogTrace("Initializing token refreshing process");
            //var token = TokenProvider.Token;
            //if (token is null || token.RefreshToken is null)
            //{
            //    Logger.LogTrace("Token is not installed");
            //    return;
            //}
            //using var scope = ServiceProvider.CreateScope();
            //Logger.LogTrace("Current access token expires in [{seconds}]", token.ExpiresIn);
            //var oauthClient = scope.ServiceProvider.GetRequiredService<FafOAuthClient>();
            //var result = await oauthClient.RefreshToken(token.RefreshToken, cancellationToken);
            //if (result.IsError)
            //{
            //    Logger.LogError("Error on reloading access token [{error}]", result.ErrorDescription);
            //    return;
            //}
            //token = result.TokenBearer;
            //TokenProvider.Save(token);
            //Logger.LogTrace("New access token expires in [{seconds}] seconds", token.ExpiresIn);
        }
    }
}