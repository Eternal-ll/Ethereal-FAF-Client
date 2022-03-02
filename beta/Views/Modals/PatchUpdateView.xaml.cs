using beta.ViewModels;
using System.Windows.Controls;

namespace beta.Views.Modals
{
    /// <summary>
    /// Interaction logic for PatchUpdatedView.xaml
    /// </summary>
    public partial class PatchUpdateView : UserControl
    {
        public TestDownloaderModel Model { get; set; }
        public PatchUpdateView(TestDownloaderModel model)
        {
            InitializeComponent();
            Model = model;
            DataContext = this;
            Model.Download();
        }
    }
}
