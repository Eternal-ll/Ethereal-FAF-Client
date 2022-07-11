using beta.Infrastructure.Commands;
using beta.Infrastructure.Services;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for UnitsDatabasesView.xaml
    /// </summary>
    public partial class UnitsDatabasesView : UserControl
    {
        private readonly NavigationService NavigationService;
        public UnitsDatabasesView(NavigationService navigationService)
        {
            NavigateCommand = new LambdaCommand(OnNavigateCommand);
            DataContext = this;
            NavigationService = navigationService;
            InitializeComponent();
        }
        public ICommand NavigateCommand { get; private set; }
        private void OnNavigateCommand(object parameter)
        {
            if (parameter is string url)
            {
                NavigationService.Navigate(new Uri(url));
            }
        }
    }
}
