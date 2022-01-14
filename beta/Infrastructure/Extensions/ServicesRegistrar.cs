using beta.Infrastructure.Services;
using beta.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace beta.Infrastructure.Extensions
{
    public static class ServicesRegistrar
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services) => services
            .AddSingleton<IOAuthService, OAuthService>()
            .AddSingleton<ILobbySessionService, LobbySessionService>()
            .AddSingleton<IIRCService, IRCService>();
    }
}
