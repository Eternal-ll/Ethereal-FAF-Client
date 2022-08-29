using Ethereal.FAF.UI.Client.ViewModels;
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
        }

        public GamesViewModel ViewModel { get; }
    }
}
