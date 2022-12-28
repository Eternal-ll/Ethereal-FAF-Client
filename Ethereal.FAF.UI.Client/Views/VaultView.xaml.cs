using Ethereal.FAF.UI.Client.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for VaultView.xaml
    /// </summary>
    public partial class VaultView : INavigableView<VaultViewModel>
    {
        public VaultView(VaultViewModel model)
        {
            ViewModel = model;
            InitializeComponent();
        }

        public VaultViewModel ViewModel { get; }
    }
}
