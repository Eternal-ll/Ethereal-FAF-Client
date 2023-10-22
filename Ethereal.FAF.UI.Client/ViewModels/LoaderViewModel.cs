using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.MapGen;
using Ethereal.FAF.UI.Client.Infrastructure.Patch;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Wpf.Ui.Mvvm.Contracts;

namespace Ethereal.FAF.UI.Client.ViewModels
{
	public class LoaderViewModel : Base.ViewModel
    {
        private readonly INavigationWindow NavigationWindow;
        private readonly MapGenerator MapGenerator;
        private readonly PatchWatcher PatchWatcher;
        private readonly IConfiguration Configuration;
        private readonly ILogger<LoaderViewModel> Logger;

        public LoaderViewModel(INavigationWindow navigationWindow, IConfiguration configuration, MapGenerator mapGenerator, ILogger<LoaderViewModel> logger, PatchWatcher patchWatcher)
        {
            NavigationWindow = navigationWindow;
            Configuration = configuration;
            MapGenerator = mapGenerator;
            Logger = logger;
            PatchWatcher = patchWatcher;
        }

        #region SplashText
        private string _SplashText;
        public string SplashText
        {
            get => _SplashText;
            set => Set(ref _SplashText, value);
        }
        #endregion

        public void SetLabel(string label) => SplashText = label;
        public void Navigate()
        {
            NavigationWindow.Navigate(typeof(Views.LoaderView));
        }


        public async Task<bool> RunChecks()
        {
            var game = Configuration.GetForgedAllianceLocation();
            if (!ForgedAllianceHelper.DirectoryHasAnyGameFile(game))
            {
                if (!ForgedAllianceHelper.TryFindGameDirectory(out game))
                {
                    NavigationWindow.Navigate(typeof(SelectGameLocationView));
                    return false;
                }
                UserSettings.Update(ConfigurationConstants.ForgedAllianceLocation, game);
            }
            var patch = Configuration.GetForgedAlliancePatchLocation();
            if (string.IsNullOrWhiteSpace(patch))
            {
                if (!ForgedAllianceHelper.DirectoryHasAnyGameFile(FaPaths.DefaultConfigLocation))
                {
                    NavigationWindow.Navigate(typeof(SelectFaPatchLocationView));
                    return false;
                }
                UserSettings.Update(ConfigurationConstants.ForgedAlliancePatchLocation, FaPaths.DefaultConfigLocation);
            }
            var vault = Configuration.GetForgedAllianceVaultLocation();
            if (string.IsNullOrWhiteSpace(vault))
            {
                Logger.LogWarning("Vault location is empty");
                NavigationWindow.Navigate(typeof(SelectVaultLocationView));
                return false;
            }
            await MapGenerator.InitializeAsync();
            return true;
        }
        public async Task TryPassChecksAndLetsSelectServer()
        {
            if (!await RunChecks())
                return;
        }
    }
}
