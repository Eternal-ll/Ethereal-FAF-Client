using Ethereal.FAF.UI.Client.ViewModels;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for LoaderView.xaml
    /// </summary>
    public sealed partial class LoaderView : INavigableView<LoaderViewModel>
    {
        public LoaderViewModel ViewModel { get; }
        public LoaderView(LoaderViewModel vm)
        {
            ViewModel = vm;
            InitializeComponent();
        }
    }
}
