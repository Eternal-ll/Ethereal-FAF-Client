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

            Left = Settings.Default.Left;
            Top = Settings.Default.Top;
            Height = Settings.Default.Height;
            Width = Settings.Default.Width;

            Closing += OnWindowClosing;
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Left = Left;
            Settings.Default.Top = Top;
            Settings.Default.Height = Height;
            Settings.Default.Width = Width;
        }
    }
}
