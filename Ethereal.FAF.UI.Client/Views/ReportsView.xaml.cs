using Ethereal.FAF.UI.Client.ViewModels;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for ReportsView.xaml
    /// </summary>
    public partial class ReportsView : INavigableView<ReportsViewModel>
    {
        public ReportsView(ReportsViewModel viewmodel)
        {
            ViewModel = viewmodel;
            InitializeComponent();
        }

        public ReportsViewModel ViewModel { get; }

        private void VirtualizingStackPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var root = (VirtualizingStackPanel)sender;
            //root.ScrollOwner = ScrollHost;
        }
    }
}
