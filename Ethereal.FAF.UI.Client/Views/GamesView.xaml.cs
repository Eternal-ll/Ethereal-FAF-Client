using Ethereal.FAF.UI.Client.ViewModels;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        }
    }
}
