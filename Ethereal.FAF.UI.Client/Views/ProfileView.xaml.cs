using System.Windows.Controls;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for ProfileView.xaml
    /// </summary>
    public partial class ProfileView : INavigableView<object>
    {
        public ProfileView()
        {
            InitializeComponent();
        }

        public object ViewModel { get; }
        private void VirtualizingStackPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var root = (VirtualizingStackPanel)sender;


            //var property = ScrollHost.GetType().GetProperty("ScrollInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            //property.SetValue(ScrollHost, new ScrollInfoAdapter((IScrollInfo)property.GetValue(ScrollHost)));

            root.ScrollOwner = ScrollHost;
        }
    }
}
