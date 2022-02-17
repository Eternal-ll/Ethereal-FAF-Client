using beta.Infrastructure.Services;
using beta.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace beta.Infrastructure.Extensions
{
    public static class ServicesRegistrar
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services) => services
            .AddSingleton<IOAuthService, OAuthService>()
            .AddSingleton<ISessionService, SessionService>()
            .AddSingleton<IPlayersService, PlayersService>()
            .AddSingleton<IGamesServices, GamesServices>()
            .AddSingleton<IIRCService, IRCService>()
            .AddSingleton<IAvatarService, AvatarService>()
            .AddSingleton<IMapService, MapService>()
            ;
    }
}
