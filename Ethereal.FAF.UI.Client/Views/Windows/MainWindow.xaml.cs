// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.


using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace FAF.UI.EtherealClient.Views.Windows
{
    /// <summary>
    /// Interaction logic for Container.xaml
    /// </summary>
    public sealed partial class MainWindow : INavigationWindow
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly IConfiguration Configuration;
        private readonly IHost Host;

        private readonly NavigationView NavigationView;

        public MainWindow(
            INavigationService navigationService,
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            IHost host,
            NavigationView navigationView,
            ContainerViewModel viewModel)
        {

            // Context provided by the service provider.
            DataContext = viewModel;

            Configuration = configuration;
            NavigationView = navigationView;
            ServiceProvider = serviceProvider;
            Host = host;

            // Initial preparation of the window.
            InitializeComponent();

            // If you want to use INavigationService instead of INavigationWindow you can define its navigation here.
            navigationService.SetNavigationControl(navigationView.RootNavigation);

            // !! Experimental option
            //RemoveTitlebar();

            // !! Experimental option
            //ApplyBackdrop(Wpf.Ui.Appearance.BackgroundType.Mica);

            // We initialize a cute and pointless loading splash that prepares the view and navigate at the end.
            //Loaded += (_, _) =>
            //{
            //    Navigate(typeof(LoaderView));
            //};

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            // We register a window in the Watcher class, which changes the application's theme if the system theme changes.
            //Wpf.Ui.Appearance.Watcher.Watch(this, Configuration.GetValue<BackgroundType>("UI:BackgroundType"), true, false);
        }

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override async void OnClosed(EventArgs e)
        {
            await Host.StopAsync();
            base.OnClosed(e);
        }

        #region INavigationWindow methods

        public Frame GetFrame() => RootFrame;

        public INavigation GetNavigation() => NavigationView.RootNavigation;

        public bool Navigate(Type pageType) => RootFrame.Navigate(ServiceProvider.GetRequiredService(pageType));

        public void SetPageService(IPageService pageService) 
            => NavigationView.RootNavigation.PageService = pageService;

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        private void TrayMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;
        }

        private void RootDialog_OnButtonRightClick(object sender, RoutedEventArgs e)
        {
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            RootDialog.Hide();
        }
    }
}

