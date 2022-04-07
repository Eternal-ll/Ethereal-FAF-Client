using beta.Infrastructure.Services.Interfaces;
using beta.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace beta.Infrastructure.Commands
{
    internal class HostGameCommand : Base.Command
    {
        private readonly IGameSessionService GameSessionService;
        private readonly INotificationService NotificationService;
        public HostGameCommand()
        {
            GameSessionService = App.Services.GetService<IGameSessionService>();
            NotificationService = App.Services.GetService<INotificationService>();
        }
        public override bool CanExecute(object parameter) => true;

        public override async void Execute(object parameter)
        {
            if (GameSessionService.GameIsRunning)
            {
                await NotificationService.ShowPopupAsync("Game is running");
                return;
            }
            var model = new HostGameViewModel();
            var result = await NotificationService.ShowDialog(model);
            if (result == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                model.HostGameCommand.Execute(null);
            }
        }
    }
}
