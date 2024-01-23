using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Settings;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class SettingsManager : ISettingsManager
    {
        private readonly Settings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SettingsManager> _logger;
        public SettingsManager(Settings settings, IHttpClientFactory httpClientFactory, ILogger<SettingsManager> logger)
        {
            _settings = settings;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public Settings Settings => _settings;

        public RemoteClientConfiguration ClientConfiguration { get; private set; }

        public async Task LoadAsync()
        {
            if (ClientConfiguration == null)
            {
                _logger.LogInformation("Fetching client configuration...");
                using var client = _httpClientFactory.CreateClient("ClientConfig");
                var response = await client.GetAsync(client.BaseAddress);
                if (response.IsSuccessStatusCode)
                {
                    ClientConfiguration = await response.Content.ReadFromJsonAsync<RemoteClientConfiguration>();
                }
            }
        }
    }
}
