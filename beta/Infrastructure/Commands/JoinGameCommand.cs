using beta.Infrastructure.Commands.Base;
using beta.Infrastructure.Services.Interfaces;
using beta.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace beta.Infrastructure.Commands
{
    internal class JoinGameCommand : Command
    {
        private readonly IGameLauncherService GameLauncherService;
        public JoinGameCommand() => GameLauncherService = App.Services.GetService<IGameLauncherService>();
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            if (parameter is GameVM game)
                new Thread(() => GameLauncherService.JoinGame(game))
                {
                    Name = "Game launcher thread"
                }.Start();
        }
    }
}
