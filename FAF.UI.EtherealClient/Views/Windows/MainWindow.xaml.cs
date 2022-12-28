﻿// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using FAF.UI.EtherealClient.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.TaskBar;

namespace FAF.UI.EtherealClient.Views.Windows
{
    /// <summary>
    /// Interaction logic for Container.xaml
    /// </summary>
    public partial class MainnWindow : INavigationWindow
    {
        private bool _initialized = false;

        private readonly IThemeService _themeService;

        private readonly ITaskBarService _taskBarService;

        // NOTICE: In the case of this window, we navigate to the Dashboard after loading with Container.InitializeUi()

        public MainnWindow(ContainerViewModel viewModel, INavigationService navigationService, IPageService pageService, IThemeService themeService, ITaskBarService taskBarService)
        {
            // Attach the theme service
            _themeService = themeService;

            // Attach the taskbar service
            _taskBarService = taskBarService;

            // Context provided by the service provider.
            DataContext = viewModel;

            // Initial preparation of the window.
            InitializeComponent();

            // We define a page provider for navigation
            SetPageService(pageService);

            // If you want to use INavigationService instead of INavigationWindow you can define its navigation here.
            navigationService.SetNavigation(RootNavigation);

            // !! Experimental option
            //RemoveTitlebar();

            // !! Experimental option
            //ApplyBackdrop(Wpf.Ui.Appearance.BackgroundType.Mica);

            // We initialize a cute and pointless loading splash that prepares the view and navigate at the end.
            Loaded += (_, _) => InvokeSplashScreen();

            // We register a window in the Watcher class, which changes the application's theme if the system theme changes.
            // Wpf.Ui.Appearance.Watcher.Watch(this, Appearance.BackgroundType.Mica, true, false);
        }

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        #region INavigationWindow methods

        public Frame GetFrame()
            => RootFrame;

        public INavigation GetNavigation()
            => RootNavigation;

        public bool Navigate(Type pageType)
            => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService)
            => RootNavigation.PageService = pageService;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        #endregion INavigationWindow methods

        private void InvokeSplashScreen()
        {
            if (_initialized)
                return;
            _initialized = true;

            RootMainGrid.Visibility = Visibility.Collapsed;
            TitleBar.Visibility = Visibility.Collapsed;
            RootWelcomeGrid.Visibility = Visibility.Visible;

            _taskBarService.SetState(this, TaskBarProgressState.Indeterminate);

            Task.Run(async () =>
            {
                // Remember to always include Delays and Sleeps in
                // your applications to be able to charge the client for optimizations later.
                await Task.Delay(4000);

                Dispatcher.Invoke(() =>
                {
                    RootWelcomeGrid.Visibility = Visibility.Hidden;
                    RootMainGrid.Visibility = Visibility.Visible;
                    TitleBar.Visibility = Visibility.Visible;
                    //RenderSize = new(1280, 720);
                    Width = 1280;
                    Height = 720;
                    //Navigate(typeof(.Dashboard));

                    _taskBarService.SetState(this, TaskBarProgressState.None);
                }, System.Windows.Threading.DispatcherPriority.Send);

                return true;
            });
        }

        private void NavigationButtonTheme_OnClick(object sender, RoutedEventArgs e)
        {
            _themeService.SetTheme(_themeService.GetTheme() == ThemeType.Dark ? ThemeType.Light : ThemeType.Dark);
        }

        private void TrayMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            System.Diagnostics.Debug.WriteLine($"DEBUG | WPF UI Tray clicked: {menuItem.Tag}", "Wpf.Ui.Demo");
        }

        private void RootNavigation_OnNavigated(INavigation sender, RoutedNavigationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG | WPF UI Navigated to: {sender?.Current ?? null}", "Wpf.Ui.Demo");

            // This funky solution allows us to impose a negative
            // margin for Frame only for the Dashboard page, thanks
            // to which the banner will cover the entire page nicely.
            RootFrame.Margin = new Thickness(
                left: 0,
                top: sender?.Current?.PageTag == "dashboard" ? -69 : 0,
                right: 0,
                bottom: 0);
        }

        private void RootDialog_OnButtonRightClick(object sender, RoutedEventArgs e)
        {
            RootDialog.Hide();
        }
    }
}

