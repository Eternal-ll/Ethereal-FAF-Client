using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using FAF.Domain.LobbyServer;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace Ethereal.FAF.UI.Client.Views
{
    /// <summary>
    /// Interaction logic for MatchView.xaml
    /// </summary>
    public sealed partial class MatchView : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<ConnectionState> Connections { get; set; } = new();
        public event PropertyChangedEventHandler PropertyChanged;
        public MatchView(IceManager IceManager, GameInfoMessage game)
        {
            Connections = new();
            DataContext = this;
            IceManager.Initialized += IceManager_Initialized;
            InitializeComponent();
        }

        private void IceManager_Initialized(object sender, System.EventArgs e)
        {
            ((IceManager)sender).IceClient.ConnectionStateChanged += IceClient_ConnectionStateChanged;
        }

          private void IceClient_ConnectionStateChanged(object sender, ConnectionState e)
        {
            Dispatcher.Invoke(() =>
            {
                var old = Connections.FirstOrDefault(c => c.RemotePlayerId == e.RemotePlayerId);
                Connections.Remove(old);
                Connections.Add(e);
            });
        }
    }
}
