using Ethereal.FAF.UI.Client.ViewModels;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for ProfileView.xaml
    /// </summary>
    public partial class ProfileView : INavigableView<ProfileViewModel>
    {
        public ProfileView(ProfileViewModel model)
        {
            ViewModel = model;
            InitializeComponent();
        }

        public ProfileViewModel ViewModel { get; }
        private void VirtualizingStackPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var root = (VirtualizingStackPanel)sender;
            //root.ScrollOwner = ScrollHost;
        }
    }
}
