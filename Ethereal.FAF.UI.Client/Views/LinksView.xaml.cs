using Ethereal.FAF.UI.Client.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for LinksView.xaml
    /// </summary>
    public sealed partial class LinksView : INavigableView<LinksViewModel>
    {
        public LinksView(LinksViewModel model)
        {
            ViewModel = model;
            InitializeComponent();
        }

        public LinksViewModel ViewModel { get; }
    }
}
