using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for SelectServerView.xaml
    /// </summary>
    public partial class SelectServerView : PageBase
    {
        public SelectServerView(SelectServerViewModel vm)
        {
            ViewModel = vm;
            DataContext = this;
            InitializeComponent();
        }

        public SelectServerViewModel ViewModel { get; }
        public override ViewModel GetViewModel() => ViewModel;
    }
}
