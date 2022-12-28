using Ethereal.FAF.UI.Client.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for ChangelogView.xaml
    /// </summary>
    public partial class ChangelogView : INavigableView<ChangelogViewModel>
    {
        public ChangelogView(ChangelogViewModel model)
        {
            DataContext = model;
            ViewModel = model;
            InitializeComponent();
        }
        public ChangelogViewModel ViewModel { get; }
    }
}
