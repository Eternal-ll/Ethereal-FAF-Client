using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for PrepareClientView.xaml
    /// </summary>
    public partial class PrepareClientView : INavigableView<PrepareClientViewModel>
    {
        public PrepareClientView(PrepareClientViewModel vm)
        {
            ViewModel = vm;
            InitializeComponent();
        }

        public PrepareClientViewModel ViewModel { get; }
        public override ViewModel GetViewModel() => ViewModel;
    }
}
