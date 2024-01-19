using Ethereal.FAF.UI.Client.ViewModels;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for PlayPage.xaml
    /// </summary>
    public partial class PlayPage : INavigableView<PlayPageViewModel>
    {
        public PlayPage(PlayPageViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        public PlayPageViewModel ViewModel { get; }
    }
}
