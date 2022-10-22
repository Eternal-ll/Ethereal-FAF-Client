using Ethereal.FAF.UI.Client.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for GamesView.xaml
    /// </summary>
    public partial class GamesView : INavigableView<GamesViewModel>
    {
        public GamesView(GamesViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            Resources.Add("JoinGameCommand", ViewModel.JoinGameCommand);
            Resources.Add("WatchGameCommand", ViewModel.WatchGameCommand);
        }

        public GamesViewModel ViewModel { get; }

        private void Image_Initialized(object sender, System.EventArgs e)
        {
            var image = (Image)sender;
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.LowQuality);
            if (image.Source is not null && image.Source is BitmapImage bitmap)
            {
                if (!bitmap.IsFrozen && bitmap.CanFreeze)
                {
                    bitmap.Freeze();
                }
            }
            image.Initialized -= Image_Initialized;
        }
        Task task;
        private void Image_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool visible && visible) return;
            var image = (Image)sender;
            //var t = GC.GetGeneration(image.Source);
            image.Source = null;
            try
            {
                image.UpdateLayout();
            }
            catch
            {
            }
            //GC.Collect(1);
            //GC.WaitForPendingFinalizers();
            //return;
            if (task is not null) return;
            task = Task.Run(async () =>
            {
                await Task.Delay(500);
                Dispatcher.Invoke(UpdateLayout);
                //task = null;
                GC.Collect(1);
                task= null;
                //GC.WaitForPendingFinalizers();
            });
        }
    }
}
