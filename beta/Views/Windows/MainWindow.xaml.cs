using beta.Infrastructure.Navigation;
using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf;
using ModernWpf.Controls;
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
            //GameLauncherService.PatchUpdateRequired += OnPatchUpdateRequired;
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
    }
}
