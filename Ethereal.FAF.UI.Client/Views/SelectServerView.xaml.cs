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
            this.IsVisibleChanged += SelectServerView_IsVisibleChanged;
            InitializeComponent();
        }

        private void SelectServerView_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true) 
            {
                ViewModel.StartApiCalls();
            }
            else
            {
                ViewModel.StopApiCalls();
            }
        }

        public SelectServerVM ViewModel { get; }
    }
}
