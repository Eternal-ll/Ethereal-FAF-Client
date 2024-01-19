using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Infrastructure.Updater;
using Ethereal.FAF.UI.Client.Models.Progress;
using Ethereal.FAF.UI.Client.Models.Update;
using Ethereal.FAF.UI.Client.ViewModels.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Ethereal.FAF.UI.Client.ViewModels.Dialogs
{
    public partial class UpdateViewModel : ContentDialogViewModelBase
    {
        private readonly ILogger<UpdateViewModel> _logger;
        private readonly ISettingsManager _settingsManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUpdateHelper _updateHelper;
        private readonly IGithubService _githubService;

        private bool isLoaded;

        [ObservableProperty]
        private bool isUpdateAvailable;

        [ObservableProperty]
        private UpdateInfo? updateInfo;

        [ObservableProperty]
        private string? releaseNotes;

        [ObservableProperty]
        private string? updateText;

        [ObservableProperty]
        private int progressValue;

        [ObservableProperty]
        private bool isProgressIndeterminate;

        [ObservableProperty]
        private bool showProgressBar;

        [ObservableProperty]
        private string? currentVersionText;

        [ObservableProperty]
        private string? newVersionText;

        private static Regex RegexChangelog() => new(@"(##\s*(v[0-9]+\.[0-9]+\.[0-9]+(?:-(?:[0-9A-Za-z-.]+))?)((?:\n|.)+?))(?=(##\s*v[0-9]+\.[0-9]+\.[0-9]+)|\z)");

        public UpdateViewModel(
            ILogger<UpdateViewModel> logger,
            ISettingsManager settingsManager,
            IHttpClientFactory httpClientFactory,
            IUpdateHelper updateHelper,
            IGithubService githubService)
        {
            _logger = logger;
            _settingsManager = settingsManager;
            _httpClientFactory = httpClientFactory;
            _updateHelper = updateHelper;
            _githubService = githubService;

            Infrastructure.Helper.EventManager.Instance.UpdateAvailable += (_, info) =>
            {
                IsUpdateAvailable = true;
                UpdateInfo = info;
            };
            updateHelper.StartCheckingForUpdates().SafeFireAndForget();
        }

        public async Task Preload()
        {
            if (UpdateInfo is null)
                return;

            ReleaseNotes = await GetReleaseNotes(UpdateInfo.Version);
        }

        partial void OnUpdateInfoChanged(UpdateInfo? value)
        {
            CurrentVersionText = $"v{VersionHelper.GetCurrentVersion()}";
            NewVersionText = $"v{value?.Version}";
        }

        public override async Task OnLoadedAsync()
        {
            if (!isLoaded)
            {
                await Preload();
            }
        }

        /// <inheritdoc />
        public override void OnUnloaded()
        {
            base.OnUnloaded();
            isLoaded = false;
        }

        [RelayCommand]
        private async Task InstallUpdate()
        {
            if (UpdateInfo == null)
            {
                return;
            }

            ShowProgressBar = true;
            IsProgressIndeterminate = true;
            UpdateText = string.Format("Downloading update...");

            try
            {
                await _updateHelper.DownloadUpdate(
                    UpdateInfo,
                    new Progress<ProgressReport>(report =>
                    {
                        ProgressValue = Convert.ToInt32(report.Percentage);
                        IsProgressIndeterminate = report.IsIndeterminate;
                        UpdateText = report.Message;
                    })
                );
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to download update");

                var dialog = DialogHelper.CreateMarkdownDialog(
                    $"{e.GetType().Name}: {e.Message}",
                    "Unexcepted error occuried"
                );

                await dialog.ShowAsync();
                return;
            }

            UpdateText = "Getting a few things ready...";
            await using (new MinimumDelay(500, 1000))
            {
                Process.Start(UpdateHelper.ExecutableFile.FullName,
                    $"--wait-for-exit-pid {Environment.ProcessId}"
                );
            }

            UpdateText = "Update complete. Restarting in 3 seconds...";
            await Task.Delay(1000);
            UpdateText = "Update complete. Restarting in 2 seconds...";
            await Task.Delay(1000);
            UpdateText = "Update complete. Restarting in 1 second...";
            await Task.Delay(1000);
            UpdateText = "Update complete. Restarting...";

            Application.Current.Shutdown();
        }

        internal async Task<string> GetReleaseNotes(string version)
        {
            using var client = _httpClientFactory.CreateClient();

            try
            {
                var release = await _githubService.GetRelease(version);
                return release.Body;
            }
            catch (HttpRequestException e)
            {
                return $"## Unable to fetch release notes ({e.StatusCode})\n\n[{version}]({version})";
            }
            catch (TaskCanceledException) { }

            return $"## Unable to fetch release notes\n\n[{version}]({version})";
        }

        /// <summary>
        /// Formats changelog markdown including up to the current version
        /// </summary>
        /// <param name="markdown">Markdown to format</param>
        /// <param name="currentVersion">Versions equal or below this are excluded</param>
        /// <param name="maxChannel">Maximum channel level to include</param>
        internal static string? FormatChangelog(
            string markdown,
            string currentVersion,
            UpdateChannel maxChannel = UpdateChannel.Stable
        )
        {
            var pattern = RegexChangelog();

            var results = pattern
                .Matches(markdown)
                .Select(
                    m =>
                        new
                        {
                            Block = m.Groups[1].Value.Trim(),
                            Version = Version.TryParse(m.Groups[2].Value.Trim(), out var version)
                                ? version
                                : null,
                            Content = m.Groups[3].Value.Trim()
                        }
                )
                .Where(x => x.Version is not null)
                .ToList();

            // Join all blocks until and excluding the current version
            // If we're on a pre-release, include the current release
            var currentVersionBlock = results.FindIndex(
                x => x.Version.ToString() == currentVersion
            );

            // For mismatching build metadata, add one
            //if (
            //    currentVersionBlock != -1
            //    && results[currentVersionBlock].Version?.Metadata != currentVersion.Metadata
            //)
            //{
            //    currentVersionBlock++;
            //}

            // Support for previous pre-release without changelogs
            //if (currentVersionBlock == -1)
            //{
            //    currentVersionBlock = results.FindIndex(
            //        x => x.Version == currentVersion.WithoutPrereleaseOrMetadata()
            //    );

            //    // Add 1 if found to include the current release
            //    if (currentVersionBlock != -1)
            //    {
            //        currentVersionBlock++;
            //    }
            //}

            // Still not found, just include all
            if (currentVersionBlock == -1)
            {
                currentVersionBlock = results.Count;
            }

            // Filter out pre-releases
            var blocks = results
                .Take(currentVersionBlock)
                .Where(
                    x => true
                        //x.Version!.PrereleaseIdentifiers.Count == 0
                        //|| x.Version.PrereleaseIdentifiers[0].Value switch
                        //{
                        //    "pre" when maxChannel >= UpdateChannel.Preview => true,
                        //    "dev" when maxChannel >= UpdateChannel.Development => true,
                        //    _ => false
                        //}
                )
                .Select(x => x.Block);

            return string.Join(Environment.NewLine + Environment.NewLine, blocks);
        }
    }
}
