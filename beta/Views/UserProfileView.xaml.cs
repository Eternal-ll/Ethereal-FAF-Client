using beta.Models.Server;
using beta.ViewModels;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for UserProfileView.xaml
    /// </summary>
    public partial class UserProfileView : UserControl
    {
        public UserProfileView() => InitializeComponent();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public UserProfileView(int id) : base() => DataContext = new ProfileViewModel(id);
        public UserProfileView(PlayerInfoMessage player) : base() => DataContext = new ProfileViewModel(player);
        public UserProfileView(PlayerInfoMessage player, NavigationService nav) : base() => DataContext = new ProfileViewModel(player, nav);
    }
}
