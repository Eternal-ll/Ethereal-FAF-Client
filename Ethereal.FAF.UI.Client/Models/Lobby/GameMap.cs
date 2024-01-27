using CommunityToolkit.Mvvm.ComponentModel;
using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.MapGen;

namespace Ethereal.FAF.UI.Client.Models.Lobby
{
    /// <summary>
    /// Information about Forged Alliance map
    /// </summary>
    public abstract partial class GameMap : ObservableObject
    {
        [ObservableProperty]
        private string _RawName;
        [ObservableProperty]
        private string _Name;
        [ObservableProperty]
        private FA.Vault.MapScenario _Scenario;
        [ObservableProperty]
        private string _Version;
        [ObservableProperty]
        private string _PathToImage;
    }
    /// <summary>
    /// Custom Forged Alliance map
    /// </summary>
    public partial class CustomGameMap : GameMap
    {
        public CustomGameMap(GameMapInfo info)
        {
            RawName = info.Raw;
            Name = info.Name;
            Version = info.Version;
        }
    }
    /// <summary>
    /// Generated map by Neroxis map generator
    /// </summary>
    public partial class NeroxisGameMap : GameMap
    {
        public NeroxisGameMap() {}
        public NeroxisGameMap(NeroxisMapInfo info)
        {
            RawName = info.Raw;
            Version = info.Version;
            Name = info.Seed;
        }
    }
}
