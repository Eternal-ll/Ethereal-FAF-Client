using beta.Infrastructure.Navigation;
using beta.Properties;
using ModernWpf;
using System.Windows;
using System.Windows.Threading;

namespace beta.Views.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Settings.Default.PropertyChanged += OnSettingChange;
            
            var navManager = new NavigationManager(MainFrame, ModalFrame);

            navManager.Navigate(new AuthView());

            Closing += OnMainWindowClosing;
            //var t = new ContentDialog();
            //t.PreviewKeyDown += (s, e) =>
            //{
            //    if (e.Key == System.Windows.Input.Key.Escape)
            //    {
            //        e.Handled = true;
            //    }
            //};

            Left = Settings.Default.Left;
            Top = Settings.Default.Top;

            Width = Settings.Default.Width;
            Height = Settings.Default.Height;
        }

        private void OnMainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Left = Left;
            Settings.Default.Top = Top;

            Settings.Default.Width = Width;
            Settings.Default.Height = Height;

            Closing -= OnMainWindowClosing;
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
        private void OnSettingChange(object sender, System.ComponentModel.PropertyChangedEventArgs e) => ((Settings)sender).Save();
    }
}
