using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;

namespace beta.Infrastructure.Commands
{
    internal class ShowProfileCommand : Base.Command
    {
        private readonly IPlayersService PlayersService;
        public ShowProfileCommand()
        {
            //PlayersService = App.Services.GetService<IPlayersService>();
        }

        //private ProfileWindow _Window;
        ContentDialog Dialog;
        public override bool CanExecute(object parameter) => Dialog is null;

        public override void Execute(object parameter)
        {
            if (parameter is null)
            {
                // TODO Notify
                return;
            }
            PlayerInfoMessage model = null;

            if (parameter is PlayerInfoMessage player)
            {
                model = player;
            }
            if (parameter is int id)
            {
                model = PlayersService.GetPlayer(id.ToString());
            }
        }
    }
}
