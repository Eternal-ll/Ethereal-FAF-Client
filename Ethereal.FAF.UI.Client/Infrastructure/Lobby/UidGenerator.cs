using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Polly.Contrib.WaitAndRetry;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Lobby
{
    /// <summary>
    /// System UID generator using lobby session code
    /// </summary>
    public sealed class UidGenerator : IUIDService
    {
        private static FileInfo FileInfo = new(Path.Combine(AppHelper.FilesDirectory.FullName, "faf-uid.exe"));

        private readonly ILogger<UidGenerator> _logger;
        private readonly ISettingsManager _settingsManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public UidGenerator(ILogger<UidGenerator> logger, ISettingsManager settingsManager, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _settingsManager = settingsManager;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> GenerateAsync(string session, CancellationToken cancellationToken = default)
        {
            await EnsureFafUidExist(cancellationToken);
            _logger.LogTrace("Generating UID for session [{session}]", session);
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = FileInfo.FullName,
                    Arguments = session,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
            _logger.LogTrace("Launching UID generator on [{fafuid}]", process.StartInfo.FileName);
            process.Start();
            _logger.LogTrace("Reading output...");
            string result = await process.StandardOutput.ReadLineAsync();
            _logger.LogTrace("Done reading ouput");
            _logger.LogTrace("Generated UID: [**********]");
            _logger.LogTrace("Closing UID generator...");
            process.Close();
            process.Dispose();
            _logger.LogTrace("UID closed");
            return result;
        }
        private async Task EnsureFafUidExist(CancellationToken cancellationToken)
        {
            var file = FileInfo;
            if (file.Exists) return;

            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            HttpResponseMessage? response = null;
            foreach (var delay in Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(50), retryCount: 4))
            {
                response = await client.GetAsync(_settingsManager.ClientConfiguration.FafUidUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    response = null;
                    continue;
                }
            }
            if (response == null)
            {
                throw new ApplicationException("Unable to fetch faf-uid");
            }
            using var fs = FileInfo.OpenWrite();
            await response.Content.CopyToAsync(fs, cancellationToken);
        }
    }
}
