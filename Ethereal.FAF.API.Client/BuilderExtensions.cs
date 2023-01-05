using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Ethereal.FAF.API.Client
{
    public static partial class BuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="api"></param>
        /// <param name="content"></param>
        /// <returns>Adds <see cref="IFafApiClient"/> and <see cref="IFafContentClient"/></returns>
        public static IServiceCollection AddFafApi(this IServiceCollection services, Uri api, Uri content) => services
            .AddTransient<AuthHeaderHandler>()
            .AddTransient<VerifyHeaderHandler>()
            .AddRefitClient<IFafApiClient>()
            .ConfigureHttpClient(c => c.BaseAddress = api)
            .Services
            .AddRefitClient<IFafContentClient>()
            .ConfigureHttpClient(c => c.BaseAddress = content)
            .Services;
    }
}
