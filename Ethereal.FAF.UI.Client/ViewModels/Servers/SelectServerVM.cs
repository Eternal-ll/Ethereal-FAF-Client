using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Wpf.Ui.Extensions;
using Wpf.Ui.Mvvm.Services;

namespace Ethereal.FAF.UI.Client.ViewModels.Servers
{
    public class SelectServerVM : Base.ViewModel
    {
        private readonly IConfiguration Configuration;
        private readonly IHttpClientFactory HttpClientFactory;
        private readonly ILogger Logger;
        private readonly SnackbarService SnackbarService;
        private readonly PatchWatcher PatchWatcher;

        private CancellationTokenSource ApiCallsCancellationTokenSource;

        public SelectServerVM(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<SelectServerVM> logger, ServersManagement serversManagement, SnackbarService snackbarService, PatchWatcher patchWatcher)
        {
            Configuration = configuration;
            HttpClientFactory = httpClientFactory;
            Logger = logger;
            ServersManagement = serversManagement;

            serversManagement.ServerManagerAdded += ServersManagement_ServerManagerAdded;
            serversManagement.OAuthRequired += ServersManagement_OAuthRequired;
            serversManagement.ErrorOccuried += ServersManagement_ErrorOccuried;

            ConnectAndAuthorizeOnServerCommand = new LambdaCommand(OnConnectAndAuthorizeOnServerCommand, CanConnectAndAuthorizeOnServerCommand);
            CancelConnectAndAuthorizeOnServerCommand = new LambdaCommand(OnCancelConnectAndAuthorizeOnServerCommand, CanCancelConnectAndAuthorizeOnServerCommand);

            Servers = serversManagement.Servers;
            SnackbarService = snackbarService;
            PatchWatcher = patchWatcher;
        }

        private void ServersManagement_ErrorOccuried(object sender, (Server server, string error, string description) e)
        {
            SnackbarService.Show(e.error, e.description, Wpf.Ui.Common.SymbolRegular.Warning20, Wpf.Ui.Common.ControlAppearance.Secondary);
        }

        private void ServersManagement_OAuthRequired(object sender, (Server server, string OAuthUrl) e)
        {
            SnackbarService.Show("Notification", "OAuth link generated");
            OAuthLinks.Remove(OAuthLinks.FirstOrDefault(o => o.server.Equals(e.server)));
            OAuthLinks.Add(e);
        }

        private void ServersManagement_ServerManagerAdded(object sender, ServerManager e)
        {
            SnackbarService.Show("Warning", "It will lag for a few seconds. I will launch patch watchers");
            PatchWatcher.InitializePatchWatchers();
            Task.Run(PatchWatcher.InitializePatchWatching);
            if (!JavaHelper.HasJavaRuntime())
            {
                Task.Run(() => JavaHelper.PrepareJavaRuntime());
            }
        }

        public ServersManagement ServersManagement { get; }
        public ObservableCollection<Server> Servers { get; }

        public ObservableCollection<(Server server, string oAuth)> OAuthLinks { get; set; } = new();

        #region SelectedServer
        private Server _SelectedServer;
        public Server SelectedServer
        {
            get => _SelectedServer;
            set => Set(ref _SelectedServer, value);
        }
        #endregion


        private CancellationTokenSource OAuthCancellationTokenSource;

        #region ConnectAndAuthorizeOnServerCommand

        public ICommand ConnectAndAuthorizeOnServerCommand { get; set; }
        private bool CanConnectAndAuthorizeOnServerCommand(object arg) => SelectedServer is not null;
        private async void OnConnectAndAuthorizeOnServerCommand(object arg)
        {
            OAuthCancellationTokenSource = new();
            try
            {
                await ServersManagement.AuthorizeAndConnectToServerAsync(SelectedServer, OAuthCancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                SnackbarService.Timeout = 15000;
                SnackbarService.Show("Faulted", ex.ToString(), Wpf.Ui.Common.SymbolRegular.Warning20, Wpf.Ui.Common.ControlAppearance.Secondary);
                SelectedServer.ServerState = ServerState.Idle;
            }
            if (SelectedServer.ServerState is ServerState.Authorizing)
            {
                SelectedServer.ServerState = ServerState.Idle;
            }
            OAuthCancellationTokenSource = null;
        }

        #endregion

        #region CancelConnectAndAuthorizeOnServerCommand
        public ICommand CancelConnectAndAuthorizeOnServerCommand { get; }
        private bool CanCancelConnectAndAuthorizeOnServerCommand(object arg) => OAuthCancellationTokenSource is not null;
        private void OnCancelConnectAndAuthorizeOnServerCommand(object arg)
        {
            OAuthCancellationTokenSource.Cancel();
        }
        #endregion

        public void StartApiCalls()
        {
            if (ApiCallsCancellationTokenSource is not null)
            {
                ApiCallsCancellationTokenSource.Cancel();
            }
            ApiCallsCancellationTokenSource = new();
            var task = Task.Run(async () =>
            {
                var client = HttpClientFactory.CreateClient();
                while (!ApiCallsCancellationTokenSource.IsCancellationRequested)
                {
                    foreach (var server in Servers)
                    {
                        try
                        {
                            if (server.Site is null) continue;
                            var baseAddress = server.Site.Append("/lobby_api?resource=");
                            var playersCount = int.Parse(await client.GetStringAsync(baseAddress + "players"));
                            var gamesCount = int.Parse(await client.GetStringAsync(baseAddress + "games"));
                            server.SetPlayersCount(playersCount);
                            server.SetGamesCount(gamesCount);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogWarning("Failed to get count of players/games from [{host}]", server.Site);
                        }
                    }
                    await Task.Delay(15000);
                }
            });
        }
        public void StopApiCalls()
        {
            ApiCallsCancellationTokenSource?.Cancel();
        }
    }
}
