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

            //var top = Settings.Default.Top;
            //var left = Settings.Default.Left;
            //var height = Settings.Default.Height;
            //var width = Settings.Default.Width;

            //if (top < 0 || top > 1000) top = 0;
            //if(left < 0 || left > 1000) left = 0;
            //if (height < 0 || height > 1000) height = 1000;
            //if (width < 0 || width > 1000) width = 1000;

            //Top = top;
            //Left = left;
            //Height = height;
            //Width = width;

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
