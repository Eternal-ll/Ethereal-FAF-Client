using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Common;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for LoaderView.xaml
    /// </summary>
    public sealed partial class LoaderView : UserControl
    {
        public LoaderView() => InitializeComponent();

        public Task<bool> ShowNotification(string title, string message, SymbolRegular icon)
        {
            return RootSnackbar.ShowAsync(title, message, icon);
        }

        private void LinkTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(LinkTextBox.Text);
            ShowNotification("Notification", "Copied!", SymbolRegular.Copy24);
        }
        public CancellationTokenSource CancellationTokenSource;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource?.Cancel();
        }
    }
}
