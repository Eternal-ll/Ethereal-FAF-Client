using beta.ViewModels;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for HostGameView.xaml
    /// </summary>
    public partial class HostGameView : UserControl
    {
        private readonly HostGameViewModel ViewModel;
        public HostGameView(HostGameViewModel model)
        {
            ViewModel = model;
            DataContext = model;
            InitializeComponent();
            IsVisibleChanged += HostGameView_IsVisibleChanged;
        }

        private void HostGameView_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                ViewModel.AddSelectionEvent();
                MapsViewContentControl.Content = ViewModel.Maps;
            }
            else
            {
                ViewModel.RemoveSelectionEvent();
                MapsViewContentControl.Content = null;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e) =>
            ViewModel.Password = ((PasswordBox)sender).Password;

        private void MapsView_MapSelected(object sender, string e)
        {

        }
    }
}
