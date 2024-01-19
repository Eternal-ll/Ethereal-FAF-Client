using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for AuthView.xaml
    /// </summary>
    public partial class AuthView : PageBase, INavigableView<AuthViewModel>
    {
        public AuthView(AuthViewModel authViewModel)
        {
            ViewModel = authViewModel;
            InitializeComponent();
        }
        public AuthViewModel ViewModel { get; }
        public override ViewModel GetViewModel() => ViewModel;

        private void ContentPresenter_GiveFeedback(object sender, System.Windows.GiveFeedbackEventArgs e)
        {

        }
    }
}
