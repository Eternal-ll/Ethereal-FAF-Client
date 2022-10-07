using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using System.Runtime.InteropServices;
using System;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Services;
using DesktopNotifications;
using DesktopNotifications.Windows;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class NotificationService
    {
        private static INotificationManager CreateManager()
        {
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            //{
            //    return new FreeDesktopNotificationManager();
            //}

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsNotificationManager();
            }

            //if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            //{
            //    return new AppleNotificationManager();
            //}
            return null;
        }

        private readonly SnackbarService SnackbarService;
        private readonly DialogService DialogService;
        private readonly LobbyClient LobbyClient;
        private readonly INotificationManager OSNotificationManager;

        public NotificationService(SnackbarService snackbarService, LobbyClient lobbyClient, DialogService dialogService)
        {
            SnackbarService = snackbarService;
            LobbyClient = lobbyClient;
            OSNotificationManager = CreateManager();

            lobbyClient.NotificationReceived += LobbyClient_NotificationReceived;
            DialogService = dialogService;
        }

        private void LobbyClient_NotificationReceived(object sender, global::FAF.Domain.LobbyServer.NotificationData e) => 
            Notify("Server", e.text);

        public void Notify(string title, string message, SymbolRegular symbol = SymbolRegular.Info24, bool ignoreOs = true)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                SnackbarService.Show(title, message, symbol);
            });
            if (ignoreOs) return;
            OSNotificationManager.ShowNotification(new Notification()
            {
                Title = title,
                Body = message
            });
        }
        public void Notify(string title, string message, Uri image, SymbolRegular symbol = SymbolRegular.Info24)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                SnackbarService.Show(title, message, symbol);
            });
            if (OSNotificationManager is not WindowsNotificationManager windows) return;
            windows.ShowNotification(new Notification()
            {
                Title = title,
                Body = message
            },
            hero: image);
        }
        public async Task<bool> ShowDialog(string title, string message, string leftButton = "Yes", string rightButton = "No")
        {
            var notification = new Notification()
            {
                Title = title,
                Body = message,
                Buttons =
                {
                    (leftButton, "answer_yes"),
                    (rightButton, "answer_no"),
                }
            };
            var reason = "";
            OSNotificationManager.NotificationActivated += (s, e) =>
            {
                reason = e.ActionId;
            };
            OSNotificationManager.NotificationDismissed += (s, e) =>
            {
                reason = e.Reason.ToString();
            };

            var dialog = DialogService.GetDialogControl();


            await OSNotificationManager.ShowNotification(notification);
            var osDialog = Task.Run(async () =>
            {
                while (string.IsNullOrWhiteSpace(reason))
                {
                    await Task.Delay(150);
                }
            });

            var appDialog = App.Current.Dispatcher.Invoke(async () =>
            {
                dialog.Content = null;
                dialog.ButtonLeftName = leftButton;
                dialog.ButtonRightName = rightButton;
                var result = await dialog.ShowAndWaitAsync("Invite", "Invites you to party", false);
                return result;
            });
            Task.WaitAny(osDialog, appDialog);

            App.Current.Dispatcher.Invoke(() =>
            {
                dialog.Hide();
            });

            return osDialog.IsCompleted ? reason == "answer_yes" : appDialog.Result is Wpf.Ui.Controls.Interfaces.IDialogControl.ButtonPressed.Left;
        }
    }
}
