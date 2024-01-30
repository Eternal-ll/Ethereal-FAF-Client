using AsyncAwaitBestPractices;
using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Progress;
using FAF.Domain.LobbyServer;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Contrib.WaitAndRetry;
using StreamJsonRpc;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    /// <summary>
    /// Proxy class to work with FAF Java ICE Adapter
    /// </summary>
    internal class FafJavaIceAdapter : IGameNetworkAdapter
    {
        private readonly IJavaRuntime _javaRuntime;
        private readonly IFafAuthService _fafAuthService;
        private readonly ISettingsManager _settingsManager;
        private readonly IDownloadService _downloadService;
        private readonly IFafLobbyEventsService _fafLobbyEventsService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFafJavaIceAdapterCallbacks _fafJavaIceAdapterCallbacks;
        private readonly ILogger _logger;

        private Process FafIceAdapterProcess;

        private TcpClient FafIceAdapterTcpClient;
        private IFafJavaIceAdapterClient FafJavaIceAdapterClient;
        private JsonRpc JsonRpc;

        public FafJavaIceAdapter(
            IJavaRuntime javaRuntime,
            IFafAuthService fafAuthService,
            ISettingsManager settingsManager,
            IDownloadService downloadService,
            ILogger<FafJavaIceAdapter> logger,
            IFafLobbyEventsService fafLobbyEventsService,
            IServiceProvider serviceProvider,
            IFafJavaIceAdapterCallbacks fafJavaIceAdapterCallbacks)
        {
            fafLobbyEventsService.IceUniversalDataReceived2 += FafLobbyEventsService_IceUniversalDataReceived2;
            _javaRuntime = javaRuntime;
            _fafAuthService = fafAuthService;
            _settingsManager = settingsManager;
            _downloadService = downloadService;
            _fafLobbyEventsService = fafLobbyEventsService;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _fafJavaIceAdapterCallbacks = fafJavaIceAdapterCallbacks;
        }

        public async Task<int> Run(long gameId, string mode, IProgress<ProgressReport> progress = null, CancellationToken cancellationToken = default)
        {
            var pathToJava = await _javaRuntime.EnsurJavaRuntimeExist(progress, cancellationToken);
            var pathToAdapter = await EnsureFafJavaAdapterExistAsync(progress, cancellationToken);

            _logger.LogInformation("Searching for free ports...");
            var rpcPort = NetworkHelper.GetAvailablePort();
            var gpgnetPort = NetworkHelper.GetAvailablePort();
            _logger.LogInformation("Rpc port: [{port}]", rpcPort);
            _logger.LogInformation("GPGNet port: [{port}]", gpgnetPort);
            FafIceAdapterProcess = new()
            {
                StartInfo = new()
                {
                    FileName = pathToJava,
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    UseShellExecute = false,
                    Arguments = IceAdapterArguments.Generate(pathToAdapter)
                        .WithPlayerId(_fafAuthService.GetUserId())
                        .WithPlayerLogin(_fafAuthService.GetUserName())
                        .WithRpcPort(rpcPort)
                        .WithGPGNetPort(gpgnetPort)
                        .WithGameId(gameId, true)
                        .WithForcedRelay(false)
                        //.Append("--debug-window")
                        .ToString(),
                }
            };
            _logger.LogInformation("Starting Java Ice Adapter with args: [{args}]", FafIceAdapterProcess.StartInfo.Arguments);
            var started = FafIceAdapterProcess.Start();
            if (!started)
            {
                throw new InvalidOperationException("Failed to launch FAF Java Ice Adapter");
            }

            var apiResult = await _serviceProvider
                .GetService<IFafApiClient>()
                .GetCoturnServersAsync(cancellationToken);
            var iceServers = apiResult.Data
                .Where(x => x.Attributes.Active)
                .Select(x => new IceServer()
                {
                    Credential = x.Attributes.Credential,
                    CredentialType = x.Attributes.CredentialType,
                    Urls = x.Attributes.Urls,
                    Username = x.Attributes.Username
                })
                .ToList();

            FafIceAdapterTcpClient = new();

            var attempts = 50;
            var delays = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(100), attempts, null, true);
            var attempt = 1;
            foreach (var delay in delays)
            {
                try
                {
                    await FafIceAdapterTcpClient.ConnectAsync("127.0.0.1", rpcPort);
                }
                catch(Exception ex)
                {
                    _logger.LogWarning(ex.Message);
                }
                    //.ContinueWith(x => _logger.LogWarning(x.Exception.InnerException.Message), TaskContinuationOptions.OnlyOnFaulted);
                if (FafIceAdapterTcpClient.Connected)
                {
                    _logger.LogInformation(
                        "Connected to RPC service on [{attempt}] attempt",
                        attempt);
                    break;
                }
                await Task.Delay(delay, cancellationToken);
                attempt++;
            }

            var rpc = SetupJsonRpc(FafIceAdapterTcpClient.GetStream());
            rpc.StartListening();
            
            FafJavaIceAdapterClient = rpc.Attach<IFafJavaIceAdapterClient>();
            JsonRpc = rpc;

            await FafJavaIceAdapterClient.SetIceServersAsync(iceServers);
            await FafJavaIceAdapterClient.SetLobbyInitModeAsync(mode);
            return gpgnetPort;
        }
        private JsonRpc SetupJsonRpc(Stream stream)
        {
            var jsonRpcMessageFormatter = new SystemTextJsonFormatter();
            var jsonRpcMessageHandler = new NewLineDelimitedMessageHandler(stream, stream, jsonRpcMessageFormatter);
            var rpc = new JsonRpc(jsonRpcMessageHandler)
            {
                CancelLocallyInvokedMethodsWhenConnectionIsClosed = true
            };
            rpc.AddLocalRpcTarget<IFafJavaIceAdapterCallbacks>(_fafJavaIceAdapterCallbacks, new()
            {
                DisposeOnDisconnect = true,
            });
            return rpc;
        }

        private void FafLobbyEventsService_IceUniversalDataReceived2(object sender, IceUniversalData2 e) => Task
            .Run(() => SendDataAsync(e))
            .SafeFireAndForget(x => _logger.LogError(x.Message));
        private async Task SendDataAsync(IceUniversalData2 e)
        {
            if (FafIceAdapterTcpClient?.Connected == false)
            {
                return;
            }
            if (e.Command == ServerCommand.JoinGame)
            {
                var remotePlayerLogin = e.args[0].ToString();
                var remotePlayerId = long.Parse(e.args[1].ToString());
                await FafJavaIceAdapterClient.JoinGameAsync(remotePlayerLogin, remotePlayerId);
            }
            else if (e.Command == ServerCommand.HostGame)
            {
                var mapName = e.args[0].ToString();
                await FafJavaIceAdapterClient.HostGameAsync(mapName);
            }
            else if (e.Command == ServerCommand.ConnectToPeer)
            {
                var remotePlayerLogin = e.args[0].ToString();
                var remotePlayerId = long.Parse(e.args[1].ToString());
                var offer = bool.Parse(e.args[2].ToString());
                await FafJavaIceAdapterClient.ConnectToPeerAsync(remotePlayerLogin, remotePlayerId, offer);
            }
            else if (e.Command == ServerCommand.IceMsg)
            {
                var remotePlayerId = long.Parse(e.args[0].ToString());
                var msg = e.args[1];
                await FafJavaIceAdapterClient.IceMsgAsync(remotePlayerId, msg);
            }
            else if (e.Command == ServerCommand.DisconnectFromPeer)
            {
                var remotePlayerId = long.Parse(e.args[0].ToString());
                await FafJavaIceAdapterClient.DisconnectFromPeerAsync(remotePlayerId);
            }
        }

        private async Task<string> EnsureFafJavaAdapterExistAsync(
            IProgress<ProgressReport> progress = null,
            CancellationToken cancellationToken = default)
        {
            var url = _settingsManager.ClientConfiguration?.FafIceAdapterUrl;
            var file = Path.GetFileName(url);
            var downloadPath = Path.Combine(AppHelper.FilesDirectory.FullName, file);
            if (!File.Exists(downloadPath))
            {
                await _downloadService.DownloadToFileAsync(url, downloadPath, progress, null, cancellationToken);
            }
            _logger.LogInformation("Path to FAF Java Ice Adapter: [{path}]", downloadPath);
            return downloadPath;
        }

        public async Task Stop()
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var graceShutdown = await Task
                .Run(async () =>
                {
                    await FafJavaIceAdapterClient.QuitAsync();
                    FafIceAdapterTcpClient.Close();
                    await FafIceAdapterProcess.WaitForExitAsync();
                }, cancellationTokenSource.Token)
                .ContinueWith(x => x.IsCompletedSuccessfully, TaskScheduler.Default);

            FafIceAdapterTcpClient?.Close();
            FafIceAdapterTcpClient?.Dispose();
            FafIceAdapterTcpClient = null;

            FafJavaIceAdapterClient = null;
            JsonRpc?.Dispose();
            JsonRpc = null;

            if (!graceShutdown) FafIceAdapterProcess?.Kill();
            FafIceAdapterProcess?.Dispose();
            FafIceAdapterProcess = null;
        }
    }
}
