using Ethereal.FAF.UI.Client.ViewModels;
using System.Windows.Controls;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views.Hosting
{
    /// <summary>
    /// Interaction logic for HostGameView.xaml
    /// </summary>
    public sealed partial class HostGameView : INavigableView<HostGameViewModel>
    {
        public HostGameView(HostGameViewModel model)
        {
            ViewModel = model;
            DataContext = ViewModel;
            InitializeComponent();
        }

        public HostGameViewModel ViewModel { get; }

        private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var input = (Wpf.Ui.Controls.PasswordBox)sender;
            ViewModel.Game.Password = input.Password;
        }
    }
}
