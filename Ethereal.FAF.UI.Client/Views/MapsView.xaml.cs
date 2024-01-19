using Ethereal.FAF.UI.Client.ViewModels;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for MapsView.xaml
    /// </summary>
    public partial class MapsView : INavigableView<MapsViewModel>
    {
        public MapsView(MapsViewModel model)
        {
            ViewModel = model;

            Resources.Add("DownloadMapCommand", model.DownloadMapCommand);

            Initialized += MapsView_Initialized;
            InitializeComponent();
        }

        private void MapsView_Initialized(object sender, System.EventArgs e)
        {
            ViewModel.RunRequest();
        }

        public MapsViewModel ViewModel { get; }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.RunRequest();
        }
    }
}
