using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    //public class WsTransportClient : WsClient, ITransportClient
    //{
    //    public event EventHandler<SocketError> SocketError;
    //    public event EventHandler<ConnectionState> ConnectionStateChange;
    //    public event EventHandler<byte[]> DataReceived;

    //    private readonly List<byte> _cache = new();
    //    private readonly Uri _wssConnectUrl;

    //    public WsTransportClient(DnsEndPoint endPoint, Uri wssConnectUrl, int optionReceiveBufferSize = 4096) : base(endPoint)
    //    {
    //        OptionReceiveBufferSize = optionReceiveBufferSize;
    //        _wssConnectUrl = wssConnectUrl;
    //    }
    //    bool ITransportClient.Connect()
    //    {
    //        return Connect();
    //    }

    //    bool ITransportClient.Disconnect()
    //    {
    //        return Disconnect();
    //    }
    //    protected override void OnConnected()
    //    {
    //        base.OnConnected();
    //    }
    //    protected override void OnConnecting()
    //    {
    //        base.OnConnecting();
    //    }
    //    public override void OnWsConnecting(HttpRequest request)
    //    {
    //        // wss://ws.faforever.com/?verify=1705253713-wKc904Bosc%2BzWZ84trUr%2BCSl8MA9gCYUXQtZ9BRvI10%3D
    //        // https://ws.faforever.com/?verify=1705253713-wKc904Bosc%2BzWZ84trUr%2BCSl8MA9gCYUXQtZ9BRvI10%3D
    //        ConnectionStateChange?.Invoke(this, ConnectionState.Connecting);
    //        request.SetBegin("GET", request.ToString());
    //        request.SetHeader("Host", "ws.faforever.com");
    //        request.SetHeader("Upgrade", "websocket");
    //        request.SetHeader("Connection", "Upgrade");
    //        request.SetHeader("Sec-WebSocket-Key", Convert.ToBase64String(WsNonce));
    //        request.SetHeader("Sec-WebSocket-Version", "13");
    //        request.SetHeader("Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits");
    //        request.SetBody();
    //        base.OnWsConnecting(request);
    //    }
    //    public override void OnWsPing(byte[] buffer, long offset, long size)
    //    {
    //        base.OnWsPing(buffer, offset, size);
    //    }
    //    public override void OnWsPong(byte[] buffer, long offset, long size)
    //    {
    //        base.OnWsPong(buffer, offset, size);
    //    }
    //    public override void OnWsConnected(HttpResponse response)
    //    {
    //        ConnectionStateChange?.Invoke(this, ConnectionState.Connected);
    //    }
    //    public override void OnWsDisconnecting() => ConnectionStateChange?.Invoke(this, ConnectionState.Disconnecting);
    //    public override void OnWsDisconnected() => ConnectionStateChange?.Invoke(this, ConnectionState.Disconnected);
    //    public override void OnWsConnected(HttpRequest request)
    //    {
    //        base.OnWsConnected(request);
    //    }
    //    public override bool OnWsConnecting(HttpRequest request, HttpResponse response)
    //    {
    //        return base.OnWsConnecting(request, response);
    //    }
    //    public override void OnWsReceived(byte[] buffer, long offset, long size)
    //    {
    //        if (_cache.Count == 0 && buffer[0] == '{' && buffer[^1] == '\n')
    //        {
    //            DataReceived?.Invoke(this, buffer);
    //            return;
    //        }
    //        for (int i = (int)offset; i < (int)size; i++)
    //        {
    //            if (buffer[i] == '\n')
    //            {
    //                DataReceived?.Invoke(this, _cache.ToArray());
    //                _cache.Clear();
    //                continue;
    //            }
    //            _cache.Add(buffer[i]);
    //        }
    //    }
    //    protected override void OnError(SocketError error)
    //    {
    //        base.OnError(error);
    //    }
    //    public override void OnWsError(string error)
    //    {
    //        base.OnWsError(error);
    //    }
    //    public override void OnWsError(SocketError error)
    //    {
    //        base.OnWsError(error);
    //    }
    //    protected override void OnReceivedResponseError(HttpResponse response, string error)
    //    {
    //        base.OnReceivedResponseError(response, error);
    //    }

    //    bool ITransportClient.Send(string text)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    bool ITransportClient.Send(byte[] data)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
