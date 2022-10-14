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
            logger.LogTrace("Initializing on [{directory}]", patchFolder);
            Logger = logger;
            var bin = Path.Combine(patchFolder, "bin");
            var gamedata = Path.Combine(patchFolder, "gamedata");
            try
            {
                var binDirectory = new DirectoryInfo(bin);
                var gamedataDirectory = new DirectoryInfo(gamedata);
                PatchDirectory = new DirectoryInfo(patchFolder);
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
                logger.LogError("Cant initialize on [{directory}] with exception \n[{exception}]", patchFolder, ex);
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
                logger.LogTrace("Initialized on [{directory}]", patchFolder);
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

        private FeaturedMod LatestFeaturedMod;
        public async Task UpdatePatch(FeaturedMod mod, int version = 0, bool forceCheck = false, CancellationToken cancellationToken = default, IProgress<string> progress = null)
        {
            progress?.Report("Confirming patch");
            var accessToken = TokenProvider.GetToken();
            var path = $"featuredMods\\{(int)mod}\\files\\{(version == 0 ? "latest" : version)}";
            Logger.LogTrace("Checking patch for [{}] version [{}] with forced confirmation [{}]", mod, version, forceCheck);
            // last patch url was the same
            if (!IsFilesChanged && !forceCheck && LatestFeaturedMod == mod)
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
            LatestFeaturedMod = mod;
            var files = apiResponse.Content.Data;
            var requiredFiles = files
                .Where(f => 
                !FilesMD5.TryGetValue(Path.Combine(f.Group.ToLower(), f.Name.ToLower()), out var cached) ||
                cached != f.MD5)
                .ToArray();
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
                var origP = Path.Combine(file.Group, file.Name);
                var p = origP.ToLower();
                var url = new Uri(file.CacheableUrl);
                var fileResponse = await contentClient.GetFileStreamAsync(url.LocalPath[1..], accessToken, file.HmacToken, cancellationToken);
                if (!fileResponse.IsSuccessStatusCode)
                {
                    Logger.LogError($"Failed to download [{origP}] [{i}] of [{requiredFiles.Length}]");
                    continue;
                }
                Logger.LogTrace($"Downloading [{origP}] [{i}] of [{requiredFiles.Length}]");
                progress?.Report($"Downloading [{origP}] [{i}] of [{requiredFiles.Length}]");
                using var fs = new FileStream(Path.Combine(PatchDirectory.FullName, origP), FileMode.Create);
                await fileResponse.Content.CopyToAsync(fs, cancellationToken);
                i++;

                // update MD5 of local file
                if(!FilesMD5.TryAdd(p, file.MD5))
                {
                    FilesMD5[p] = file.MD5;
                }
            }
            Logger.LogTrace($"Downloaded [{i}] of [{requiredFiles.Length}]");
            progress?.Report($"Downloaded [{i}] of [{requiredFiles.Length}]");
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
            Logger.LogTrace("File deleted [{deleted}] change type [{changed}]", e.FullPath, e.ChangeType);
            var data = e.FullPath.Split('\\','/');
            var group = data[^2];
            var path = Path.Combine(group, e.Name);
            if (FilesMD5.Remove(path.ToLower()))
            {
                Logger.LogTrace("File removed from MD5 dictionary [{removed}] as [{path}]", e.FullPath, path);
            }
            else
            {
                Logger.LogWarning("File wasnt removed from MD5 dictionary [{path}]", path);
            }
            IsFilesChanged = true;
        }

        private string LastFileInWork;
        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            Logger.LogTrace("File edited [{edited}] change type [{change}]", e.FullPath, e.ChangeType);
            var data = e.FullPath.Split('\\','/');
            var group = data[^2];
            var path = Path.Combine(group, e.Name);
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
            Logger.LogTrace("Processing files in [{file}]", patch.FullName);
            foreach (var file in patch.EnumerateFiles())
            {
                var key = Path.Combine(file.Directory.Name, file.Name);
                var md5 = await CalculateMD5(file.FullName);
                if (!FilesMD5.TryAdd(key.ToLower(), md5))
                {
                    Logger.LogTrace("File updated in [{key}] with old MD5 [{md5old}] to [{md5new}]", key, FilesMD5[key], md5);
                    FilesMD5[key.ToLower()] = md5;
                    return;
                }
                progress?.Report($"File added in [{key}] with MD5 [{md5[..10]}..]");
                Logger.LogTrace("File added in [{key}] with MD5 [{ms5}]", key, md5);
            }
        }
        static async Task<string> CalculateMD5(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = new FileStream(path: filename, FileMode.Open);
            var hash = await md5.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public void CopyOriginalBinFilesToPatchFolder(string gameFolder)
        {
            var files = FilesToCopy;
            foreach (var file in files)
            {
                var source = Path.Combine(gameFolder, file);
                if (File.Exists(source))
                {
                    Logger.LogError("File not exists [{file}]", source);
                    continue;
                }
                var target = Path.Combine(gameFolder, file);
                File.Copy(source, target);
            }
        }

        public static readonly string[] FilesToCopy =
        {
            "bin/BsSndRpt.exe",
            "bin/BugSplat.dll",
            "bin/BugSplatRc.dll",
            "bin/DbgHelp.dll",
            "bin/GDFBinary.dll",
            "bin/msvcm80.dll",
            "bin/msvcp80.dll",
            "bin/msvcr80.dll",
            "bin/SHSMP.DLL",
            "bin/sx32w.dll",
            "bin/wxmsw24u-vs80.dll",
            "bin/zlibwapi.dll"
        };
    }
}
