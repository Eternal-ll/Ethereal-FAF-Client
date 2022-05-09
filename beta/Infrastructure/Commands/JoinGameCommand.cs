using beta.Infrastructure.Commands.Base;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using Microsoft.Extensions.DependencyInjection;
using System;

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
                await GameLauncherService.JoinGame(game)
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
