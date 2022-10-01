using beta.Models.Debugger;
using beta.Views;
using beta.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace beta.Infrastructure.Services
{
    public class ApplicationHostService : IHostedService
    {
        private readonly IServiceProvider ServiceProvider;

        private Window MainWindow;

        public ApplicationHostService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            PrepareNavigation();
            return HandleActivationAsync();
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
        private async Task HandleActivationAsync()
        {
            await Task.CompletedTask;

            if (Application.Current.Windows.Count == 0)
            {
                AppDebugger.Init();
                MainWindow = ServiceProvider.GetService<MainWindow>();
                MainWindow!.Show();

                // NOTICE: You can set this service directly in the window 
                // _navigationWindow.SetPageService(_pageService);

                // NOTICE: In the case of this window, we navigate to the Dashboard after loading with Container.InitializeUi()
                // _navigationWindow.Navigate(typeof(Views.Pages.Dashboard));
            }

            //var notifyIcon = _serviceProvider.GetService(typeof(INotifyIconService)) as INotifyIconService;

            //if (!notifyIcon!.IsRegistered)
            //{
            //    notifyIcon!.SetParentWindow(_navigationWindow as Window);
            //    notifyIcon.Register();
            //}

            await Task.CompletedTask;
        }

        private void PrepareNavigation()
        {

        }
    }
    public class NavigationService
    {
        private readonly ViewService ViewService;
        private ModernWpf.Controls.NavigationView NavigationView;
        
        public NavigationService(ViewService viewService)
        {
            ViewService = viewService;
        }

        private Frame Frame;
        public void SetFrame(Frame frame) => Frame = frame;
        public void Navigate(Type viewType)
        {
            var view = ViewService.GetView(viewType);
            Frame?.Navigate(view);
        }
        public void Navigate(Uri url)
        {
            if (Frame.Content is Views.WebViews.WebViewControl webView)
            {
                webView.Navigate(url);
                return;
            }
            var view = ViewService.GetView(typeof(Views.WebViews.WebViewControl)) as Views.WebViews.WebViewControl;
            view.Navigate(url);
            Frame?.Navigate(view);
        }
        public bool CanGoBack() => Frame.CanGoBack;
        public void GoBack() => Frame.GoBack();
        public bool CanGoForward() => Frame.CanGoForward;
        public void GoForward() => Frame.GoForward();
        public void Focus() => Frame.Focus();
        public void SetNavigationView(ModernWpf.Controls.NavigationView nav) =>
            NavigationView = nav;
    }
    public class ViewService
    {
        /// <summary>
        /// Service which provides the instances of pages.
        /// </summary>
        private readonly IServiceProvider ServiceProvider;

        /// <summary>
        /// Creates new instance and attaches the <see cref="IServiceProvider"/>.
        /// </summary>
        public ViewService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public T GetView<T>() where T : UserControl
        {
            return (T)ServiceProvider.GetService(typeof(T));
        }

        /// <inheritdoc />
        public FrameworkElement GetView(Type pageType)
        {
            return ServiceProvider.GetService(pageType) as FrameworkElement;
        }
    }
}
