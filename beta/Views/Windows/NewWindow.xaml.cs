using beta.Properties;
using ModernWpf.Controls;
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
            Loaded += OnWindowLoaded;
            Closing += OnWindowClosing;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.IsWindowMaximized)
            {
                WindowState = WindowState.Maximized;
                return;
            }
            Settings.Default.WindowLocation = RestoreBounds.Location;
            Settings.Default.WindowSize = RestoreBounds.Size;
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.WindowLocation = RestoreBounds.Location;
            Settings.Default.WindowSize = RestoreBounds.Size;
            Settings.Default.IsWindowMaximized = WindowState == WindowState.Maximized;

            App.Current.Shutdown();
        }
    }
}
