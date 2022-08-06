using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Ethereal.FAF.LobbyServer
{
    public static class BuilderExtensions
    {
        public static IServiceCollection AddLobbyServer(this IServiceCollection services) => services
            .AddScoped<CommandManager>()
            .AddSingleton<LobbyServer>()
            ;
    }
    public class LobbyServer
    {
        public event EventHandler<string> ServerLaunched;
        public readonly ConcurrentDictionary<string, TcpClient> Clients = new();
        
        private readonly ILogger Logger;
        private readonly int Port;

        private readonly CommandManager CommandManager;
        public TcpListener Server;
        public LobbyServer(ILogger<LobbyServer> logger, CommandManager commandManager)
        {
            Logger = logger;
            CommandManager = commandManager;
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            if (Server is not null)
            {
                Server.Stop();
            }
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            TcpListener server = new TcpListener(localAddr, Port);
            Server = server;
            server.Start();
            ServerLaunched?.Invoke(this, server.LocalEndpoint.ToString());
            while (server.Server.IsBound)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                Logger.LogTrace("Ожидание подключений... ");
                var client = await server.AcceptTcpClientAsync();
                Logger.LogTrace("Подключен клиент [{client}]...", client.Client.RemoteEndPoint?.ToString());
                Task.Run(async () =>
                {
                    NetworkStream stream = client.GetStream();
                    List<byte> data = new();
                    while (client.Connected)
                    {
                        byte[] buffer = new byte[1];
                        await stream.ReadAsync(buffer.AsMemory(0, 1));
                        if (buffer[0] == 0) continue;
                        if (buffer[0] == 10)
                        {
                            await Received(client, Encoding.UTF8.GetString(data.ToArray()))
                            .ContinueWith(task =>
                            {
                                if (task.IsFaulted)
                                {
                                    Logger.LogWarning("Клиент отключен [{client}], exception: [{exception}]", client.Client.RemoteEndPoint?.ToString(), task.Exception);
                                }
                            });
                            data.Clear();
                        }
                        data.Add(buffer[0]);
                    }
                    Logger.LogTrace("Клиент отключен [{client}]...", client.Client.RemoteEndPoint?.ToString());
                });
                await Task.Delay(500);
            }
            server.Stop();
        }
        public async Task Received(TcpClient client, string json)
        {
            // {"command": "ask_session", "version": "0.20.1+12-g2d1fa7ef.git", "user_agent": "ethereal-faf-client"}
            Logger.LogTrace("Клиент [{client}]... Запрос: [{json}]", client.Client.RemoteEndPoint?.ToString(), json);
            //TimeOutWatcher.Restart();
            var commandText = json.GetRequiredJsonRowValue();
            if (Enum.TryParse<ServerCommand>(commandText, out var command))
            {
                var stream = client.GetStream();
                using StreamWriter sr = new StreamWriter(stream);
                await sr.WriteLineAsync(await CommandManager.UniversalAsync(command, json));
            }
            else Logger.LogTrace("Клиент [{client}]... Запрос: [{json}] не известен", client.Client.RemoteEndPoint?.ToString(), commandText);
        }
    }
}
