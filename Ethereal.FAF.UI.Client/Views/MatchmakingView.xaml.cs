using Ethereal.FAF.UI.Client.Infrastructure.Attributes;
using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for MatchmakingView.xaml
    /// </summary>
    [Transient]
    public partial class MatchmakingView : INavigableView<MatchmakingViewModel>
    {
        public MatchmakingView(MatchmakingViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
        public MatchmakingViewModel ViewModel { get; }
        public override ViewModel GetViewModel()
        {
            return ViewModel;
        }
    }
}
