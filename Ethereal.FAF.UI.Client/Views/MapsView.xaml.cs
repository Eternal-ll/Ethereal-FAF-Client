using Ethereal.FAF.UI.Client.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for MapsView.xaml
    /// </summary>
    public partial class MapsView : INavigableView<MapsViewModel>
    {
        public MapsView(MapsViewModel model)
        {
            ViewModel = model;
            InitializeComponent();
        }

        public MapsViewModel ViewModel { get; }
    }
}
