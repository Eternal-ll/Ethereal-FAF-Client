using Ethereal.FAF.UI.Client.ViewModels;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for PlayTabPage.xaml
    /// </summary>
    public partial class PlayTabPage : INavigableView<PlayTabViewModel>
    {
        public PlayTabPage(PlayTabViewModel vm)
        {
            ViewModel = vm;
            InitializeComponent();
        }

        public PlayTabViewModel ViewModel { get; }
        public override ViewModel GetViewModel() => ViewModel;
    }
}
