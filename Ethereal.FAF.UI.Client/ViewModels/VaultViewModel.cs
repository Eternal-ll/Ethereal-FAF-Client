using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class VaultViewModel : Base.ViewModel
    {
        private readonly IConfiguration Configuration;

        private readonly MapsViewModel MapsViewModel;
        private readonly ModsViewModel ModsViewModel;

        public ServersManagement ServersManagement { get; }
        public string VaultLocation => Configuration.GetForgedAllianceVaultLocation();

        public VaultViewModel(ServersManagement serversManagement, MapsViewModel mapsViewModel, ModsViewModel modsViewModel, IConfiguration configuration)
        {
            serversManagement.ServerManagerAdded += ServersManagement_ServerManagerAdded;
            serversManagement.ServerManagerRemoved += ServersManagement_ServerManagerRemoved;

            MapsViewModel = mapsViewModel;
            ServersManagement = serversManagement;
            ModsViewModel = modsViewModel;

            SelectedServerManager = serversManagement.ServersManagers.FirstOrDefault();

            Configuration = configuration;
        }

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

        #region MyRegion
        private ServerManager _SelectedServerManager;
        public ServerManager SelectedServerManager
        {
            get => _SelectedServerManager;
            set
            {
                if (Set(ref _SelectedServerManager, value))
                {
                    ModsViewModel.SetFafApiClient(value?.GetApiClient());
                    MapsViewModel.SetFafApiClient(value?.GetApiClient());
                    MapsViewModel.SetContentUrl(value?.GetServer().Content.ToString());
                }
            }
        }
        #endregion

        public bool CanSelectServer => ServersManagement.ServersManagers.Count > 1;
    }
}
