using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface INavigationManager
    {
        public Task Navigate(UserControl userControl);
    }

    public interface INavigationAware
    {
        public Task OnViewChanged(INavigationManager navigationManager);
    }
}
