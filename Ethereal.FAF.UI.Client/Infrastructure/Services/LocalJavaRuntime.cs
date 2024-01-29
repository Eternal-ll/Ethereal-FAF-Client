using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Progress;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class LocalJavaRuntime : IJavaRuntime
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IDownloadService _downloadService;
        private readonly ILogger<LocalJavaRuntime> _logger;

        public LocalJavaRuntime(ISettingsManager settingsManager, IDownloadService downloadService, ILogger<LocalJavaRuntime> logger)
        {
            _settingsManager = settingsManager;
            _downloadService = downloadService;
            _logger = logger;
        }

        public async Task<string> EnsurJavaRuntimeExist(IProgress<ProgressReport> progress = null, CancellationToken cancellationToken = default)
        {
            var javaRuntime = Path.Combine(AppHelper.FilesDirectory.FullName, "jre", "bin", "java.exe");
            if (File.Exists(javaRuntime))
            {
                _logger.LogInformation("Path to Java Runtime: [{path}]", javaRuntime);
                return javaRuntime;
            }
            var url = _settingsManager.ClientConfiguration?.JavaRuntimeUrl;
            var file = Path.GetFileName(url);
            var downloadPath = Path.Combine(AppHelper.FilesDirectory.FullName, file);

            await _downloadService.DownloadToFileAsync(url, downloadPath, progress, null, cancellationToken);

            if (!string.IsNullOrEmpty(Path.GetExtension(file)))
            {
                await ArchiveHelper.Extract(downloadPath, AppHelper.FilesDirectory.FullName, progress);
            }
            _logger.LogInformation("Path to Java Runtime: [{path}]", javaRuntime);
            return javaRuntime;
        }
    }
}
