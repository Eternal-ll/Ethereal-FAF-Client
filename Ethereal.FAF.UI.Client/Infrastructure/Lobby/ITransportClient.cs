using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    public interface ITransportClient : IDisposable
    {
        public event EventHandler<SocketError> SocketError;
        public event EventHandler<ConnectionState> OnState;
        public event EventHandler<byte[]> OnData;
        public ConnectionState State { get; set; }
        public bool IsConnecting { get; }
        public bool IsConnected { get; }
        public Task Connect(CancellationToken cancellationToken = default);
        public Task Disconnect(CancellationToken cancellationToken = default);
        public bool SendData(byte[] data);
    }
}
