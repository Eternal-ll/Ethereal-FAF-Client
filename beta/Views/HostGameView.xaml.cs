using beta.ViewModels;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for HostGameView.xaml
    /// </summary>
    public partial class HostGameView : UserControl
    {
        public HostGameView() => InitializeComponent();

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e) =>
            ((HostGameViewModel)DataContext).Password = ((PasswordBox)sender).Password;
    }
}
