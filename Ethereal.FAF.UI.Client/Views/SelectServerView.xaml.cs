using Ethereal.FAF.UI.Client.ViewModels.Servers;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for SelectServerView.xaml
    /// </summary>
    public partial class SelectServerView : INavigableView<SelectServerVM>
    {
        public SelectServerView(SelectServerVM vm)
        {
            ViewModel = vm;
            DataContext = vm;
            InitializeComponent();
        }

        public SelectServerVM ViewModel { get; }
    }
}
