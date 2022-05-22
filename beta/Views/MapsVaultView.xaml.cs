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
            ContentFrame.Navigate(new MapsView(ContentFrame.NavigationService));
            Resources.Add("NavigateCommand", new LambdaCommand(OnNavigateCommand));
            ((NavigationThemeTransition)ContentFrame.ContentTransitions[0])
                .DefaultNavigationTransitionInfo = new SlideNavigationTransitionInfo()
                { Effect = SlideNavigationTransitionEffect.FromRight };
        }
        int lastId = 0;
        private void OnNavigateCommand(object parameter)
        {
            if (parameter is null)
            {
                if (ContentFrame.CanGoBack)
                    ContentFrame.GoBack();
                //ContentFrame.Navigate(typeof(MapsView), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                return;
            }
            if (parameter is int id)
            {
                if (lastId == id && ContentFrame.CanGoForward)
                {
                    ContentFrame.GoForward();
                }
                else ContentFrame.Navigate(new MapDetailsView(id));
            }
        }
    }
}
