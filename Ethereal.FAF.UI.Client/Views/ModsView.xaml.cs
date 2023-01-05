using Ethereal.FAF.UI.Client.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for ModsView.xaml
    /// </summary>
    public partial class ModsView : INavigableView<ModsViewModel>
    {
        public ModsView(ModsViewModel vm)
        {
            ViewModel = vm;
            InitializeComponent();
        }

        public ModsViewModel ViewModel { get; }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.RunRequest();
        }
    }
}
