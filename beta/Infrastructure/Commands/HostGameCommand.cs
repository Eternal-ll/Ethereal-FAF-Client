using beta.Infrastructure.Services.Interfaces;
using beta.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Media;

namespace beta.Infrastructure.Commands
{
    internal class HostGameCommand : Base.Command
    {
        private IGameSessionService GameSessionService;
        private INotificationService NotificationService;
        private Window Window;
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            var gameSessionService = GameSessionService ??= ServiceProvider.GetService<IGameSessionService>();
            if (gameSessionService.GameIsRunning)
            {
                (NotificationService ??= ServiceProvider.GetService<INotificationService>())
                    .ShowPopupAsync("Game is running");
                return;
            }
            var model = App.Services.GetService<HostGameViewModel>();
            Window = new();
            Window.Background = (Brush)App.Current.Resources["NavigationViewDefaultPaneBackground"];
            Window.Content = model;
            Window.Closed += Window_Closed;
            model.Finished += (s, e) => Window.Close();
            Window.ShowDialog();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Window.Closed -= Window_Closed;
            Window = null;
        }
    }
}
