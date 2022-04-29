using beta.Infrastructure.Services.Interfaces;
using beta.ViewModels;
using beta.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace beta.Infrastructure.Commands
{
    internal class HostGameCommand : Base.Command
    {
        private readonly IGameSessionService GameSessionService;
        private readonly INotificationService NotificationService;
        private Window Window;
        public HostGameCommand()
        {
            GameSessionService = App.Services.GetService<IGameSessionService>();
            NotificationService = App.Services.GetService<INotificationService>();
        }
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            if (GameSessionService.GameIsRunning)
            {
                NotificationService.Notify("Game is running");
                return;
            }
            var model = new HostGameViewModel();
            Window = new();
            Window.Content = model;
            Window.Closed += Window_Closed;
            Window.ShowDialog();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Window.Closed -= Window_Closed;
            Window = null;
        }
    }
}
