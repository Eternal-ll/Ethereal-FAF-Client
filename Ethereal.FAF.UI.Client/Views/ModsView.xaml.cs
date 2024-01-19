using Ethereal.FAF.UI.Client.ViewModels;
using System;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for ModsView.xaml
    /// </summary>
    public partial class ModsView : INavigableView<ModsViewModel>
    {
        public ModsView(ModsViewModel model)
        {
            ViewModel = model;

            //Resources.Add("DownloadMapCommand", model.DownloadMapCommand);

            Initialized += MapsView_Initialized;
            InitializeComponent();
        }

        private void MapsView_Initialized(object sender, EventArgs e)
        {
            ViewModel.RunRequest();
        }

        public ModsViewModel ViewModel { get; }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.RunRequest();
        }
    }
}
