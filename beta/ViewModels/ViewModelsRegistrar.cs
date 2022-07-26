using Microsoft.Extensions.DependencyInjection;

namespace beta.ViewModels
{
    public static class ViewModelsRegistrar
    {
        public static IServiceCollection RegisterViewModel(this IServiceCollection services) => services
            .AddScoped<MainViewModel>()
            .AddSingleton<SettingsViewModel>()
            .AddSingleton<IrcViewModel>()
            .AddSingleton<HostGameViewModel>()
            .AddScoped<NewsViewModel>()
            .AddSingleton<ChatViewModel>();
    }
}
