using Ethereal.FAF.UI.Client.Infrastructure.Attributes;
using Ethereal.FAF.UI.Client.ViewModels.Data;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for MatchmakerQueuesView.xaml
    /// </summary>
    [Transient]
    public partial class MatchmakerQueuesView : INavigableView<MatchmakerQueuesViewModel>
    {
        public MatchmakerQueuesView(MatchmakerQueuesViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
        public MatchmakerQueuesViewModel ViewModel => (MatchmakerQueuesViewModel)DataContext;
    }
}
