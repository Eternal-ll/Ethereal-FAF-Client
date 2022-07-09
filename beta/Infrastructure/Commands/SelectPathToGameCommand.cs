using beta.Infrastructure.Services.Interfaces;
using beta.Properties;
using beta.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace beta.Infrastructure.Commands
{
    internal class SelectPathToGameCommand : Base.Command
    {
        private readonly INotificationService NotificationService;
        //public SelectPathToGameCommand() => NotificationService = App.Services.GetService<INotificationService>();

        public override bool CanExecute(object parameter) => true;

        public override async void Execute(object parameter)
        {
            var model = new SelectPathToGameViewModel();
            var result = await NotificationService.ShowDialog(model);
            if (result is ModernWpf.Controls.ContentDialogResult.None)
            {
                return;
            }
            Settings.Default.PathToGame = model.Path;
        }
    }
}
