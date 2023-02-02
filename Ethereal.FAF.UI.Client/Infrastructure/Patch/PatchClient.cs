using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Services;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Patch
{
    public class PatchClient
    {
        private readonly ILogger Logger;
        private readonly ITokenProvider TokenProvider;
        private readonly IConfiguration Configuration;
        private readonly IFafApiClient FafApiClient;
        private readonly IFafContentClient FafContentClient;
        private readonly PatchWatcher PatchWatcher;

        public PatchClient(
            ILogger<PatchClient> logger,
            IConfiguration configuration,
            IFafApiClient fafApiClient,
            IFafContentClient fafContentClient,
            ITokenProvider tokenProvider,
            PatchWatcher patchWatcher)
        {
            Logger = logger;
            Configuration = configuration;
            PatchWatcher = patchWatcher;
            FafApiClient = fafApiClient;
            FafContentClient = fafContentClient;
            TokenProvider = tokenProvider;
        }

        public bool CopyOriginalFilesToFAForeverPatch(string game = null)
        {
            game = Configuration.GetForgedAllianceLocation();
            var patch = Configuration.GetForgedAlliancePatchLocation();
            var bin = Path.Combine(game, ForgedAllianceHelper.BinFolder);
            var gamedata = Path.Combine(game, ForgedAllianceHelper.GamedataFolder);
            if (Directory.Exists(bin)) Directory.CreateDirectory(bin);
            if (Directory.Exists(gamedata)) Directory.CreateDirectory(gamedata);
            foreach (var item in ForgedAllianceHelper.FilesToCopy)
            {
                var file = Path.Combine(game, item);
                var target = Path.Combine(patch, item);
                if (File.Exists(target))
                {
                    //Logger.LogTrace("File already copied [{file}]", target);
                    continue;
                }
                if (!File.Exists(file))
                {
                    Logger.LogError("File not found [{file}]", file);
                    return false;
                    //throw new Exception($"File not found [{file}]");
                }
                File.Copy(file, Path.Combine(patch, target));
                Logger.LogTrace("File copied [{file}] to [{target}]", file, target);
            }
            return true;
        }

        private static FeaturedMod LatestFeaturedMod;
        private static string LatestHost;
        public async Task ConfirmPatchAsync(FeaturedMod mod, int version = 0, bool forceCheck = false,
            CancellationToken cancellationToken = default, IProgress<string> progress = null)
        {
            var path = $"featuredMods\\{(int)mod}\\files\\{(version == 0 ? "latest" : version)}";
            Logger.LogTrace("Patch confirmation...");
            Logger.LogTrace("Latest  featured mod: [{mod}]", LatestFeaturedMod);
            Logger.LogTrace("Current featured mod: [{mod}]", mod);
            Logger.LogTrace("Force patch confirmation: [{force}]", forceCheck);
            Logger.LogTrace("Files changed: [{changed}]", PatchWatcher.IsFilesChanged);
            if (!PatchWatcher.IsFilesChanged && !forceCheck && LatestFeaturedMod == mod)
            {
                Logger.LogTrace("Confirmation skipped. All files up to date");
                progress?.Report("Confirmation skipped. All files up to date");
                return;
            }
            CopyOriginalFilesToFAForeverPatch();
            progress?.Report("Confirming patch from API");
            var accessToken = await TokenProvider.GetAccessTokenAsync();
            var apiResponse = version == 0 ? 
                await FafApiClient.GetLatestAsync((int)mod, accessToken, cancellationToken) :
                await FafApiClient.GetAsync((int)mod, version, accessToken, cancellationToken);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw apiResponse.Error;
            }
            LatestFeaturedMod = mod;
            var files = apiResponse.Content.Data;
            var requiredFiles = files
                .Where(f => !PatchWatcher.FilesMD5.TryGetValue(Path.Combine(f.Group.ToLower(), f.Name.ToLower()), out var cached) || cached != f.MD5)
                .ToArray();
            if (requiredFiles.Length == 0)
            {
                Logger.LogInformation("Confirmed from API. All files up to date");
                progress?.Report("Confirmed from API. All files up to date");
                PatchWatcher.IsFilesChanged = false;
                return;
            }
            PatchWatcher.StopWatchers();
            Logger.LogTrace("Downloading [{required}] out of [{total}] files", requiredFiles.Length, files.Length);
            for (int i = 1; i <= requiredFiles.Length; i++)
            {
                var file = requiredFiles[i - 1];
                var groupfile = Path.Combine(file.Group, file.Name);
                var url = new Uri(file.CacheableUrl);
                var fileDown = Path.Combine(Configuration.GetForgedAlliancePatchLocation(), groupfile);
                
                var md5 = !File.Exists(fileDown) ? null :  await PatchWatcher.CalculateMD5(Path.Combine(Configuration.GetForgedAlliancePatchLocation(), groupfile));
                if (!File.Exists(fileDown) || md5 != file.MD5)
                {
                    var fileResponse = await FafContentClient.GetFileStreamAsync(url.LocalPath[1..], accessToken, file.HmacToken, cancellationToken);
                    if (!fileResponse.IsSuccessStatusCode)
                    {
                        Logger.LogError($"[{fileResponse.StatusCode}] Failed to download [{groupfile}] [{i}] of [{requiredFiles.Length}]");
                        continue;
                    }
                    Logger.LogTrace($"Downloading [{groupfile}] [{i}] out of [{requiredFiles.Length}]");
                    progress?.Report($"Downloading [{groupfile}] [{i}] out of [{requiredFiles.Length}]");
                    using var fs = new FileStream(Path.Combine(Configuration.GetForgedAlliancePatchLocation(), groupfile), FileMode.Create);
                    await fileResponse.Content.CopyToAsync(fs, cancellationToken);
                    await fileResponse.Content.DisposeAsync();
                    fileResponse.Content.Close();
                }
                PatchWatcher.AddOrUpdate(groupfile, file.MD5);
            }
            Logger.LogInformation("Updated from API. All files up to date");
            progress?.Report("Updated from API. All files up to date");
            PatchWatcher.IsFilesChanged = false;
            PatchWatcher.StartWatchers();
        }
    }
}
