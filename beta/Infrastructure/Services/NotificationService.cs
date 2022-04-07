using beta.Infrastructure.Services.Interfaces;
using beta.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using System.Threading;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    internal class NotificationService : Interfaces.INotificationService
    {
        //private readonly IGameSessionService GameSessionService;

        public ContentDialog ContentDialog { get; private set; }
        public NotificationService()
        {
            //GameSessionService = App.Services.GetService<IGameSessionService>();

            App.Current.Dispatcher.Invoke(() => ContentDialog = new()
            {
                CloseButtonText = "OK"
            });
        }
        private void WaitForComplete()
        {
            while (ContentDialog.IsVisible)
            {
                Thread.Sleep(100);
            }
        }

        public async Task ShowPopupAsync(string text)
        {
            //WaitForComplete();
            await ContentDialog.Dispatcher.Invoke(async () =>
            {
                ContentDialog.PrimaryButtonText = null;
                ContentDialog.SecondaryButtonText = null;
                ContentDialog.CloseButtonText = "OK";
                ContentDialog.Content = text;
                await ContentDialog.ShowAsync();
            });
        }
        public void ShowPopup(string text)
        {
            ContentDialog = new();
            ContentDialog.CloseButtonText = "OK";
            ContentDialog.Content = text;
            ContentDialog.ShowAsync();
        }

        public async Task ShowPopupAsync(object model)
        {
            //WaitForComplete();
            ContentDialog.Dispatcher.Invoke(() =>
            {
                ContentDialog = new();
                ContentDialog.PrimaryButtonText = null;
                ContentDialog.SecondaryButtonText = null;
                ContentDialog.CloseButtonText = "OK";
                ContentDialog.Content = model;
                ContentDialog.ShowAsync();
            });
        }

        public async Task<ContentDialogResult> ShowDialog(string text)
        {
            ContentDialog = new();
            ContentDialog.Content = text;
            return await ContentDialog.ShowAsync();
        }

        public async Task<ContentDialogResult> ShowDialog(object model, string primary = null, string secondary = null, string close = null)
        {
            ContentDialog = new();
            ContentDialog.Content = model;

            ContentDialog.PrimaryButtonText = primary;
            ContentDialog.SecondaryButtonText = secondary;
            ContentDialog.CloseButtonText = close;

            switch (model)
            {
                case SelectPathToGameViewModel pathModel:
                    ContentDialog.PrimaryButtonCommand = pathModel.ConfirmCommand;
                    ContentDialog.PrimaryButtonText = "Save";
                    ContentDialog.CloseButtonText = "Close";
                    break;
                case PassPasswordViewModel passwordModel:
                    ContentDialog.PrimaryButtonCommand = passwordModel.PassPasswordCommand;
                    ContentDialog.PrimaryButtonText = "Pass";
                    ContentDialog.IsPrimaryButtonEnabled = false;
                    ContentDialog.CloseButtonText = "Cancel";
                    break;
                case HostGameViewModel hostVM:
                    //ContentDialog.PrimaryButtonCommand = hostVM.HostGameCommand;
                    ContentDialog.PrimaryButtonText = "Host";
                    ContentDialog.CloseButtonText = "Cancel";
                    break;
            }

            return await ContentDialog.ShowAsync();
        }

        public async Task<ContentDialogResult> ShowDialog(string text, string primary = null, string secondary = null, string close = null)
        {
            ContentDialog = new();
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
