﻿using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.Ice;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Progress;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Contrib.WaitAndRetry;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    /// <summary>
    /// Proxy class to work with FAF Java ICE Adapter
    /// </summary>
    internal class FafJavaIceAdapter : IGameNetworkAdapter, IFafJavaIceAdapterClient
    {
        private readonly IJavaRuntime _javaRuntime;
        private readonly IFafAuthService _fafAuthService;
        private readonly ISettingsManager _settingsManager;
        private readonly IDownloadService _downloadService;
        //private readonly IFafLobbyEventsService _fafLobbyEventsService;
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
            //IFafLobbyEventsService fafLobbyEventsService,
            IServiceProvider serviceProvider,
            IFafJavaIceAdapterCallbacks fafJavaIceAdapterCallbacks)
        {
            //fafLobbyEventsService.IceUniversalDataReceived2 += FafLobbyEventsService_IceUniversalDataReceived2;
            _javaRuntime = javaRuntime;
            _fafAuthService = fafAuthService;
            _settingsManager = settingsManager;
            _downloadService = downloadService;
            //_fafLobbyEventsService = fafLobbyEventsService;
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
                        .WithForcedRelay(true)
                        //.Append("--debug-window")
                        .ToString(),
                }
            };
            Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = $"https://ice-telemetry.faforever.com/app.html?gameId={gameId}&playerId={_fafAuthService.GetUserId()}"
            });
            _logger.LogInformation("Starting Java Ice Adapter with args: [{args}]", FafIceAdapterProcess.StartInfo.Arguments);
            progress?.Report(new(-1, "Ice Adapter", "Running Java Ice Adapter...", true));
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

            progress?.Report(new(-1, "Ice Adapter", "Connecting to Java Ice Adapter...", true));
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

        //private void FafLobbyEventsService_IceUniversalDataReceived2(object sender, IceUniversalData2 e) => Task
        //    .Run(() => SendDataAsync(e))
        //    .SafeFireAndForget(x => _logger.LogError(x.Message));
        //private async Task SendDataAsync(IceUniversalData2 e)
        //{
        //    if (FafIceAdapterTcpClient?.Connected == false)
        //    {
        //        return;
        //    }
        //    if (e.Command == ServerCommand.JoinGame)
        //    {
        //        var remotePlayerLogin = e.args[0].ToString();
        //        var remotePlayerId = long.Parse(e.args[1].ToString());
        //        await FafJavaIceAdapterClient.JoinGameAsync(remotePlayerLogin, remotePlayerId);
        //    }
        //    else if (e.Command == ServerCommand.HostGame)
        //    {
        //        var mapName = e.args[0].ToString();
        //        await FafJavaIceAdapterClient.HostGameAsync(mapName);
        //    }
        //    else if (e.Command == ServerCommand.ConnectToPeer)
        //    {
        //        var remotePlayerLogin = e.args[0].ToString();
        //        var remotePlayerId = long.Parse(e.args[1].ToString());
        //        var offer = bool.Parse(e.args[2].ToString());
        //        await FafJavaIceAdapterClient.ConnectToPeerAsync(remotePlayerLogin, remotePlayerId, offer);
        //    }
        //    else if (e.Command == ServerCommand.IceMsg)
        //    {
        //        var remotePlayerId = long.Parse(e.args[0].ToString());
        //        var msg = e.args[1].ToString();
        //        await FafJavaIceAdapterClient.IceMsgAsync(remotePlayerId, msg);
        //    }
        //    else if (e.Command == ServerCommand.DisconnectFromPeer)
        //    {
        //        var remotePlayerId = long.Parse(e.args[0].ToString());
        //        await FafJavaIceAdapterClient.DisconnectFromPeerAsync(remotePlayerId);
        //    }
        //}

        private async Task<string> EnsureFafJavaAdapterExistAsync(
            IProgress<ProgressReport> progress = null,
            CancellationToken cancellationToken = default)
        {
            var url = _settingsManager.ClientConfiguration?.FafIceAdapterUrl;
            var file = Path.GetFileName(url);
            var downloadPath = Path.Combine(AppHelper.FilesDirectory.FullName, file);
            if (!File.Exists(downloadPath))
            {
                _logger.LogInformation("Adapter not found, downloading...");
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
                    if (FafJavaIceAdapterClient == null) return false;
                    await FafJavaIceAdapterClient.QuitAsync();
                    FafIceAdapterTcpClient.Close();
                    await FafIceAdapterProcess.WaitForExitAsync();
                    return true;
                }, cancellationTokenSource.Token)
                .ContinueWith(x => x.IsCompletedSuccessfully ? x.Result : false, TaskScheduler.Default);

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

        public async Task PrepareAsync(IProgress<ProgressReport> progress = null, CancellationToken cancellationToken = default)
        {
            await _javaRuntime.EnsurJavaRuntimeExist(progress, cancellationToken);
            await EnsureFafJavaAdapterExistAsync(progress, cancellationToken);
        }

        #region IFafJavaIceAdapterClient
        private bool JavaIceAdapterConnected([CallerMemberName] string method = null)
        {
            _logger.LogTrace(
                "Invoking method [{0}]",
                method);
            var connected = !(
                FafIceAdapterTcpClient == null ||
                JsonRpc == null ||
                !FafIceAdapterTcpClient.Connected);
            if (!connected)
            {
                _logger.LogWarning(
                    "Adapter not connected, method [{0}] ignored",
                    method);
            }
            return connected;
        }
        /// <summary>
        /// Gracefully shuts down the faf-ice-adapter.
        /// </summary>
        /// <returns></returns>
        [JsonRpcMethod("quit")]
        public Task QuitAsync() => throw new NotImplementedException();
        /// <summary>
        /// Tell the game to create the lobby and host game on Lobby-State.
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns></returns>
        [JsonRpcMethod("hostGame")]
        public async Task HostGameAsync(string mapName)
        {
            if (!JavaIceAdapterConnected()) return;
            await FafJavaIceAdapterClient.HostGameAsync(mapName);
        }
        /// <summary>
        /// Tell the game to create the Lobby, create a PeerRelay in answer mode and join the remote game.
        /// </summary>
        /// <param name="remotePlayerLogin"></param>
        /// <param name="remotePlayerId"></param>
        /// <returns></returns>
        [JsonRpcMethod("joinGame")]
        public async Task JoinGameAsync(string remotePlayerLogin, long remotePlayerId)
        {
            if (!JavaIceAdapterConnected()) return;
            await FafJavaIceAdapterClient.JoinGameAsync(remotePlayerLogin, remotePlayerId);
        }
        /// <summary>
        /// Create a PeerRelay and tell the game to connect to the remote peer with offer/answer mode.
        /// </summary>
        /// <param name="remotePlayerLogin"></param>
        /// <param name="remotePlayerId"></param>
        /// <param name="offer"></param>
        /// <returns></returns>
        [JsonRpcMethod("connectToPeer")]
        public async Task ConnectToPeerAsync(string remotePlayerLogin, long remotePlayerId, bool offer)
        {
            if (!JavaIceAdapterConnected()) return;
            await FafJavaIceAdapterClient.ConnectToPeerAsync(remotePlayerLogin, remotePlayerId, offer);
        }
        /// <summary>
        /// Destroy PeerRelay and tell the game to disconnect from the remote peer.
        /// </summary>
        /// <param name="remotePlayerId"></param>
        /// <returns></returns>
        [JsonRpcMethod("disconnectFromPeer")]
        public async Task DisconnectFromPeerAsync(long remotePlayerId)
        {
            if (!JavaIceAdapterConnected()) return;
            await FafJavaIceAdapterClient.DisconnectFromPeerAsync(remotePlayerId);
        }
        /// <summary>
        /// Set the lobby mode the game will use.<br/>
        /// Supported values are "normal" for normal lobby and "auto" for automatch lobby (aka ladder).
        /// </summary>
        /// <param name="lobbyInitMode"></param>
        /// <returns></returns>
        [JsonRpcMethod("setLobbyInitMode")]
        public Task SetLobbyInitModeAsync(string lobbyInitMode) => throw new NotImplementedException();
        /// <summary>
        /// Add the remote ICE message to the PeerRelay to establish a connection.
        /// </summary>
        /// <param name="remotePlayerId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        [JsonRpcMethod("iceMsg")]
        public async Task IceMsgAsync(long remotePlayerId, string msg)
        {
            if (!JavaIceAdapterConnected()) return;
            await FafJavaIceAdapterClient.IceMsgAsync(remotePlayerId, msg);
        }
        /// <summary>
        /// Send an arbitrary message to the game.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="chunks"></param>
        /// <returns></returns>
        [JsonRpcMethod("sendToGpgNet")]
        public Task SendToGpgNetAsync(string header, params string[] chunks) => throw new NotImplementedException();
        /// <summary>
        /// ICE server array for use in webrtc.
        /// Must be called before joinGame/connectToPeer.<br/>
        /// See <see href="https://developer.mozilla.org/en-US/docs/Web/API/RTCIceServer"/>
        /// </summary>
        /// <param name="iceServers"></param>
        /// <returns></returns>
        [JsonRpcMethod("setIceServers")]
        public Task SetIceServersAsync(List<IceServer> iceServers) => throw new NotImplementedException();
        /// <summary>
        /// Polls the current status of the faf-ice-adapter.
        /// </summary>
        /// <returns></returns>
        [JsonRpcMethod("status")]
        [Obsolete]
        public Task<object> StatusAsync() => throw new NotImplementedException();

        #endregion
    }
}
