using CommunityToolkit.Mvvm.Input;
using System;
using Wpf.Ui;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public partial class PlayTabViewModel : Base.ViewModel
    {
        private readonly INavigationService _navigationService;

        public PlayTabViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }
        [RelayCommand]
        private void NavigatePage(Type page)
        {
            _navigationService.Navigate(page);
        }
    }
}
