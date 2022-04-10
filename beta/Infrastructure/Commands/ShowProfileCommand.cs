using beta.Models.Server;
using beta.ViewModels;
using beta.Views.Windows;
using ModernWpf.Controls;
using System.Windows;

namespace beta.Infrastructure.Commands
{
    internal class ShowProfileCommand : Base.Command
    {
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
            if (parameter is not PlayerInfoMessage model) 
            {
                return;
            }
            Dialog = new()
            {
                //Owner = App.Current.MainWindow,
                Content = new ProfileViewModel(model)
            };
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
            Dialog = null;
        }

        private void _ProfileWindow_Closed(object sender, System.EventArgs e)
        {
            //((Window)sender).Closed -= _ProfileWindow_Closed;
            //_Window = null;
        }
    }
}
