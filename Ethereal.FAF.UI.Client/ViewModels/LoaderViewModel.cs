using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.Views;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Wpf.Ui.Mvvm.Contracts;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class LoaderViewModel : Base.ViewModel
    {
        private readonly INavigationWindow NavigationWindow;
        private readonly IConfiguration Configuration;

        public LoaderViewModel(INavigationWindow navigationWindow, IConfiguration configuration)
        {
            NavigationWindow = navigationWindow;
            Configuration = configuration;
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
            if (!ForgedAllianceHelper.DirectoryHasAnyGameFile(patch))
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
                NavigationWindow.Navigate(typeof(SelectVaultLocationView));
                return false;
            }
            return true;
        }
        public async Task TryPassChecksAndLetsSelectServer()
        {
            if (!await RunChecks()) return;
            NavigationWindow.Navigate(typeof(SelectServerView));
        }
    }
}
