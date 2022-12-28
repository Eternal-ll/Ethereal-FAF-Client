using Ethereal.FAF.UI.Client.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for NavigationView.xaml
    /// </summary>
    public partial class NavigationView : INavigableView<NavigationViewModel>
    {
        public NavigationView(NavigationViewModel vm)
        {
            ViewModel = vm;
            InitializeComponent();
        }

        public NavigationViewModel ViewModel { get; }
    }
}
