using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.Models.Progress;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wpf.Ui;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public partial class GameManager : ObservableObject
    {
        private IGameLauncher _gameLauncher;
        private IContentDialogService _contentDialogService;
        public bool Initialized;
        public void Initialize(
            IGameLauncher gameLauncher,
            IContentDialogService contentDialogService)
        {
            if (Initialized) return;
            _gameLauncher = gameLauncher;
            _contentDialogService = contentDialogService;
            Initialized = true;

            _gameLauncher.OnState += _gameLauncher_OnState;
        }

        private void _gameLauncher_OnState(object sender, GameLauncherState e)
        {
            if (e == GameLauncherState.Joining)
            {
                JoiningGame = true;
            }
            else
            {
                JoiningGame = false;
            }
        }

        [RelayCommand]
        private async Task JoinGame(Game game)
        {
            string password = null;
            if (game.PasswordProtected)
            {
                var textbox = new PasswordBox();
                var result = await _contentDialogService.ShowSimpleDialogAsync(new()
                {
                    Title = "Enter password for lobby",
                    Content = textbox,
                    PrimaryButtonText = "Join",
                    SecondaryButtonText = string.Empty,
                    CloseButtonText = "Cancel",
                });
                if (result != Wpf.Ui.Controls.ContentDialogResult.Primary)
                {
                    return;
                }
                password = textbox.Password;
            }
            JoiningCancellationTokenSource = new();
            var progress = new Progress<ProgressReport>(x =>
            {
                ProgressValue = Convert.ToInt32(x.Percentage);
                IsProgressIndeterminate = x.IsIndeterminate;
                ProgressText = $"{x.Title}: {x.Message}";
            });
            await _gameLauncher.JoinGameAsync(game, password, progress,
                JoiningCancellationTokenSource.Token);
        }

        private CancellationTokenSource JoiningCancellationTokenSource;

        [ObservableProperty]
        private int _ProgressValue;
        [ObservableProperty]
        private string _ProgressText;
        [ObservableProperty]
        private bool _IsProgressIndeterminate;
        [ObservableProperty]
        private bool _JoiningGame = false;
        [RelayCommand]
        private void Cancel()
        {
            if (JoiningCancellationTokenSource.IsCancellationRequested) return;
            JoiningCancellationTokenSource.Cancel();
        }

    }
}
