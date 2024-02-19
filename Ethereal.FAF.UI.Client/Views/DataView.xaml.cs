using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for VaultView.xaml
    /// </summary>
    public partial class DataView: INavigableView<DataViewModel>
    {
        public DataView(DataViewModel model)
        {
            ViewModel = model;
            InitializeComponent();
        }

        public DataViewModel ViewModel { get; }
        public override ViewModel GetViewModel() => ViewModel;
    }
}
