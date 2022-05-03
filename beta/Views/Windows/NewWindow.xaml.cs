using beta.Properties;
using System.Windows;

namespace beta.Views.Windows
{
    /// <summary>
    /// Interaction logic for NewWindow.xaml
    /// </summary>
    public partial class NewWindow : Window
    {
        public NewWindow()
        {
            InitializeComponent();

            Top = Settings.Default.Top;
            Left = Settings.Default.Left;
            Height = Settings.Default.Height;
            Width = Settings.Default.Width;

            if (Settings.Default.IsWindowMaximized)
            {
                WindowState = WindowState.Maximized;
            }

            Closing += OnWindowClosing;
        }
        public bool ShutdownApplication { get; set; } = true;

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Settings.Default.Top = RestoreBounds.Top;
                Settings.Default.Left = RestoreBounds.Left;
                Settings.Default.Height = RestoreBounds.Height;
                Settings.Default.Width = RestoreBounds.Width;
                Settings.Default.IsWindowMaximized = true;
            }
            else
            {
                Settings.Default.Top = Top;
                Settings.Default.Left = Left;
                Settings.Default.Height = Height;
                Settings.Default.Width = Width;
                Settings.Default.IsWindowMaximized = false;
            }
            if (!ShutdownApplication) return;
            //App.Current.Shutdown();
        }
    }
}
