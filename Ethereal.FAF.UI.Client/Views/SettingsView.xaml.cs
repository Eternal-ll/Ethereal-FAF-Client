using Ethereal.FAF.UI.Client.Resources.Controls;
using Ethereal.FAF.UI.Client.ViewModels;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public sealed partial class SettingsView : INavigableView<SettingsViewModel>
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

            
            //var property = ScrollHost.GetType().GetProperty("ScrollInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            //property.SetValue(ScrollHost, new ScrollInfoAdapter((IScrollInfo)property.GetValue(ScrollHost)));

            root.ScrollOwner = ScrollHost;
        }
    }
}
