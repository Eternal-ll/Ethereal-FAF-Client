using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for LinksView.xaml
    /// </summary>
    public partial class LinksView : INavigableView<LinksViewModel>
    {
        public LinksView(LinksViewModel model)
        {
            ViewModel = model;
            InitializeComponent();
        }

        public LinksViewModel ViewModel { get; }
    }
}
