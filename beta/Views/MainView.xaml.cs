using beta.Infrastructure.Navigation;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Enums;
using beta.Views.Modals;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : INavigationAware, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));

        #region NavigationManager

        private INavigationManager NavigationManager;
        public void OnViewChanged(INavigationManager navigationManager) => NavigationManager = navigationManager;
        #endregion

        private readonly IIrcService IrcService;

        private ContentDialog Dialog;

        private IrcState _IrcState;
        public IrcState IrcState
        {
            get => _IrcState;
            set
            {
                if (!Equals(value, _IrcState))
                {
                    _IrcState = value;
                    OnPropertyChanged(nameof(IrcState));
                }
            }
        }

        #region Ctor
        public MainView()
        {
            InitializeComponent();
            DataContext = this;

            Profile.Content = Properties.Settings.Default.PlayerNick;

            IrcService = App.Services.GetService<IIrcService>();
            IrcService.StateChanged += (s, e) => IrcState = e;

            IrcState = IrcService.State;

            Pages = new UserControl[]
            {
                new HomeView(),
                new ChatView(),
                new GlobalView(),
                new AnalyticsView(),
                new SettingsView(),
                new MapsView()
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
