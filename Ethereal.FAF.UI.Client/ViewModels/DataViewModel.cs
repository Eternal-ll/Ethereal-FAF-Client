using CommunityToolkit.Mvvm.ComponentModel;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models;
using Ethereal.FAF.UI.Client.Views;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public partial class DataViewModel : Base.ViewModel
    {
        private readonly ISettingsManager _settingsManager;

        public DataViewModel(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public string VaultLocation => _settingsManager.Settings.ForgedAllianceVaultLocation;

        [ObservableProperty]
        private NavigationCard[] _NavigationCards;
        protected override void OnInitialLoaded()
        {
            NavigationCards = new NavigationCard[]
            {
                new()
                {
                    Name = "Matchmaking.queues",
                    Description = "matchmaking.queues.desription",
                    PageType = typeof(MatchmakerQueuesView)
                }
            };
            base.OnInitialLoaded();
        }
        public override void OnUnloaded()
        {
            NavigationCards = null;
            base.OnUnloaded();
        }
    }
}
