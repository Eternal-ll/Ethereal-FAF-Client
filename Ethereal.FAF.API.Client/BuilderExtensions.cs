using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Ethereal.FAF.API.Client
{
    public static partial class BuilderExtensions
    {

        private static void Configure(HttpClient c) => c.BaseAddress = new Uri("https://api.faforever.com/");

        public static IServiceCollection AddFafApi(this IServiceCollection services) => services
            .AddTransient<AuthHeaderHandler>()
            .AddTransient<VerifyHeaderHandler>()

            .AddRefitClient<IFeaturedFilesClient>()
            .ConfigureHttpClient(Configure)
            .AddHttpMessageHandler<VerifyHeaderHandler>()

            .Services;
    }
}
