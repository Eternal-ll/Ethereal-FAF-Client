using System;
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
            var window = new UiWindow()
            {
                ResizeMode = System.Windows.ResizeMode.NoResize
            };
            var bitmap = new BitmapImage(new System.Uri(parameter.ToString()));
            var image = new Image()
            {
                Source = bitmap
            };
            window.Content = image;
            window.Width = 512;
            window.Height = 512;
            window.ShowDialog();
            image.Source = null;
            image.UpdateLayout();
            image = null;
            window.Content = null;
            bitmap.StreamSource?.Dispose();
            bitmap.StreamSource?.Close();
            bitmap.UriSource = null;
            window.UpdateLayout();
            window = null;
            GC.Collect();
        }
    }
}
