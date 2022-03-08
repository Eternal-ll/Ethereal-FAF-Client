using beta.ViewModels;
using System.Windows.Controls;

namespace beta.Views.Modals
{
    /// <summary>
    /// Interaction logic for PatchUpdatedView.xaml
    /// </summary>
    public partial class PatchUpdateView : UserControl
    {
        public TestDownloaderVM Model { get; set; }
        public PatchUpdateView(TestDownloaderVM model)
        {
            InitializeComponent();
            Model = model;
            DataContext = this;
            Model.Download();
        }
    }
}
