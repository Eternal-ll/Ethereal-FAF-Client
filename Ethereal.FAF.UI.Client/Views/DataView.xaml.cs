using Ethereal.FAF.UI.Client.ViewModels;
using System.Windows.Controls;
using Wpf.Ui.Common.Interfaces;

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
        private void VirtualizingStackPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var root = (VirtualizingStackPanel)sender;


            //var property = ScrollHost.GetType().GetProperty("ScrollInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            //property.SetValue(ScrollHost, new ScrollInfoAdapter((IScrollInfo)property.GetValue(ScrollHost)));

            root.ScrollOwner = ScrollHost;
        }
    }
}
