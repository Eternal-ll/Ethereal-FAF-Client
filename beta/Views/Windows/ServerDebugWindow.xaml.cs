using System;
using System.Windows;

namespace beta.Views.Windows
{
    /// <summary>
    /// Interaction logic for ServerDebugWindow.xaml
    /// </summary>
    public partial class ServerDebugWindow : Window
    {
        public ServerDebugWindow()
        {
            InitializeComponent();
        }
        public void LOGLobby(string data)
        {
            Dispatcher.Invoke(() =>
            {
                ItemsControl.Items.Add(data);
            });

        }
        public void LOGIRC(string data)
        {
            Dispatcher.Invoke(() =>
            {
                IRCItemsControl.Items.Add(data);
            });

        }

        internal void LOG(object p)
        {
            throw new NotImplementedException();
        }
    }
}
