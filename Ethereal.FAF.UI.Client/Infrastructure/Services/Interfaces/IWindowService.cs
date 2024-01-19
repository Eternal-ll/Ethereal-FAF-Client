using System;
using System.Windows;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IWindowService
    {
        public T Show<T>() where T : class;
        public T GetWindow<T>() where T : class;
    }
    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T GetWindow<T>() where T : class
        {
            if (!typeof(Window).IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException($"The window class should be derived from {typeof(Window)}.");

            var windowInstance = _serviceProvider.GetService(typeof(T)) as Window;

            if (windowInstance == null)
                throw new InvalidOperationException("Window is not registered as service.");

            return (T)Convert.ChangeType(windowInstance, typeof(T));
        }

        public T Show<T>() where T : class
        {
            if (!typeof(Window).IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException($"The window class should be derived from {typeof(Window)}.");

            var windowInstance = _serviceProvider.GetService(typeof(T)) as Window;

            if (windowInstance == null)
                throw new InvalidOperationException("Window is not registered as service.");

            windowInstance.Show();
            return (T)Convert.ChangeType(windowInstance, typeof(T));
        }
    }
}
