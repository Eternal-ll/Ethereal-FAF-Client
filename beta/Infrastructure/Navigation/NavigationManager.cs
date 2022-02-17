using System.Windows.Controls;

namespace beta.Infrastructure.Navigation
{
    public class NavigationManager : INavigationManager
    {
        private readonly ContentControl MainFrame;
        private readonly ContentControl ModalFrame;

        public NavigationManager(ContentControl mainFrame) => MainFrame = mainFrame;
        public NavigationManager(ContentControl mainFrame, ContentControl modalFrame)
        {
            MainFrame = mainFrame;
            ModalFrame = modalFrame;
        }

        public void Navigate(UserControl userControl)
        {
            MainFrame.Content = userControl;
            if (userControl is not INavigationAware navigationAware)
                return;

            navigationAware.OnViewChanged(this);
        }
    };
}