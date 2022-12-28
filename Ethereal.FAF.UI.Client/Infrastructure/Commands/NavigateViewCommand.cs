using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Mvvm.Contracts;

namespace Ethereal.FAF.UI.Client.Infrastructure.Commands
{
    internal class NavigateViewCommand : Base.Command
    {
        private INavigationService NavigationService;
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            NavigationService ??= App.Hosting.Services.GetService<INavigationService>();
            switch (parameter)
            {
                case System.Type pageType: NavigationService.Navigate(pageType); break;
                default: break;
            }
        }
    }
}