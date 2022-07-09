using System;
using System.Windows;

namespace beta.Infrastructure.Services
{
    public class WindowService : Interfaces.IWindowService
    {
        private readonly IServiceProvider ServiceProvider;

        public WindowService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public void Show(Type windowType)
        {
            if (!typeof(Window).IsAssignableFrom(windowType))
                throw new InvalidOperationException($"The window class should be derived from {typeof(Window)}.");

            var windowInstance = ServiceProvider.GetService(windowType) as Window;

            windowInstance?.Show();
        }

        public T Show<T>() where T : class
        {
            if (!typeof(Window).IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException($"The window class should be derived from {typeof(Window)}.");

            var windowInstance = ServiceProvider.GetService(typeof(T)) as Window;

            if (windowInstance == null)
                throw new InvalidOperationException("Window is not registered as service.");

            windowInstance.Show();

            return (T)Convert.ChangeType(windowInstance, typeof(T));
        }
    }
}

