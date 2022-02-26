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
            //.AddSingleton<IIrcService, IrcService>()
            .AddSingleton<IAvatarService, AvatarService>()
            .AddSingleton<IMapsService, MapsService>()
            .AddSingleton<ISocialService, SocialService>()
            .AddSingleton<ICacheService, CacheService>()
            ;
    }
}
