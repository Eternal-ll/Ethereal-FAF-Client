using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class DataViewModel : Base.ViewModel
    {
        private readonly IConfiguration Configuration;

        public DataViewModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string VaultLocation => Configuration.GetForgedAllianceVaultLocation();
    }
}
