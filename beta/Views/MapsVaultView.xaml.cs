using beta.Infrastructure.Commands;
using ModernWpf.Media.Animation;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for MapsVaultView.xaml
    /// </summary>
    public partial class MapsVaultView : UserControl
    {
        public MapsVaultView()
        {
            InitializeComponent();
            ContentFrame.Navigate(typeof(MapsView));
            Resources.Add("NavigateCommand", new LambdaCommand(OnNavigateCommand));
        }

        private void OnNavigateCommand(object parameter)
        {
            if (parameter is null)
            {
                if (ContentFrame.CanGoBack)
                    ContentFrame.GoBack(new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                //ContentFrame.Navigate(typeof(MapsView), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                return;
            }
            if (parameter is int id)
            {
                ContentFrame.Navigate(typeof(MapDetailsView), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight});
            }
        }
    }
}
