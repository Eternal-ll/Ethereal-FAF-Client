using beta.Infrastructure.Commands.Base;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using System.Threading.Tasks;

namespace beta.Infrastructure.Commands
{
    internal class JoinGameCommand : Command
    {
        private readonly IGameSessionService GameLauncherService;
        private readonly INotificationService NotificationService;
        public JoinGameCommand()
        {
            GameLauncherService = App.Services.GetService<IGameSessionService>();
            NotificationService = App.Services.GetService<INotificationService>();
        }
        public override bool CanExecute(object parameter)
        {
            if (parameter is GameInfoMessage game)
                return game.State == Models.Server.Enums.GameState.Open;
            return false;
        }

        public override async void Execute(object parameter)
        {
            if (parameter is GameInfoMessage game)
            {
                string password = null;
                if (game.password_protected)
                {
                    PassPasswordViewModel model = new();
                    var result = await NotificationService.ShowDialog(model);
                    if (result is ContentDialogResult.None)
                    {
                        return;
                    }
                    password = model.Password;
                }
                await Task.Run(async () => await GameLauncherService.JoinGame(game, password))
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            NotificationService.ShowExceptionAsync(task.Exception);
                        }
                    });
            }
        }
    }
}
