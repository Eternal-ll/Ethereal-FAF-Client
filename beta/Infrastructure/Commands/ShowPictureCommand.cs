using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace beta.Infrastructure.Commands
{
    internal class ShowPictureCommand : Base.Command
    {
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            if (parameter is string url && Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                var window = new Window
                {
                    Content = new Image
                    {
                        Source = new BitmapImage(new Uri(url))
                    }
                };
                window.Show();
            }
        }
    }
}
