using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using beta.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.IO;

namespace beta.Infrastructure.Commands
{
    internal class LaunchOfflineGameCommand : Base.Command
    {
        private readonly INotificationService NotificationService;
        private readonly SelectPathToGameCommand SelectPathToGameCommand;
        public LaunchOfflineGameCommand()
        {
            //NotificationService = App.Services.GetService<INotificationService>();
        }
        public override bool CanExecute(object parameter) => true;

        public override async void Execute(object parameter)
        {
            var path = Settings.Default.PathToGame;
            if (path[^1] != '\\') path += '\\';
            var pathToExe = path + "bin\\ForgedAlliance.exe";
            if (!File.Exists(pathToExe))
            {
                var model = new SelectPathToGameViewModel();
                var result = await NotificationService.ShowDialog(model);
                if (result is ModernWpf.Controls.ContentDialogResult.None)
                {
                    return;
                }
                Settings.Default.PathToGame = model.Path;
            }
            Process game = new()
            {
                StartInfo = new()
                {
                    FileName = pathToExe,
                    UseShellExecute = true,
                }
            };
        }
    }
}
