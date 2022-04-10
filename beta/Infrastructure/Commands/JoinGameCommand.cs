using beta.Infrastructure.Commands.Base;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using Microsoft.Extensions.DependencyInjection;

namespace beta.Infrastructure.Commands
{
    internal class JoinGameCommand : Command
    {
        private readonly IGameSessionService GameLauncherService;
        public JoinGameCommand() => GameLauncherService = App.Services.GetService<IGameSessionService>();
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            if (parameter is GameInfoMessage game)
                GameLauncherService.JoinGame(game);
        }
    }
}
