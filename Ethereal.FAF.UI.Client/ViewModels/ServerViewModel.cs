using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.IRC;
using Ethereal.FAF.UI.Client.Infrastructure.Lobby;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using Ethereal.FAF.UI.Client.Views;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Ui;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public sealed class ServerViewModel : Base.ViewModel
    {
        private readonly TokenProvider TokenProvider;
        private readonly UidGenerator UidGenerator;
        private readonly LobbyClient LobbyClient;
        private readonly IrcClient IrcClient;

        private readonly SnackbarService SnackbarService;

        private readonly IConfiguration Configuration;
        private readonly INavigationWindow NavigationWindow;

        public ServerViewModel(TokenProvider tokenProvider, LobbyClient lobbyClient, IrcClient ircClient, INavigationWindow navigationWindow, SnackbarService snackbarService, UidGenerator uidGenerator, IConfiguration configuration)
        {
            TokenProvider = tokenProvider;
            LobbyClient = lobbyClient;
            IrcClient = ircClient;
            NavigationWindow = navigationWindow;
            SnackbarService = snackbarService;
            UidGenerator = uidGenerator;
            Configuration = configuration;

            //TokenProvider.TokenReceived += TokenProvider_TokenReceived;
            lobbyClient.StateChanged += LobbyClient_StateChanged;
            lobbyClient.SessionReceived += LobbyClient_SessionReceived;
            lobbyClient.AuthentificationFailed += LobbyClient_AuthentificationFailed;
        }

        private void TokenProvider_TokenReceived(object sender, TokenBearer e)
        {
            //LobbyClient.Authorization = $"Bearer {e.AccessToken}";
            //LobbyClient.ConnectAsync();
        }

        private void LobbyClient_AuthentificationFailed(object sender, global::FAF.Domain.LobbyServer.AuthentificationFailedData e)
        {
            LobbyClient.Disconnect();
            //LobbyClient.ConnectAsync();
        }

        private CancellationTokenSource CancellationTokenSource;

        private void LobbyClient_SessionReceived(object sender, string session)
        {
            RunAuthorizationTask(session);
        }

        private void RunAuthorizationTask(string session)
        {
            CancellationTokenSource = new();
            Task.Run(async () =>
            {
                var uid = await UidGenerator.GenerateAsync(session);
                var token = await TokenProvider.GetAccessTokenAsync(CancellationTokenSource.Token);
                LobbyClient.Authenticate(token, uid, session);
            }, CancellationTokenSource.Token);
        }

        private void LobbyClient_StateChanged(object sender, LobbyState e)
        {
            //App.Current.Dispatcher.BeginInvoke(() =>
            //SnackbarService.Show("Lobby", $"Lobby connection state changed to [{e}]", Wpf.Ui.Common.SymbolRegular.NetworkCheck20), System.Windows.Threading.DispatcherPriority.Background);
            switch (e)
            {
                case LobbyState.None:
                    break;
                case LobbyState.Connecting:
                    break;
                case LobbyState.Connected:
                    LobbyClient.AskSession(Configuration.GetValue<string>("Server:OAuth:ClientId"), Configuration.GetClientVersion());
                    break;
                case LobbyState.Authorizing:
                    break;
                case LobbyState.Authorized:
                    App.Current.Dispatcher.BeginInvoke(() => NavigationWindow.Navigate(typeof(NavigationView)), System.Windows.Threading.DispatcherPriority.Background);
                    break;
                case LobbyState.Disconnecting:
                    break;
                case LobbyState.Disconnected:
                    CancellationTokenSource?.Cancel();
                    App.Current.Dispatcher.BeginInvoke(() => NavigationWindow.Navigate(typeof(LoaderView)), System.Windows.Threading.DispatcherPriority.Background);
					//LobbyClient.Disconnect();
                    //LobbyClient.Connect();
					break;
            }
        }
    }
}
    