using Castle.DynamicProxy;
using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Ethereal.FAF.UI.Client.Models.Progress;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Patch
{
    public class PatchClient : IPatchClient
    {
        private readonly ILogger _logger;
        private readonly IFafApiClient _fafApiClient;
        private readonly IDownloadService _downloadService;
        private readonly ISettingsManager _settingsManager;
        private readonly PatchWatcher _patchWatcher;

        public PatchClient(
            ILogger<PatchClient> logger,
            IFafApiClient fafApiClient,
            PatchWatcher patchWatcher,
            IDownloadService downloadService,
            ISettingsManager settingsManager)
        {
            _logger = logger;
            _patchWatcher = patchWatcher;
            _fafApiClient = fafApiClient;
            _downloadService = downloadService;
            _settingsManager = settingsManager;
        }

        public bool CopyOriginalFilesToFAForeverPatch(string targetDirectory)
        {
            var game = _settingsManager.Settings.ForgedAllianceLocation;
            var patch = targetDirectory;
            var bin = Path.Combine(targetDirectory, ForgedAllianceHelper.BinFolder);
            if (!Directory.Exists(bin)) Directory.CreateDirectory(bin);
            foreach (var item in ForgedAllianceHelper.FilesToCopy)
            {
                var file = Path.Combine(game, item);
                var target = Path.Combine(patch, item);
                if (File.Exists(target))
                {
                    continue;
                }
                if (!File.Exists(file))
                {
                    _logger.LogError("File not found [{file}]", file);
                    return false;
                }
                File.Copy(file, Path.Combine(patch, target));
                _logger.LogTrace("File copied [{file}] to [{target}]", file, target);
            }
            return true;
        }

        private static string LatestFeaturedMod;
        private static string LatestHost;
        public async Task EnsurePatchExist(string mod, string root, int version = 0, bool forceCheck = false,
            IProgress<ProgressReport> progress = null,
            CancellationToken cancellationToken = default)
        {
            progress?.Report(new(0, "PatchClient", "Fetching featured mod..."));
            var featuredMods = await _fafApiClient.GetFeaturedMods($"technicalName=='{mod}'");
            await featuredMods.EnsureSuccessStatusCodeAsync();
            var featuredMod = featuredMods.Content.Data.FirstOrDefault() ?? throw new InvalidOperationException("Featured mod not found");
            _logger.LogTrace("Patch confirmation...");
            _logger.LogTrace("Latest  featured mod: [{mod}]", LatestFeaturedMod);
            _logger.LogTrace("Current featured mod: [{mod}]", mod);
            _logger.LogTrace("Force patch confirmation: [{force}]", forceCheck);
            _logger.LogTrace("Files changed: [{changed}]", _patchWatcher.IsChanged());
            if (!_patchWatcher.IsChanged() && !forceCheck && LatestFeaturedMod == mod)
            {
                _logger.LogTrace("Confirmation skipped. All files up to date");
                //progress?.Report("Confirmation skipped. All files up to date");
                return;
            }
            progress?.Report(new(0, "PatchClient", "Copying original game files..."));
            CopyOriginalFilesToFAForeverPatch(root);
            progress?.Report(new(0, "PatchClient", "Fetching patch files..."));
            var apiResponse = version == 0 ? 
                await _fafApiClient.GetLatestAsync(featuredMod.Id, cancellationToken) :
                await _fafApiClient.GetAsync(featuredMod.Id, version, cancellationToken);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw apiResponse.Error;
            }
            LatestFeaturedMod = mod;
            var files = apiResponse.Content.Data;
            var requiredFiles = files
                .Where(f => !_patchWatcher.FilesMD5.TryGetValue(Path.Combine(f.Group.ToLower(), f.Name.ToLower()), out var cached) || cached != f.MD5)
                .ToArray();
            if (requiredFiles.Length == 0)
            {
                _logger.LogInformation("Confirmed from API. All files up to date");
                //progress?.Report("Confirmed from API. All files up to date");
                _patchWatcher.SetState(false);
                return;
            }
            _patchWatcher.StopWatchers();
            _logger.LogTrace("Downloading [{required}] out of [{total}] files", requiredFiles.Length, files.Length);
            foreach (var file in requiredFiles)
            {
                var groupfile = Path.Combine(file.Group, file.Name);
                var targetFile = Path.Combine(root, groupfile);

                var md5 = !File.Exists(targetFile) ? null : await PatchWatcher.CalculateMD5(targetFile);
                if (!File.Exists(targetFile) || md5 != file.MD5)
                {
                    await _downloadService.DownloadToFileAsync(file.Url, targetFile, progress, "FafContent", cancellationToken);
                    _patchWatcher.AddOrUpdate(groupfile, file.MD5);
                }
            }
            _logger.LogInformation("Updated from API. All files up to date");
            //progress?.Report("Updated from API. All files up to date");
            _patchWatcher.SetState(false);
            _patchWatcher.StartWatchers();
        }
    }
}
