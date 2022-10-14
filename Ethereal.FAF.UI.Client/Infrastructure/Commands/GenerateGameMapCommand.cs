using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Models.Lobby;
using Ethereal.FAF.UI.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    internal sealed class CopyCommand : Base.Command
    {
        private NotificationService Notification { get; set; }
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            if (parameter is null) return;
            Notification ??= App.Hosting.Services.GetService<NotificationService>();
            Clipboard.SetText(parameter.ToString());
            Notification.Notify("Copy", $"Copied \"{parameter}\" !", Wpf.Ui.Common.SymbolRegular.Copy24);
        }
    }
    internal class GenerateGameMapCommand : Base.Command
    {
        private NotificationService Notification => App.Hosting.Services.GetService<NotificationService>();
        public override bool CanExecute(object parameter) => 
            parameter is Game game && 
            game.IsMapgen &&
            game.MapGeneratorState is MapGeneratorState.NotGenerated;
        public override void Execute(object parameter) => 
            Task.Run(async () =>
            {
                var game = (Game)parameter;
                game.MapGeneratorState = MapGeneratorState.Generating;
                var mapgen = App.Hosting.Services.GetService<MapGenerator>();
                var generated = await mapgen.GenerateMapAsync(mapname: game.Mapname);
                var preview = generated[0].Replace("_scenario.lua", "_preview.png");
                if (!File.Exists(preview))
                {
                    await Exception(game, $"Generated map preview not exists. Path: {preview}");
                    return;
                }
                game.SmallMapPreview = preview;
                game.OnPropertyChanged(nameof(game.SmallMapPreview));
                game.MapGeneratorState = MapGeneratorState.Generated;
                Notification.Notify("Map generator", $"Map {game.Mapname} is generated", new System.Uri(preview));
            })
            .ContinueWith(async t =>
            {
                var game = (Game)parameter;
                if (t.IsFaulted)
                {
                    await Exception(game, t.Exception.ToString());
                }
            });
        private async Task Exception(Game game, string exception)
        {
            game.MapGeneratorException = exception;
            game.MapGeneratorState = MapGeneratorState.Faulted;
            await Task.Delay(System.TimeSpan.FromSeconds(5));
            game.MapGeneratorState = MapGeneratorState.NotGenerated;
            game.MapGeneratorException = null;
            Notification.Notify("Map generator", exception);
        }
    }
}
