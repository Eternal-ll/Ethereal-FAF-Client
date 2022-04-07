using beta.ViewModels;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for NavigationView.xaml
    /// </summary>
    public partial class NavigationView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));

        #region ViewModel
        private NavigationViewModel _ViewModel;
        public NavigationViewModel ViewModel
        {
            get => _ViewModel;
            set
            {
                if (!Equals(value, _ViewModel))
                {
                    _ViewModel = value;
                    OnPropertyChanged(nameof(ViewModel));
                }
            }
        } 
        #endregion

        public NavigationView()
        {
            InitializeComponent();

            DataContextChanged += NavigationView_DataContextChanged;
            Profile.Content = Properties.Settings.Default.PlayerNick;
            return;



            //// TODO rewrite
            //if (string.IsNullOrWhiteSpace(Properties.Settings.Default.PathToGame))
            //{
            //    var steamPath = @"C:\Program Files (x86)\Steam\SteamApps\Supreme Commander Forged Alliance";

            //    if (!Directory.Exists(steamPath))
            //    {
            //        Dialog = new ContentDialog();

            //        Dialog.PreviewKeyDown += (s, e) =>
            //        {
            //            if (e.Key == System.Windows.Input.Key.Escape)
            //            {
            //                e.Handled = true;
            //            }
            //        };
            //        Dialog.Content = new SelectPathToGameView(Dialog);
            //        Dialog.ShowAsync();
            //    }
            //    else
            //    {
            //        // TODO INVOKE SOMETHING AND SHOW TO USER
            //        Properties.Settings.Default.PathToGame = steamPath;
            //    }   
            //}
        }

        private void NavigationView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is NavigationViewModel viewModel)
            {
                ViewModel = viewModel;
                DataContext = this;
            }
        }
        private readonly Dictionary<Type, UserControl> Pages = new()
        {
            { typeof(ChatControlView), null},
            { typeof(GlobalGamesView), null },
            { typeof(ProfileView), null },
            { typeof(SettingsView), null },
            { typeof(MapsView), null },
        };

        private void OnNavigationViewSelectionChanged(ModernWpf.Controls.NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;

            string selectedItemTag = (string)selectedItem.Tag;

            if (selectedItemTag is null) return;

            if (selectedItemTag == "Logout")
            {
                // TODO
                return;
            }

            string pageName = "beta.Views." + selectedItemTag + "View";
            Type viewType = typeof(GlobalView).Assembly.GetType(pageName);

            UserControl view = null;

            if (Pages.TryGetValue(viewType, out var viewg))
            {
                if (viewg is null)
                {
                    view = (UserControl)Activator.CreateInstance(viewType);
                    Pages[viewType] = view;
                }
                else
                {
                    view = viewg;
                }
            }
            else
            {
                view = (UserControl)Activator.CreateInstance(viewType);
            }

            //var view = Activator.CreateInstance(viewType);

            ContentFrame.Content = view;
        } 
    }
}
