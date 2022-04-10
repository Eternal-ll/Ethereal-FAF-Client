using beta.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace beta.Views.Modals
{
    /// <summary>
    /// Interaction logic for PassPasswordView.xaml
    /// </summary>
    public partial class PassPasswordView : UserControl
    {
        public PassPasswordView() => InitializeComponent();

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) =>
            ((PassPasswordViewModel)DataContext).Password = ((PasswordBox)sender).Password;

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is not null) PasswordBox.Password = null;
            if (e.OldValue is not null) ((PassPasswordViewModel)e.OldValue).Dispose();
        }
    }
}
