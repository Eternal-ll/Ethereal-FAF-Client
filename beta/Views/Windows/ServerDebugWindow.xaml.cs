using beta.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace beta.Views.Windows
{
    /// <summary>
    /// Interaction logic for ServerDebugWindow.xaml
    /// </summary>
    public partial class ServerDebugWindow : Window
    {
        private readonly IIrcService IrcService;
        public ServerDebugWindow()
        {
            InitializeComponent();
            IrcService = App.Services.GetService<IIrcService>();
        }
        public void LOGLobby(string data)
        {
            Dispatcher.Invoke(() =>
            {
                Lobby.AppendText(data + '\n');
            });
        }
        public void LOGIRC(string data)
        {
            Dispatcher.Invoke(() =>
            {
                IRC.AppendText(data + '\n');
            });
        }

        public void LOGICE(string data)
        {
            Dispatcher.Invoke(() =>
            {
                Ice.AppendText(data + '\n');
            });
        }
        public void LOGJSONRPC(string data)
        {
            Dispatcher.Invoke(() =>
            {
                IceJsonRPC.AppendText(data + '\n');
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IrcService.Test();
        }
    }
}
