using Microsoft.Extensions.DependencyInjection;

namespace beta.Views
{
    public static class ViewsRegistrar
    {
        public static IServiceCollection RegisterViews(this IServiceCollection services) => services
            .AddScoped<CustomGamesView>()

            .AddSingleton<HostGameView>()
            .AddTransient<MapsView>()

            .AddScoped<SettingsView>()

            .AddTransient<AuthorizationView>()
            .AddTransient<LogoutView>()
            .AddTransient<DonationView>()
            .AddTransient<AboutView>()

            .AddScoped<PlayModeSelectView>()
            .AddTransient<WebViews.WebViewControl>()
            .AddTransient<NewsView>()
            .AddTransient<UnitsDatabasesView>()
            .AddTransient<ChatView>()
            .AddTransient<VaultView>()
            ;
    }
}
