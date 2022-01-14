using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using beta.Infrastructure.Services.Interfaces;

namespace beta.Infrastructure.Services
{
    public class NavigationManager : INavigationManager
    {
        private readonly ContentControl MainFrame;
        private readonly ContentControl ModalFrame;

        public NavigationManager(ContentControl mainFrame, ContentControl modalFrame)
        {
            MainFrame = mainFrame;
            ModalFrame = modalFrame;
        }

        public async Task Navigate(UserControl userControl)
        {
            MainFrame.Content = userControl;
            if (userControl is not INavigationAware navigationAware)
                return;

            await navigationAware.OnViewChanged(this);
        }
    };
}