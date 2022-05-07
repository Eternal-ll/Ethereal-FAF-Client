using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    internal class ReplayServerService : IReplayServerService
    {
        public event EventHandler<ReplayRecorder> ReplayRecorderCreated;

        private readonly ILogger Logger;

        public ReplayServerService(ILogger<ReplayServerService> logger)
        {
            Logger = logger;
        }

        private readonly TcpListener ReplayListener = new(IPAddress.Loopback, 0);
        public int StartReplayServer()
        {
            var listener = ReplayListener;
            if (listener.Server.IsBound)
            {
                Logger.LogInformation($"Replay listener is already launched on port: {((IPEndPoint)listener.LocalEndpoint).Port}");
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            listener.Start();
            Task.Run(() => ListenReplays());
            Logger.LogInformation($"Replay listener launched on port: {((IPEndPoint)listener.LocalEndpoint).Port}");
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }

        private async Task ListenReplays()
        {
            var listener = ReplayListener;
            while (listener.Server.IsBound)
            {
                var client = await listener.AcceptTcpClientAsync();
                Logger.LogInformation($"Received game replay translation connection");

                ReplayRecorder recorder = new(client);
                ReplayRecorderCreated?.Invoke(this, recorder);
            }
        }

        public void StopReplayServer() => ReplayListener.Stop();
    }
    internal class ReplayRecorders
    {

    }
}
