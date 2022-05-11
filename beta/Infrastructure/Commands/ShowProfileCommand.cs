using beta.Infrastructure.Services.Interfaces;
using beta.Models.Server;
using beta.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;

namespace beta.Infrastructure.Commands
{
    internal class ShowProfileCommand : Base.Command
    {
        private readonly IPlayersService PlayersService;
        public ShowProfileCommand()
        {
            PlayersService = App.Services.GetService<IPlayersService>();
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

            if (model is null) return;
            Dialog = new()
            {
                //Owner = App.Current.MainWindow,
                Content = new ProfileViewModel(model)
            };
            Dialog.CloseButtonText = "Close";
            Dialog.Closed += Dialog_Closed;
            Dialog.ShowAsync();

            //_Window = new()
            //{
            //    Owner = App.Current.MainWindow,
            //    DataContext = new ProfileViewModel(model)
            //};
            //_Window.Closed += _ProfileWindow_Closed;
            //_Window.ShowDialog();
        }

        private void Dialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            sender.Closed -= Dialog_Closed;
            ((ProfileViewModel)Dialog.Content).Dispose();
            Dialog.Content = null;
            Dialog = null;
        }

        private void _ProfileWindow_Closed(object sender, System.EventArgs e)
        {
            //((Window)sender).Closed -= _ProfileWindow_Closed;
            //_Window = null;
        }
    }
}
