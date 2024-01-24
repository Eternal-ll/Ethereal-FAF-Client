using CommunityToolkit.Mvvm.ComponentModel;
using Ethereal.FAF.UI.Client.Views;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public partial class NavigationViewModel : Base.ViewModel
    {
        public NavigationViewModel()
        {
            LoadMenuItems();
        }

        private void LoadMenuItems()
        {

            MenuItems = new()
            {
                new NavigationViewItem("Play", SymbolRegular.XboxController24, typeof(PlayTabPage))
                {
                    MenuItems = new NavigationViewItem[]
                    {
                        new("Custom", SymbolRegular.XboxController24, typeof(GamesView)),
                    }
                },
                new NavigationViewItem("Players", SymbolRegular.PeopleList24, typeof(PlayersView)),
            };
            FooterMenuItems = new()
            {

            };
        }

        [ObservableProperty]
        private ObservableCollection<object> _MenuItems;
        [ObservableProperty]
        private ObservableCollection<object> _FooterMenuItems;
    }
}
