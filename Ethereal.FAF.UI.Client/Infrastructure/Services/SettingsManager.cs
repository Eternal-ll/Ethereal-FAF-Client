using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Settings;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class SettingsManager : ISettingsManager
    {
        private readonly Settings _settings;

        public SettingsManager(Settings settings)
        {
            _settings = settings;
        }

        public Settings Settings => _settings;

        public RemoteClientConfiguration ClientConfiguration { get; }
    }
}
