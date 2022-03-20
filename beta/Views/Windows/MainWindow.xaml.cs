using beta.Infrastructure.Navigation;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Properties;
using beta.Views.Modals;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf;
using ModernWpf.Controls;
using System;
using System.Windows;
using System.Windows.Threading;

namespace beta.Views.Windows
{

    public partial class MainWindow : Window
    {
        private readonly ISessionService SessionService;
        private readonly IGameLauncherService GameLauncherService;

        private ContentDialog Dialog;
        public MainWindow()
        {
            InitializeComponent();

            SessionService = App.Services.GetService<ISessionService>();
            GameLauncherService = App.Services.GetService<IGameLauncherService>();


            NavigationManager navManager = new(MainFrame, ModalFrame);
            navManager.Navigate(new AuthView());

            Dialog = new ContentDialog();

            Dialog.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    e.Handled = true;
                }
            };

            Left = Settings.Default.Left;
            Top = Settings.Default.Top;

            Width = Settings.Default.Width;
            Height = Settings.Default.Height;

            Closing += OnMainWindowClosing;

            //SessionService.TcpClient.StateChanged += SessionService_StateChanged;
            GameLauncherService.PatchUpdateRequired += OnPatchUpdateRequired;
        }

        private void OnPatchUpdateRequired(object sender, Infrastructure.EventArgs<ViewModels.TestDownloaderVM> e)
        {
            Dialog.Dispatcher.Invoke(() =>
            {
                // TODO write new downloader model
                try
                {
                    var view = new PatchUpdateView(e);
                    Dialog.Content = view;
                    Dialog.ShowAsync();
                    view.Model.DownloadFinished += (s, e) =>
                    {
                        view = null;
                        Dialog.Content = null;
                        Dialog.Hide();
                        GameLauncherService.JoinGame();
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }

        private void SessionService_StateChanged(object sender, ManagedTcpClientState e)
        {
            if (e == ManagedTcpClientState.Disconnected)
            {
                Dialog.ShowAsync();
                SessionService.Authorize();
            }

            if (e == ManagedTcpClientState.Connected)
            {
                if (Dialog is not null)
                    Dialog.Hide();
            }
        }

        private void OnMainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Left = Left;
            Settings.Default.Top = Top;

            Settings.Default.Width = Width;
            Settings.Default.Height = Height;

            Closing -= OnMainWindowClosing;
            Application.Current.Shutdown();
        }

        private void ToggleAppThemeHandler(object sender, RoutedEventArgs e)
        {
            ClearValue(ThemeManager.RequestedThemeProperty);

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                var tm = ThemeManager.Current;
                if (tm.ActualApplicationTheme == ApplicationTheme.Dark)
                {
                    tm.ApplicationTheme = ApplicationTheme.Light;
                }
                else
                {
                    tm.ApplicationTheme = ApplicationTheme.Dark;
                }
            });
        }

        private void ToggleWindowThemeHandler(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.GetActualTheme(this) == ElementTheme.Light)
            {
                ThemeManager.SetRequestedTheme(this, ElementTheme.Dark);
            }
            else
            {
                ThemeManager.SetRequestedTheme(this, ElementTheme.Light);
            }
        }
    }
}
