using Ethereal.FAF.UI.Client.Infrastructure.Api;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    public class WebSocketTransportClient : ITransportClient
    {
        private static string _lineBreak = "\n";

        public bool IsConnecting => false;
        public bool IsConnected => _websocketClient.IsRunning;
        public ConnectionState State { get; set; }

        public event EventHandler<SocketError> SocketError;
        public event EventHandler<ConnectionState> OnState;
        public event EventHandler<byte[]> OnData;

        private readonly IFafUserApi _fafUserApi;
        private readonly WebsocketClient _websocketClient;
        private bool _connected;
        private bool _isDisconnected;

        public WebSocketTransportClient(Uri url, IFafUserApi fafUserApi, ILogger<WebsocketClient> logger = null)
        {
            _websocketClient = new(url, logger);
            _fafUserApi = fafUserApi;
            _websocketClient.IsReconnectionEnabled = false;
            _websocketClient.IsTextMessageConversionEnabled = false;
            _websocketClient.MessageReceived
                .Subscribe(x =>
                {
                    // 7240
                    // 21720
                    // 33304
                    // 44920
                    // 38535
                    // 44888
                    OnData?.Invoke(this, x.Binary);
                });
            _websocketClient.DisconnectionHappened
                .Subscribe(x =>
                {
                    _connected = false;
                    UpdateState(ConnectionState.Disconnected);
                });
        }
        private void UpdateState(ConnectionState state)
        {
            State = state;
            OnState?.Invoke(this, state);
        }

        public async Task Connect(CancellationToken cancellationToken)
        {
            UpdateState(ConnectionState.Connecting);
            var lobbyAccess = await _fafUserApi.GetLobbyAccess(cancellationToken);
            _websocketClient.Url = lobbyAccess.AccessUrl;
            await _websocketClient.StartOrFail();
            _connected = true;
            UpdateState(ConnectionState.Connected);
        }

        public async Task Disconnect(CancellationToken cancellationToken)
        {
            _isDisconnected = true;
            await _websocketClient.Stop(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Disconnect");
        }

        public bool SendData(byte[] data)
        {
            _websocketClient.Send(data.Append(Encoding.UTF8.GetBytes(_lineBreak)[0]).ToArray());
            return true;
        }

        public void Dispose()
        {
            _websocketClient.Dispose();
        }
    }
}
