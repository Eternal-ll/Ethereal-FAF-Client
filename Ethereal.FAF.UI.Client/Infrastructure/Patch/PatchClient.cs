using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using static Ethereal.FAF.API.Client.BuilderExtensions;

namespace Ethereal.FAF.UI.Client.Infrastructure.Patch
{
    public class PatchClient
    {
        private readonly ILogger Logger;
        private readonly IServiceProvider ServiceProvider;
        private readonly ITokenProvider TokenProvider;

        private readonly FileSystemWatcher[] FolderWatchers;
        private readonly DirectoryInfo PatchDirectory;

        private readonly Dictionary<string, string> FilesMD5 = new();

        private bool IsFilesChanged;
        private bool DownloadingFiles;

        public PatchClient(ILogger<PatchClient> logger, IServiceProvider serviceProvider, string patchFolder, ITokenProvider tokenProvider)
        {
            var baseDirectory = patchFolder;
            logger.LogTrace("Initializing with base directory: [{}]", baseDirectory);
            Logger = logger;
            var bin = baseDirectory + "bin";
            var gamedata = baseDirectory + "gamedata";
            try
            {
                var binDirectory = new DirectoryInfo(bin);
                var gamedataDirectory = new DirectoryInfo(gamedata);
                PatchDirectory = new DirectoryInfo(baseDirectory);
                if (!binDirectory.Exists)
                {
                    binDirectory.Create();
                }
                if (!gamedataDirectory.Exists)
                {
                    gamedataDirectory.Create();
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Cant initialize path folders [{}]", ex);
                return;
            }
            finally
            {
                FolderWatchers = new FileSystemWatcher[]
                {
                    new FileSystemWatcher()
                    {
                        Path = bin,
                        Filter = "*.*",
                        EnableRaisingEvents = true,
                        NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            },
                    new FileSystemWatcher()
                    {
                        Path = gamedata,
                        Filter = "*.*",
                        EnableRaisingEvents = true,
                        NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    }
                };
                logger.LogTrace("Initialized with base directory: [{}]", baseDirectory);
            }
            ServiceProvider = serviceProvider;
            TokenProvider = tokenProvider;
        }

        public async Task InitializeWatchers(IProgress<string> progress = null)
        {
            var tasks = new Task[FolderWatchers.Length];
            for (int i = 0; i < FolderWatchers.Length; i++)
            {
                var watcher = FolderWatchers[i];
                tasks[i] = Task.Run(() => ProcessPatchFiles(watcher.Path, progress));
            }
            Task.WaitAll(tasks);
            StartWatchers();
            IsFilesChanged = true;
        }


        public async Task UpdatePatch(FeaturedMod mod, int version = 0, bool forceCheck = false, CancellationToken cancellationToken = default, IProgress<string> progress = null)
        {
            progress?.Report("Confirming patch");
            var accessToken = TokenProvider.GetToken();
            var path = $"featuredMods\\{(int)mod}\\files\\{(version == 0 ? "latest" : version)}";
            Logger.LogTrace("Checking patch for [{}] version [{}] with forced confirmation [{}]", mod, version, forceCheck);
            // last patch url was the same
            if (!IsFilesChanged && !forceCheck)
            {
                Logger.LogTrace("All files up to date");
                progress?.Report("All files up to date");
                return;
            }
            var apiClient = ServiceProvider.GetService<IFeaturedFilesClient>();
            var apiResponse = version == 0 ? 
                await apiClient.GetLatestAsync((int)mod, accessToken, cancellationToken) :
                await apiClient.GetAsync((int)mod, version, accessToken, cancellationToken);
            if (!apiResponse.IsSuccessStatusCode)
            {

                return;
            }
            var files = apiResponse.Content.Data;
            var requiredFiles = files.Where(f=>!FilesMD5.TryGetValue(f.Group.ToLower() + '\\' + f.Name.ToLower(), out var cached) || cached != f.MD5).ToArray();
            if (requiredFiles.Length == 0)
            {
                Logger.LogTrace("All files up to date");
                progress?.Report("All files up to date");
                return;
            }
            StopWatchers();
            apiClient = null;
            var contentClient = ServiceProvider.GetService<IContentClient>();
            Logger.LogTrace($"[{requiredFiles.Length}] of [{files.Length}] files required to update");
            progress?.Report($"[{requiredFiles.Length}] of [{files.Length}] files required to update");
            int i = 1;
            foreach (var file in requiredFiles)
            {
                var origP = file.Group + '\\' + file.Name;
                var p = file.Group.ToLower() + '\\' + file.Name.ToLower();
                var url = new Uri(file.CacheableUrl);
                var fileResponse = await contentClient.GetFileStreamAsync(url.LocalPath[1..], accessToken, file.HmacToken, cancellationToken);
                if (!fileResponse.IsSuccessStatusCode)
                {
                    Logger.LogError($"Failed to download [{origP}] [{i}] of [{requiredFiles.Length}]");
                    progress?.Report($"Failed to download [{origP}] [{i}] of [{requiredFiles.Length}]");
                    continue;
                }
                Logger.LogTrace($"Downloading [{origP}] [{i}] of [{requiredFiles.Length}]");
                progress?.Report($"Downloading [{origP}] [{i}] of [{requiredFiles.Length}]");
                using var fs = new FileStream(PatchDirectory.FullName + '\\' + origP, FileMode.Create);
                await fileResponse.Content.CopyToAsync(fs, cancellationToken);
                i++;

                // update MD5 of local file
                if(!FilesMD5.TryAdd(p, file.MD5))
                {
                    FilesMD5[p] = file.MD5;
                }
            }
            Logger.LogTrace($"Downloaded [{i}] of [{files.Length}]");
            progress?.Report($"Downloaded [{i}] of [{files.Length}]");
            IsFilesChanged = false;
            StartWatchers();
        }

        private void StopWatchers()
        {
            foreach (var watcher in FolderWatchers)
            {
                watcher.Changed -= OnFileChanged;
                watcher.Deleted -= OnFileDeleted;
            }
        }
        private void StartWatchers()
        {
            foreach (var watcher in FolderWatchers)
            {
                watcher.Changed += OnFileChanged;
                watcher.Deleted += OnFileDeleted;
            }
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            Logger.LogTrace("File deleted [{}] change type [{}]", e.FullPath, e.ChangeType);
            var data = e.FullPath.Split('\\');
            var group = data[^2];
            var path = group + '\\' + e.Name;   
            FilesMD5.Remove(path);
            IsFilesChanged = true;
        }

        private string LastFileInWork;
        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            Logger.LogTrace("File edited [{}] change type [{}]", e.FullPath, e.ChangeType);
            var data = e.FullPath.Split('\\');
            var group = data[^2];
            var path = group + '\\' + e.Name;
            path = path.ToLower();
            if (LastFileInWork == path) return;
            LastFileInWork = path;
            if (FilesMD5.TryGetValue(path, out var cached))
            {
                var md5 = await CalculateMD5(e.FullPath);
                if (cached != md5)
                {
                    FilesMD5[path] = md5;
                    IsFilesChanged = true;
                }
            }
            LastFileInWork = null;
        }

        private async Task ProcessPatchFiles(string path, IProgress<string> progress = null)
        {
            var patch = new DirectoryInfo(path);
            Logger.LogTrace("Processing files in [{}]", patch.FullName);
            foreach (var file in patch.EnumerateFiles())
            {
                var key = file.Directory.Name + '\\' + file.Name;
                var md5 = await CalculateMD5(file.FullName);
                if (!FilesMD5.TryAdd(key, md5))
                {
                    Logger.LogTrace("File updated in [{}] [{}] with old MD5 [{}] to [{}]", key, file.Name, FilesMD5[key], md5);
                    FilesMD5[key] = md5;
                    return;
                }
                progress?.Report($"File added in [{key}] with MD5 [{md5[..10]}..]");
                Logger.LogTrace("File added in [{}] [{}] with MD5 [{}]", key, file.Name, md5);
            }
        }
        static async Task<string> CalculateMD5(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = new FileStream(path: filename, FileMode.Open);
            var hash = await md5.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
