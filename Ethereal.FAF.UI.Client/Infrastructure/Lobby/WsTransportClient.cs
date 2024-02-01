using Ethereal.FAF.UI.Client.Infrastructure.Api;
using NetCoreServer;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    internal class WsInternalClient : WssClient
    {
        private readonly Uri _uri;
        public WsInternalClient(SslContext context, DnsEndPoint endpoint, Uri uri) : base(context, endpoint)
        {
            _uri = uri;
        }

        public event EventHandler<byte[]> OnData;
        public event EventHandler Connected;
        public override void OnWsConnecting(HttpRequest request)
        {
            request.SetBegin("GET", _uri.PathAndQuery);
            request.SetHeader("Host", _uri.Host);
            //request.SetHeader("Host", "localhost");
            //request.SetHeader("Origin", "https://localhost");
            request.SetHeader("Upgrade", "websocket");
            request.SetHeader("Connection", "Upgrade");
            request.SetHeader("Sec-WebSocket-Key", Convert.ToBase64String(WsNonce));
            //request.SetHeader("Sec-WebSocket-Protocol", "chat, superchat");
            request.SetHeader("Sec-WebSocket-Version", "13");
            request.SetBody();
        }
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            Console.WriteLine("OnReceived");
            var t = Encoding.UTF8.GetString(buffer);
            Console.WriteLine(t);
            base.OnReceived(buffer, offset, size);
        }
        protected override void OnSent(long sent, long pending)
        {
            base.OnSent(sent, pending);
        }
        protected override void OnError(SocketError error)
        {
            base.OnError(error);
        }
        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            buffer = buffer.AsSpan().Slice((int)offset, (int)size).ToArray();
            Console.WriteLine("OnWsReceived");
            var t = Encoding.UTF8.GetString(buffer);
            Console.WriteLine(t);
            OnData?.Invoke(this, buffer);
        }
        public override void OnWsConnected(HttpRequest request)
        {
            Console.WriteLine("OnWsConnected");
            base.OnWsConnected(request);
        }
        public override void OnWsDisconnected()
        {
            Console.WriteLine("OnWsDisconnected");
            base.OnWsDisconnected();
        }
        protected override void OnDisconnected()
        {
            Console.WriteLine("OnDisconnected");
            base.OnDisconnected();
        }
        protected override void OnDisconnecting()
        {
            Console.WriteLine("OnDisconnecting");
            base.OnDisconnecting();
        }
        protected override void OnConnected()
        {
            Console.WriteLine("OnConnected");
            base.OnConnected();
        }
        protected override void OnConnecting()
        {
            Console.WriteLine("OnConnecting");
            base.OnConnecting();
        }
        public override void OnWsConnected(HttpResponse response)
        {
            Console.WriteLine("OnWsConnected");
            Connected?.Invoke(this, EventArgs.Empty);
            base.OnWsConnected(response);
            ReceiveAsync();
        }
        public override bool OnWsConnecting(HttpRequest request, HttpResponse response)
        {
            Console.WriteLine("OnWsConnecting");
            return base.OnWsConnecting(request, response);
        }
        public override void OnWsDisconnecting()
        {
            Console.WriteLine("OnWsDisconnecting");
            base.OnWsDisconnecting();
        }
        public override void OnWsError(SocketError error)
        {
            base.OnWsError(error);
        }
        public override void OnWsError(string error)
        {
            base.OnWsError(error);
        }
        protected override void OnReceivedResponse(HttpResponse response)
        {
            base.OnReceivedResponse(response);
        }
        protected override void OnReceivedResponseError(HttpResponse response, string error)
        {
            base.OnReceivedResponseError(response, error);
        }
        protected override void OnReceivedResponseHeader(HttpResponse response)
        {
            base.OnReceivedResponseHeader(response);
        }
    }
    internal class WsTransportClient : ITransportClient
    {
        private static byte _lineBreak = Encoding.UTF8.GetBytes("\n")[0];
        private readonly IFafUserApi _fafUserApi;

        private WsInternalClient _client;

        public WsTransportClient(IFafUserApi fafUserApi)
        {
            _fafUserApi = fafUserApi;
        }

        public ConnectionState State { get; set; }

        public bool IsConnecting => _client?.IsConnecting ?? false;
        public bool IsConnected => _client?.IsConnected ?? false;

        public event EventHandler<SocketError> SocketError;
        public event EventHandler<ConnectionState> OnState;
        public event EventHandler<byte[]> OnData;

        private async Task<WsInternalClient> GetClient(CancellationToken cancellationToken)
        {
            var access = await _fafUserApi.GetLobbyAccess(cancellationToken);
            var context = new SslContext(SslProtocols.Tls12 | SslProtocols.Tls13);
            return new(
                context,
                new DnsEndPoint(access.AccessUrl.DnsSafeHost, access.AccessUrl.Port, AddressFamily.InterNetwork),
                access.AccessUrl);
        }

        public async Task Connect(CancellationToken cancellationToken = default)
        {
            var client = await GetClient(cancellationToken);
            if (_client != null)
            {
                _client.Disconnect();
                _client.OnData -= OnData;
                _client.Dispose();
            }
            _client = client;
            _client.OnData += OnData;
            _client.Connected += _client_Connected;
            _client.ConnectAsync();
            while (!_client.IsConnected)
            {
                await Task.Delay(100, cancellationToken);
            }
        }
        private void SetState(ConnectionState state)
        {
            State = state;
            OnState?.Invoke(this, state);
        }

        private void _client_Connected(object sender, EventArgs e)
        {
            SetState(ConnectionState.Connected);
        }

        public Task Disconnect(CancellationToken cancellationToken = default)
        {
            _client.Disconnect();
            return Task.CompletedTask;
        }

        public Task Reconnect(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool SendData(byte[] data)
        {
            Array.Resize(ref data, data.Length + 2);
            data[^2] = Encoding.UTF8.GetBytes("\r")[0];
            data[^1] = Encoding.UTF8.GetBytes("\n")[0];
            var command = Encoding.UTF8.GetString(data);
            //var result =  _client.SendTextAsync(command);
            //_client.SendBinary(data);
            //_client.SendBinaryAsync(data);
            _client.SendText(@"{""user_agent"":""ethereal-faf-client"",""version"":""2.2.0"",""command"":""ask_session""}
");
            var buffer = new byte[4096];
            var result = _client.Receive(buffer);
            //_client.Send(data);
            //_client.SendAsync(data);
            //_client.Socket.Send(data);
            //Task.Run(async () => await _client.Socket.SendAsync(data)).SafeFireAndForget();
            return true;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
