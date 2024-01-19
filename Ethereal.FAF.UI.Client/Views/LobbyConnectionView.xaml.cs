using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for LobbyConnectionView.xaml
    /// </summary>
    public partial class LobbyConnectionView : PageBase
    {
        public LobbyConnectionView(LobbyConnectionViewModel viewModel)
        {
            DataContext = this;
            ViewModel = viewModel;
            InitializeComponent();
        }
        public LobbyConnectionViewModel ViewModel { get; set; }
        public override ViewModel GetViewModel() => ViewModel;
    }
}
