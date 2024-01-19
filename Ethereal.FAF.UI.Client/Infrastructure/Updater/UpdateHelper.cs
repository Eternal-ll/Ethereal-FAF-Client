using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Progress;
using Ethereal.FAF.UI.Client.Models.Update;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Updater
{
    internal class UpdateHelper : IUpdateHelper
    {
        private readonly ILogger<UpdateHelper> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDownloadService _downloadService;
        private readonly ISettingsManager _settingsManager;
        private readonly IGithubService _githubService;

        private readonly System.Timers.Timer _checkUpdateTimer;

        public const string UpdateFolderName = ".EtherealFafClientUpdate";
        public static DirectoryInfo UpdateFolder = new(UpdateFolderName);

        public static FileInfo ExecutableFile = new(Path.Combine(UpdateFolderName, Process.GetCurrentProcess().ProcessName));


        private const string _manifestFileName = "update-manifest.json";
        private static Regex _exeFileNameRegex = new("win-x64\\..*");

        public UpdateHelper(ILogger<UpdateHelper> logger, IHttpClientFactory httpClientFactory, IDownloadService downloadService, ISettingsManager settingsManager, IGithubService githubService)
        {
            _checkUpdateTimer = new(TimeSpan.FromMinutes(60).TotalMilliseconds);
            _checkUpdateTimer.Elapsed += async (_, _) => await CheckForUpdate().ConfigureAwait(false);

            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _downloadService = downloadService;
            _settingsManager = settingsManager;
            _githubService = githubService;
        }

        public event EventHandler<UpdateStatusChangedEventArgs> UpdateStatusChanged;

        public async Task StartCheckingForUpdates()
        {
            _checkUpdateTimer.Start();
            await CheckForUpdate().ConfigureAwait(false);
        }

        public async Task CheckForUpdate()
        {
            try
            {
                var release = await _githubService.GetLatestRelease();
                var manifestAsset = release.Assets.FirstOrDefault(x => x.Name == _manifestFileName) ??
                    throw new InvalidOperationException("Update manifest config is missing in release");

                var httpClient = _httpClientFactory.CreateClient("UpdateClient");
                var response = await httpClient.GetAsync(manifestAsset.BrowserDownloadUrl).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Error while checking for update {StatusCode} - {Content}",
                        response.StatusCode,
                        await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                    );
                    return;
                }

                var updateManifest = await JsonSerializer
                .DeserializeAsync<UpdateManifest>(
                        await response.Content.ReadAsStreamAsync().ConfigureAwait(false),
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                    )
                    .ConfigureAwait(false);

                if (updateManifest is null)
                {
                    _logger.LogError("UpdateManifest is null");
                    return;
                }

                foreach (var channel in Enum
                    .GetValues(typeof(UpdateChannel))
                    .Cast<UpdateChannel>()
                    .Where(c => c == _settingsManager.Settings.PreferredUpdateChannel))
                {
                    if (updateManifest.Updates.TryGetValue(channel, out var update) &&
                        await FillUpdateInfo(update))
                    {
                        OnUpdateStatusChanged(new()
                        {
                            LatestUpdate = update,
                            UpdateChannels = updateManifest.Updates.ToDictionary(
                                kv => kv.Key,
                                kv => kv.Value
                            )!
                        });
                        return;
                    }
                }

                _logger.LogInformation("No update available");

                foreach (var update in updateManifest.Updates)
                {
                    if (!await FillUpdateInfo(update.Value))
                    {
                        //
                    }
                }

                OnUpdateStatusChanged(new()
                {
                    UpdateChannels = updateManifest.Updates.ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value
                    )!
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Couldn't check for update");
            }
        }

        public async Task DownloadUpdate(UpdateInfo updateInfo, IProgress<ProgressReport> progress)
        {
            UpdateFolder.Create();
            UpdateFolder.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

            FileInfo downloadFile = new FileInfo(Path.Combine(UpdateFolder.FullName, Path.GetFileName(updateInfo.Url.ToString())));

            var extractDir = new DirectoryInfo(Path.Combine(UpdateFolder.FullName, "extract"));

            try
            {
                var url = updateInfo.Url.ToString();
                // Download update
                await _downloadService
                    .DownloadToFileAsync(
                        url,
                        downloadFile.FullName,
                        progress: progress,
                        httpClientName: "UpdateClient"
                    )
                    .ConfigureAwait(false);

                // Unzip if needed
                if (downloadFile.Extension != ".exe")
                {
                    if (extractDir.Exists)
                    {
                        extractDir.Delete(true);
                    }
                    extractDir.Create();

                    await ArchiveHelper.Extract(downloadFile.FullName, extractDir.FullName, progress).ConfigureAwait(false);

                    // Find binary and move it up to the root
                    var binaryFile = extractDir
                        .EnumerateFiles("*.exe", SearchOption.AllDirectories)
                        .First();

                    binaryFile.MoveTo(ExecutableFile.FullName, true);
                }
                // Otherwise just copy
                else
                {
                    File.Copy(downloadFile.FullName, ExecutableFile.FullName, true);
                }

                progress.Report(new ProgressReport(1d));
            }
            finally
            {
                // Clean up original download
                downloadFile?.Delete();
                // Clean up extract dir
                if (extractDir.Exists)
                {
                    extractDir.Delete();
                }
            }
        }
        private void OnUpdateStatusChanged(UpdateStatusChangedEventArgs args)
        {
            UpdateStatusChanged?.Invoke(this, args);

            if (args.LatestUpdate is { } update)
            {
                _logger.LogInformation(
                    "Update available {AppVer} -> {UpdateVer}",
                    VersionHelper.GetCurrentVersionInText(),
                    update.Version
                );

                EventManager.Instance.OnUpdateAvailable(update);
            }
        }

        private void NotifyUpdateAvailable(UpdateInfo update)
        {
            _logger.LogInformation(
                "Update available {AppVer} -> {UpdateVer}",
                VersionHelper.GetCurrentVersionInText(),
                update.Version
            );
            EventManager.Instance.OnUpdateAvailable(update);
        }
        private async Task<bool> FillUpdateInfo(UpdateInfo update)
        {
            try
            {
                var release = await _githubService.GetRelease(update.Version);
                update.ReleaseDate = release.CreatedAt;

                var downloadAsset = release.Assets.FirstOrDefault(x => _exeFileNameRegex.IsMatch(x.Name));
                if (downloadAsset == null)
                {
                    return false;
                }
                update.Url = new(downloadAsset.BrowserDownloadUrl);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
