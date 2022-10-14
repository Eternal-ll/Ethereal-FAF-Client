using Ethereal.FAF.UI.Client.ViewModels;
using System.Windows.Controls;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : INavigableView<SettingsViewModel>
    {
        public SettingsView(SettingsViewModel viewmodel)
        {
            ViewModel = viewmodel;
            InitializeComponent();
        }
        public SettingsViewModel ViewModel { get; }

        private void VirtualizingStackPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var root = (VirtualizingStackPanel)sender;
            root.ScrollOwner = ScrollHost;
        }
    }
}
