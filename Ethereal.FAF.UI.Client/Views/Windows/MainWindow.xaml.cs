// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.


using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;

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
        private readonly INavigationService _navigationService;
        private readonly ISettingsManager _settingsManager;

        public static double DefaultMaxHeight;
        public static double DefaultMaxWidth;

        public bool IsTitleBarDoubleClickAllowed;

        public MainWindow(
            INavigationService navigationService,
            ISnackbarService snackbarService,
            IContentDialogService contentDialogService,
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            IHost host,
            ContainerViewModel viewModel,
            ISettingsManager settingsManager,
            MainWindowViewModel mainWindowViewModel)
        {
            DefaultMaxHeight = MaxHeight;
            DefaultMaxWidth = MaxWidth;

            // Context provided by the service provider.
            DataContext = viewModel;

            Configuration = configuration;
            ServiceProvider = serviceProvider;
            _navigationService = navigationService;
            Host = host;

            // Initial preparation of the window.
            InitializeComponent();

            // If you want to use INavigationService instead of INavigationWindow you can define its navigation here.
            //navigationService.SetNavigationControl(navigationView.RootNavigation);
            // !! Experimental option
            //RemoveTitlebar();

            // !! Experimental option
            //ApplyBackdrop(Wpf.Ui.Appearance.BackgroundType.Mica);

            // We initialize a cute and pointless loading splash that prepares the view and navigate at the end.
            //Loaded += (_, _) =>
            //{
            //    Navigate(typeof(LoaderView));
            //};

            //MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            // We register a window in the Watcher class, which changes the application's theme if the system theme changes.
            //Wpf.Ui.Appearance.Watcher.Watch(this, Configuration.GetValue<BackgroundType>("UI:BackgroundType"), true, false);
            snackbarService.SetSnackbarPresenter(SnackbarPresenter);
            contentDialogService.SetContentPresenter(RootContentDialog);
            _settingsManager = settingsManager;

            //Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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

        public INavigationView GetNavigation() => _navigationService.GetNavigationControl();

        public bool Navigate(Type pageType) => RootFrame.Navigate(ServiceProvider.GetRequiredService(pageType));

        public void SetPageService(IPageService pageService) => throw new NotImplementedException();

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        private void TrayMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            //if (sender is not MenuItem menuItem)
            //    return;
        }

        /// <summary>
        /// Ignore maximize on title double click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBar_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsTitleBarDoubleClickAllowed) e.Handled = true;
        }
        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}

