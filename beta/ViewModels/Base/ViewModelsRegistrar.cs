using Microsoft.Extensions.DependencyInjection;

namespace beta.ViewModels.Base
{
    public static class ViewModelsRegistrar
    {
        public static void AddViewModels(this IServiceCollection services) => services
            .AddSingleton<AuthViewModel>()
        ;
    }
    
    public class ViewModelLocator
    {
        public AuthViewModel AuthViewModel => App.Services.GetRequiredService<AuthViewModel>();
    }
}
