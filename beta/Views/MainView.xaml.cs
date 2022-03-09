using beta.Infrastructure.Navigation;
using beta.Views.Modals;
using ModernWpf.Controls;
using System;
using System.IO;
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
        public void OnViewChanged(INavigationManager navigationManager) => NavigationManager = navigationManager;
        #endregion

        private ContentDialog Dialog;

        #region Ctor
        public MainView()
        {
            InitializeComponent();
            Profile.Content = Properties.Settings.Default.PlayerNick;

            Pages = new UserControl[]
            {
                new HomeView(),
                new ChatView(),
                new GlobalView(),
                new AnalyticsView(),
                new SettingsView(),
            };

            // TODO rewrite
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.PathToGame))
            {
                var defaultPath = @"C:\Program Files (x86)\Steam\SteamApps\Supreme Commander Forged Alliance\bin\SupremeCommander.exe";

                if (!File.Exists(defaultPath))
                {
                    Dialog = new ContentDialog();

                    Dialog.PreviewKeyDown += (s, e) =>
                    {
                        if (e.Key == System.Windows.Input.Key.Escape)
                        {
                            e.Handled = true;
                        }
                    };
                    Dialog.Content = new SelectPathToGameView(Dialog);
                    Dialog.ShowAsync();
                }
                else
                {
                    // TODO INVOKE SOMETHING AND SHOW TO USER
                    Properties.Settings.Default.PathToGame = defaultPath;
                }   
            }
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

            string pageName = "beta.Views." + selectedItemTag + "View";
            Type pageType = typeof(GlobalView).Assembly.GetType(pageName);

            ContentFrame.Content = GetPage(pageType);
        } 
        #endregion
    }
}
