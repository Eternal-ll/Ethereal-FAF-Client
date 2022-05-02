using beta.ViewModels;
using ModernWpf.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for NavigationView.xaml
    /// </summary>
    public partial class NavigationView
    {
        public NavigationView() => InitializeComponent();

        private void OnNavigationViewSelectionChanged(ModernWpf.Controls.NavigationView sender, NavigationViewSelectionChangedEventArgs args) =>
            ((NavigationViewModel)DataContext).CurrentViewTag = ((NavigationViewItem)args.SelectedItem).Tag?.ToString();
    }
}
