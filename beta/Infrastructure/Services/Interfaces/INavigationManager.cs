using System.Windows.Controls;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface INavigationManager
    {
        public void Navigate(UserControl userControl);
    }

    public interface INavigationAware
    {
        public void OnViewChanged(INavigationManager navigationManager);
    }
}
