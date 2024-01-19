using AsyncAwaitBestPractices;
using Ethereal.FAF.UI.Client.Infrastructure.Api;
using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using FAF.Domain.LobbyServer.Converters;
using Microsoft.Extensions.Logging;
using NetCoreServer;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Websocket.Client;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class LobbyConnectionViewModel: Base.ViewModel
    {
        private readonly ClientManager _clientManager;
        private readonly IFafUserApi _fafUserApi;
        private readonly IFafAuthService _fafAuthService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly IFafLobbyService _lobbyService;
        private readonly ILogger<WebsocketClient> _logger;

        private ITransportClient TcpTransportClient;

        public LobbyConnectionViewModel(ClientManager clientManager, IFafAuthService fafAuthService, IFafLobbyService lobbyService, IFafUserApi fafUserApi, ILogger<WebsocketClient> logger)
        {
            ConnectCommand = new LambdaCommand(OnConnectCommand, CanConnectCommand);
            DisconnectCommand = new LambdaCommand(OnDisconnectCommand, CanDisconnectCommand);
            _jsonSerializerOptions = new();
            _jsonSerializerOptions.Converters.Add(new LobbyMessageJsonConverter());
            _clientManager = clientManager;
            _fafAuthService = fafAuthService;
            _lobbyService = lobbyService;
            _fafUserApi = fafUserApi;
            _logger = logger;
        }

        public override async Task OnLoadedAsync()
        {
            //await _lobbyService.ConnectAsync();
        }

        private CancellationTokenSource CancellationTokenSource;

        #region ConnectCommand
        public ICommand ConnectCommand { get; set; }
        private bool CanConnectCommand(object arg) => true;
        private async void OnConnectCommand(object arg)
        {
            await _lobbyService.ConnectAsync(default);
            return;
            CancellationTokenSource = new();
            Task.Run(async () =>
            {
                return;
                var access = await _fafUserApi.GetLobbyAccess(CancellationTokenSource.Token);
                _logger.LogInformation(access.AccessUrl.ToString());

                #region ClientWebsocket 2
                //using (var client = new ClientWebSocket())
                //{
                //    var uri = access.AccessUrl;
                //    client.Options.SetRequestHeader("Host", uri.Host);

                //    await client.ConnectAsync(uri, CancellationToken.None);

                //    Console.WriteLine("Connected to websocket!");

                //    var buffer = new byte[1024];
                //    var segment = new ArraySegment<byte>(buffer);
                //    var sent = false;
                //    while (client.State == WebSocketState.Open)
                //    {
                //        if (!sent)
                //        {
                //            var msg = "{\"user_agent\":\"ethereal-faf-client\",\"version\":\"2.2.0\",\"command\":\"ask_session\"}\r\n";
                //            await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Binary, true, CancellationTokenSource.Token);
                //            sent = true;
                //        }
                //        else
                //        {
                //            var result = await client.ReceiveAsync(segment, CancellationToken.None);

                //            if (result.MessageType == WebSocketMessageType.Text)
                //            {
                //                var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                //                Console.WriteLine($"Received message: {message}");
                //            }
                //            sent = false;
                //        }
                //        await Task.Delay(1000);
                //    }

                //    Console.WriteLine("Disconnected from websocket.");
                //}
                #endregion

                #region ClientWebSocket
                //using (var client = new ClientWebSocket())
                //{
                //    await client.ConnectAsync(access.AccessUrl, CancellationTokenSource.Token);
                //    var state = client.State;
                //    await Task.Delay(5000);
                //    var bytes = new byte[1024];
                //    var result = await client.ReceiveAsync(bytes, CancellationTokenSource.Token);
                //    string res = Encoding.UTF8.GetString(bytes, 0, result.Count);
                //    await client.SendAsync(Encoding.UTF8.GetBytes("{\"user_agent\":\"ethereal-faf-client\",\"version\":\"2.2.0\",\"command\":\"ask_session\"}\r\n"), WebSocketMessageType.Text, WebSocketMessageFlags.None, CancellationTokenSource.Token);
                //    while (!CancellationTokenSource.IsCancellationRequested)
                //    {
                //        bytes = new byte[1024];
                //        result = await client.ReceiveAsync(bytes, CancellationTokenSource.Token);
                //        res = Encoding.UTF8.GetString(bytes, 0, result.Count);
                //    }
                //    CancellationTokenSource = null;
                //}
                #endregion

                #region WebsocketClient
                //var factory = new Func<ClientWebSocket>(() =>
                //        {
                //            var client = new ClientWebSocket
                //            {
                //                Options =
                //                {
                //                    Proxy= null,
                //                    RemoteCertificateValidationCallback =
                //                    new((a,b,c,d) => true),
                //                    KeepAliveInterval = TimeSpan.FromSeconds(5),
                //                }
                //            };
                //            return client;
                //        });
                //using (var client = new WebsocketClient(access.AccessUrl, _logger, factory))
                //{
                //    client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                //    client.MessageReceived.Subscribe(msg => Console.WriteLine(msg.Text));
                //    await client.StartOrFail();
                //    client.IsReconnectionEnabled = false;
                //    client.DisconnectionHappened.Subscribe(async x =>
                //    {
                //        if (CancellationTokenSource == null) return;
                //        var lobbyAccess = await _fafUserApi.GetLobbyAccess(CancellationTokenSource.Token);
                //        client.Url = lobbyAccess.AccessUrl;
                //        _logger.LogInformation(lobbyAccess.AccessUrl.ToString());
                //        await client.Reconnect();
                //    });

                //    Task.Run(() => client.Send("{\"user_agent\":\"ethereal-faf-client\",\"version\":\"2.2.0\",\"command\":\"ask_session\"}\r\n"))
                //    .SafeFireAndForget();
                //    while (!CancellationTokenSource.IsCancellationRequested)
                //    {
                //        await Task.Delay(1000);
                //    }
                //    CancellationTokenSource = null;
                //}
                #endregion
            }).SafeFireAndForget(x =>
            {
                _logger.LogError(x.ToString());
            });
        }

        private void TcpTransportClient_ConnectionStateChange(object sender, ConnectionState e)
        {

        }

        private void TcpTransportClient_SocketError(object sender, SocketError e)
        {

        }
        #endregion

        #region DisconnectCommand
        public ICommand DisconnectCommand { get; set; }
        private bool CanDisconnectCommand(object arg) => CancellationTokenSource != null &&
            !CancellationTokenSource.IsCancellationRequested;
        private async void OnDisconnectCommand(object arg)
        {
            await _lobbyService.DisconnectAsync(default);
        }
        #endregion
    }
}
