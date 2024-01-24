using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using FAF.UI.EtherealClient.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for NavigationView.xaml
    /// </summary>
    public partial class NavigationView : INavigableView<NavigationViewModel>
    {
        private readonly ISettingsManager _settingsManager;
        public NavigationView(
            NavigationViewModel vm,
            INavigationService navigationService,
            IPageService pageService,
            IServiceProvider serviceProvider,
            ISettingsManager settingsManager)
        {
            MainWindow mainWindow = null!;
            //if (serviceProvider.GetService<INavigationWindow>() is MainWindow window)
            //{
            //    mainWindow = window;
            //    window.Height = 800;
            //    window.MaxHeight = MainWindow.DefaultMaxHeight;
            //    window.MinHeight = 600;

            //    window.Width = 1000;
            //    window.MaxWidth = MainWindow.DefaultMaxWidth;
            //    window.MinWidth = 600;

            //    window.ResizeMode = ResizeMode.CanResize;

            //    window.TitleBar.Header = null;
            //    window.TitleBar.Title = "Ethereal FAF Client";
            //    window.TitleBar.ShowClose = true;
            //    window.TitleBar.ShowHelp = false;
            //    window.TitleBar.ShowMaximize = true;
            //    window.TitleBar.ShowMinimize = true;
            //    window.IsTitleBarDoubleClickAllowed = true;
            //}
            ViewModel = vm;
            InitializeComponent();
            navigationService.SetPageService(pageService);
            RootNavigationView.SetServiceProvider(serviceProvider);
            navigationService.SetNavigationControl(RootNavigationView);
            _settingsManager = settingsManager;
            //RootNavigationView.TitleBar = mainWindow.TitleBar;

            Loaded += NavigationView_Loaded;
            RootNavigationView.PaneOpened += RootNavigationView_PaneOpened;
            RootNavigationView.PaneClosed += RootNavigationView_PaneClosed;
        }

        private void RootNavigationView_PaneClosed(Wpf.Ui.Controls.NavigationView sender, RoutedEventArgs args)
        {
            _settingsManager.Settings.IsNavExpanded = false;
        }

        private void RootNavigationView_PaneOpened(Wpf.Ui.Controls.NavigationView sender, RoutedEventArgs args)
        {
            _settingsManager.Settings.IsNavExpanded = true;
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            RootNavigationView.IsPaneOpen = _settingsManager.Settings.IsNavExpanded;
        }

        public NavigationViewModel ViewModel { get; }
        public override ViewModel GetViewModel() => ViewModel;
    }
}
