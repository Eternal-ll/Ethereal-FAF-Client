using Ethereal.FAF.UI.Client.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for AuthView.xaml
    /// </summary>
    public partial class AuthView : INavigableView<AuthViewModel>
    {
        public AuthView(AuthViewModel authViewModel)
        {
            ViewModel = authViewModel;
            InitializeComponent();
        }
        public AuthViewModel ViewModel { get; }
    }
}
