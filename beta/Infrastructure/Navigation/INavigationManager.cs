using System.Windows.Controls;

namespace beta.Infrastructure.Navigation
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
