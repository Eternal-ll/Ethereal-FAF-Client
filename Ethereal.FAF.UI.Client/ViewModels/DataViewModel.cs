using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class DataViewModel : Base.ViewModel
    {
        private readonly IConfiguration Configuration;
        public ServersManagement ServersManagement { get; }
        private readonly MapsViewModel MapsViewModel;
        private readonly ModsViewModel ModsViewModel;

        public DataViewModel(ServersManagement serversManagement, MapsViewModel mapsViewModel, ModsViewModel modsViewModel, IConfiguration configuration)
        {
            serversManagement.ServerManagerAdded += ServersManagement_ServerManagerAdded;
            serversManagement.ServerManagerRemoved += ServersManagement_ServerManagerRemoved;

            ServersManagement = serversManagement;
            MapsViewModel = mapsViewModel;
            ModsViewModel = modsViewModel;

            SelectedServerManager = serversManagement.ServersManagers.FirstOrDefault();
            Configuration = configuration;
        }

        public string VaultLocation => Configuration.GetForgedAllianceVaultLocation();

        private void ServersManagement_ServerManagerRemoved(object sender, ServerManager e)
        {
            SelectedServerManager = null;
            OnPropertyChanged(nameof(CanSelectServer));
        }

        private void ServersManagement_ServerManagerAdded(object sender, ServerManager e)
        {
            SelectedServerManager ??= e;
            OnPropertyChanged(nameof(CanSelectServer));
        }

        public bool CanSelectServer => ServersManagement.ServersManagers.Count > 1;
        #region SelectedServerManager
        private ServerManager _SelectedServerManager;
        public ServerManager SelectedServerManager
        {
            get => _SelectedServerManager;
            set
            {
                if (Set(ref _SelectedServerManager, value))
                {
                    ModsViewModel.SetServerManager(value);
                    MapsViewModel.SetFafApiClient(value?.GetApiClient());
                    MapsViewModel.SetContentUrl(value?.GetServer().Content.ToString());
                }
            }
        }
        #endregion
    }
}
