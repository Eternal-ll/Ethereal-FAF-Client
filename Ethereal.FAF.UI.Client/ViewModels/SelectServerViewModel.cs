using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Configuration;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using Ethereal.FAF.UI.Client.Views;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Wpf.Ui;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class SelectServerViewModel : ViewModel
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ISnackbarService _snackbarService;
        private readonly INavigationWindow _navigationWindow;
        private readonly IConfiguration _configuration;
        private readonly ClientManager _clientManager;
        private object _lockObject;

        public SelectServerViewModel(ISnackbarService snackbarService, INavigationWindow navigationWindow, IConfiguration configuration, ClientManager clientManager, ISettingsManager settingsManager)
        {
            SelectServerCommand = new LambdaCommand(OnSelectServerCommand, CanSelectServerCommand);
            _snackbarService = snackbarService;
            _navigationWindow = navigationWindow;
            _configuration = configuration;
            _clientManager = clientManager;
            _settingsManager = settingsManager;
        }

        public ObservableCollection<Server> Servers { get; set; }
        protected override void OnInitialLoaded()
        {
            Servers = new();
            OnPropertyChanged(nameof(Servers));
            _lockObject = new();
            BindingOperations.EnableCollectionSynchronization(Servers, _lockObject);

            Servers.Add(_configuration.GetSection("Server").Get<Server>());
            if (_settingsManager.Settings.RememberSelectedFaServer)
            {
                var server = Servers.FirstOrDefault(x => x.Name == _settingsManager.Settings.SelectedFaServer);
                if (server != null)
                {
                    SelectedServer = server;
                }
            }
        }

        #region SelectedServer
        private Server _SelectedServer;
        public Server SelectedServer { get => _SelectedServer; set => Set(ref _SelectedServer, value); }
        #endregion

        #region SelectServerCommand
        public ICommand SelectServerCommand { get; set; }
        private bool CanSelectServerCommand(object arg) => arg != null && arg is Server;
        private void OnSelectServerCommand(object arg)
        {
            if (arg is Server server) 
            {
                _clientManager.SetServer(server);
                _navigationWindow.Navigate(typeof(AuthView));
            }
        }
        #endregion
    }
}
