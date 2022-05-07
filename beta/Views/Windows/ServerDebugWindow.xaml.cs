using beta.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace beta.Views.Windows
{
    /// <summary>
    /// Interaction logic for ServerDebugWindow.xaml
    /// </summary>
    public partial class ServerDebugWindow : Window, INotifyPropertyChanged
    {
        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        } 
        #endregion

        private readonly IIrcService IrcService;

        public ServerDebugWindow()
        {
            InitializeComponent();
            IrcService = App.Services.GetService<IIrcService>();
            DataContext = this;
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


        #region ReplayOutput
        private string _ReplayOutput;
        public string ReplayOutput
        {
            get => _ReplayOutput;
            set => Set(ref _ReplayOutput, value);
        }
        #endregion
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IrcService.Test();
        }
    }
}
