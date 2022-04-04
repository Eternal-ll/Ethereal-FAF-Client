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

            var left = Settings.Default.Left;
            Left = left < 0 ? 0 : left;
            var top = Settings.Default.Top;
            Top = left < 0 ? 0 : left;
            var height = Settings.Default.Height;
            Height = left < 0 ? 0 : left;
            var width = Settings.Default.Width;
            Width = left < 0 ? 0 : left;

            Closing += OnWindowClosing;
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Left = Left;
            Settings.Default.Top = Top;
            Settings.Default.Height = ActualHeight;
            Settings.Default.Width = ActualWidth;
        }
    }
}
