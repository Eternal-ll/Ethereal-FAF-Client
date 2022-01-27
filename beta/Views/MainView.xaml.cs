using beta.Infrastructure.Services.Interfaces;
using beta.Views.Pages;
using ModernWpf.Controls;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : INavigationAware
    {
        #region NavigationManager

        private INavigationManager NavigationManager;
        public async Task OnViewChanged(INavigationManager navigationManager) => NavigationManager = navigationManager;
        #endregion

        #region Ctor
        public MainView()
        {
            InitializeComponent();
            Profile.Content = Properties.Settings.Default.PlayerNick;

            Pages = new UserControl[]
            {
                new ChatPage(),
                new GlobalPage(),
                new MatchmakerPage(),
                new MapsPage(),
                new ModsPage(),
                new ProfilePage(),
                new SettingsPage()
            };
        }

        #endregion
        private readonly UserControl[] Pages;
        private UserControl GetPage(Type type)
        {
            var enumerator = Pages.GetEnumerator();
            while (enumerator.MoveNext()) 
            {
                var control = (UserControl)enumerator.Current;
                if (control.GetType() == type)
                    return control;
            }
            return null;
        }

        #region OnNavigationViewSelectionChanged
        private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            string selectedItemTag = (string)selectedItem.Tag;
#if DEBUG
            if (selectedItemTag == null || selectedItemTag.Length == 0) return;
#endif
            //if (selectedItemTag == "Logout")
            //{
            //    // Initialize logout process
            //    return;
            //}

            string pageName = "beta.Views.Pages." + selectedItemTag + "Page";
            Type pageType = typeof(LobbiesPage).Assembly.GetType(pageName);
#if DEBUG
            if (pageType == null) return;
#endif
            ContentFrame.Content = GetPage(pageType);
        } 
        #endregion
    }
}
