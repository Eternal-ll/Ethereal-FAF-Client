using beta.ViewModels;
using beta.ViewModels.Base;
using ModernWpf.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for NavigationView.xaml
    /// </summary>
    public partial class NavigationView
    {
        public NavigationView()
        {
            InitializeComponent();
            InitializeViews();
        }
        /// <summary>
        /// Initialize and cache main views
        /// </summary>
        private void InitializeViews()
        {
            Cache = new()
            {
                { typeof(HomeView), new HomeView() },
                { typeof(CustomGamesView),
                    new CustomGamesView()
                {
                    DataContext = new CustomGamesViewModel(NavigationFrame.NavigationService)
                }
                },
                { typeof(MatchMakerView), new MatchMakerView() },
                { typeof(MapsView), new MapsView(NavigationFrame.NavigationService) },
                { typeof(DownloadsView), new DownloadsView() },
                { typeof(UserProfileView), new UserProfileView() },
                { typeof(SettingsView), new SettingsView() },
            };
        }

        private Dictionary<Type, UserControl> Cache;

        private void OnNavigationViewSelectionChanged(ModernWpf.Controls.NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var tag = ((NavigationViewItem)args.SelectedItem).Tag;
            if (tag is null) return;
            switch (tag)
            {
                case Type type:
                    {
                        if (Cache.TryGetValue(type, out var cachedView))
                        {
                            NavigationFrame.Navigate(cachedView);
                        }
                        else
                        {
                            NavigationFrame.Navigate(type);
                        }
                        return;
                    }
                case string text:
                    if (text == "Logout")
                    {

                    }
                    break;
            }
            //((NavigationViewModel)DataContext).CurrentViewTag = ((NavigationViewItem)args.SelectedItem).Tag?.ToString();
        }
    }
}
