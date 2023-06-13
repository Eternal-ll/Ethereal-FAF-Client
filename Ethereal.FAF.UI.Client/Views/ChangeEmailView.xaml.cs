using Ethereal.FAF.UI.Client.ViewModels;
using System;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for ChangeEmailView.xaml
    /// </summary>
    public partial class ChangeEmailView : UserControl, IDisposable
    {
        public ChangeEmailView(ChangeEmailViewModel vm)
        {
            DataContext = this;
            ViewModel = vm;
            InitializeComponent();
        }

        public ChangeEmailViewModel ViewModel { get; }

        public void Dispose()
        {
            ViewModel.Dispose();
            GC.SuppressFinalize(this);
        }
        private void PasswordBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            return;
            var input = (Wpf.Ui.Controls.PasswordBox)sender;
            UpdateButton.CommandParameter = input.Password;
            ViewModel.Model.Password = input.Password;
        }
    }
}
