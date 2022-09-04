using Ethereal.FAF.UI.Client.Infrastructure.Services;
using FAF.Domain.LobbyServer;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    internal class GenerateGameMapCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => true;
        public override void Execute(object parameter)
        {
            if (parameter is not GameInfoMessage game) return;
            Task.Run(async () =>
            {
                var mapgen = App.Hosting.Services.GetService<MapGenerator>();
                var generated = await mapgen.GenerateMapAsync(mapname: game.Mapname);
                game.SmallMapPreview = generated[0].Replace("_scenario.lua", ".png");
                game.OnPropertyChanged(nameof(game.SmallMapPreview));
            });
        }
    }
}
