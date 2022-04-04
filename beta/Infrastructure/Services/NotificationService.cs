using ModernWpf.Controls;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    internal class NotificationService : Interfaces.INotificationService
    {
        private ContentDialog ContentDialog;
        public NotificationService()
        {
            App.Current.Dispatcher.Invoke(() => ContentDialog = new()
            {
                CloseButtonText = "OK"
            });
            ContentDialog.Closed += ContentDialog_Closed;
        }

        private void ContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            sender.Content = null;
        }

        public async Task ShowPopupAsync(string text)
        {
            ContentDialog.Content = text;
            await ContentDialog.ShowAsync();
        }

        public async Task ShowPopupAsync(object model)
        {
            ContentDialog.Content = model;
            await ContentDialog.ShowAsync();
        }
    }
}
