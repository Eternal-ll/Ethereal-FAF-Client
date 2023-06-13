using Ethereal.FAF.UI.Client.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for DownloadsView.xaml
    /// </summary>
    public partial class DownloadsView : INavigableView<DownloadsViewModel>
    {
        public DownloadsView(DownloadsViewModel vm)
        {
            ViewModel = vm;
            InitializeComponent();
        }

        public DownloadsViewModel ViewModel { get; }
    }
}
