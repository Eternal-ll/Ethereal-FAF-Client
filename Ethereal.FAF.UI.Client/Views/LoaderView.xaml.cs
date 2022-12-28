using Ethereal.FAF.UI.Client.ViewModels;
using System.Windows;
using Wpf.Ui.Common.Interfaces;

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

        private void LinkTextBox_GotFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
