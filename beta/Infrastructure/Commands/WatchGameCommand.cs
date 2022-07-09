using beta.Infrastructure.Commands.Base;
using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.Models.Server.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace beta.Infrastructure.Commands
{
    internal class WatchGameCommand : Command
    {
        private readonly IGameSessionService GameLauncherService;
        private readonly INotificationService NotificationService;
        public WatchGameCommand()
        {
            //GameLauncherService = App.Services.GetService<IGameSessionService>();
            //NotificationService = App.Services.GetService<INotificationService>();
        }
        public override bool CanExecute(object parameter)
        {
            //if (parameter is GameInfoMessage game)
            //    return game.State == GameState.Playing && game.ReplayLessThanFiveMinutes;
            //if (parameter is PlayerInfoMessage player && player.Game is not null)
            //    return player.Game.State == GameState.Playing && player.Game.ReplayLessThanFiveMinutes;
            return true;
        }

        public override async void Execute(object parameter)
        {
            PlayerInfoMessage player = null;
            if (parameter is PlayerInfoMessage pPlayer)
            {
                player = pPlayer;
            }
            else if (parameter is GameInfoMessage game)
            {
                player = game.Players[0];
            }
            if (player is not null)
            {
                if (player.Game is null)
                {
                    await NotificationService.ShowPopupAsync("Player left from game");
                    return;
                }
                if (player.Game.mapname.StartsWith("Nexorix", System.StringComparison.OrdinalIgnoreCase))
                {
                    await NotificationService.ShowPopupAsync("Unsupported map game");
                    return;
                }
                if (player.Game.State != GameState.Playing)
                {
                    await NotificationService.ShowPopupAsync("Game is not launched");
                    return;
                }
                if (player.Game.ReplayLessThanFiveMinutes)
                {
                    await NotificationService.ShowPopupAsync("Replay is less than 5 minutes");
                    return;
                }
                Task.Run(() => GameLauncherService.WatchGame(player.Game.uid, player.Game.mapname, player.id, player.Game.FeaturedMod, true))
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
