using beta.ViewModels;
using ModernWpf.Controls;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    internal class NotificationService : Interfaces.INotificationService
    {
        public ContentDialog ContentDialog { get; private set; }
        public NotificationService()
        {
            App.Current.Dispatcher.Invoke(() => ContentDialog = new()
            {
                CloseButtonText = "OK"
            });
            ContentDialog.Opened += ContentDialog_Opened;
            ContentDialog.Closed += ContentDialog_Closed;
        }

        private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
        }

        private void ContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            sender.Content = null;
        }

        public async Task ShowPopupAsync(string text)
        {
            ContentDialog.Dispatcher.Invoke(() =>
            {
                ContentDialog.PrimaryButtonText = null;
                ContentDialog.SecondaryButtonText = null;
                ContentDialog.CloseButtonText = "OK";
                ContentDialog.Content = text;
                ContentDialog.ShowAsync();
            });
        }

        public async Task ShowPopupAsync(object model)
        {
            ContentDialog.Dispatcher.Invoke(() =>
            {
                ContentDialog.PrimaryButtonText = null;
                ContentDialog.SecondaryButtonText = null;
                ContentDialog.CloseButtonText = "OK";
                ContentDialog.Content = model;
                ContentDialog.ShowAsync();
            });
        }

        public async Task<ContentDialogResult> ShowDialog(string text)
        {
            ContentDialog.Content = text;
            return await ContentDialog.ShowAsync();
        }

        public async Task<ContentDialogResult> ShowDialog(object model, string primary = null, string secondary = null, string close = null)
        {
            ContentDialog.Content = model;

            ContentDialog.PrimaryButtonText = primary;
            ContentDialog.SecondaryButtonText = secondary;
            ContentDialog.CloseButtonText = close;

            if (model is SelectPathToGameViewModel pathModel)
            {
                ContentDialog.PrimaryButtonCommand = pathModel.ConfirmCommand;
                ContentDialog.PrimaryButtonText = "Save";
                ContentDialog.CloseButtonText= "Close";
            }

            return await ContentDialog.ShowAsync();
        }

        public async Task<ContentDialogResult> ShowDialog(string text, string primary = null, string secondary = null, string close = null)
        {
            ContentDialog.Content = text;

            ContentDialog.PrimaryButtonText = primary;
            ContentDialog.SecondaryButtonText = secondary;
            ContentDialog.CloseButtonText = close;

            return await ContentDialog.ShowAsync();
        }

        public async Task<bool> ShowDownloadDialog(DownloadViewModel model, string close = null)
        {
            model.Completed += Download_Completed;

            ContentDialog.PreviewKeyDown += HideEscapeKey;

            var result = await ShowDialog(model, secondary: close, close: "Hide");
            
            ContentDialog.PreviewKeyDown -= HideEscapeKey;

            if (result == ContentDialogResult.None)
            {
                return model.IsCompleted;
            }

            model.Cancel();
            return false;
        }

        private void HideEscapeKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                e.Handled = true;
            }
        }

        private void Download_Completed(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            ((DownloadViewModel)sender).Completed -= Download_Completed;
            ContentDialog.Dispatcher.Invoke(() => ContentDialog.Hide());
        }
    }
}
