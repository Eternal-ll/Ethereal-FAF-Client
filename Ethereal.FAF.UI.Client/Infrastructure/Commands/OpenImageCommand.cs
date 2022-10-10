using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    public class OpenImageCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            var window = new UiWindow();
            var bitmap = new BitmapImage(new System.Uri(parameter.ToString()));
            window.Content = new Image()
            {
                Source = bitmap
            };
            window.Width = 512;
            window.Height = 512;
            window.ShowDialog();
            window.Content = null;
        }
    }
}
