using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    internal class GenerateGameMapCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => 
            parameter is Game game && 
            game.PreviewType == PreviewType.Neroxis &&
            game.MapGeneratorState is MapGeneratorState.NotGenerated;
        public override void Execute(object parameter) => 
            Task.Run(async () =>
            {
                var game = (Game)parameter;
                game.MapGeneratorState = MapGeneratorState.Generating;
                var mapgen = App.Hosting.Services.GetService<MapGenerator>();
                var generated = await mapgen.GenerateMapAsync(mapname: game.Mapname);
                game.SmallMapPreview = generated[0].Replace("_scenario.lua", "_preview.png");
                game.OnPropertyChanged(nameof(game.SmallMapPreview));
                game.MapGeneratorState = MapGeneratorState.Generated;
            });
    }
}
