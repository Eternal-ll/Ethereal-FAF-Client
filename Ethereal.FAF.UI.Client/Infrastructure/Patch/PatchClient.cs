using Ethereal.FAF.API.Client;
using FAF.Domain.LobbyServer.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Patch
{
    public class PatchClient
    {
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

        private readonly ILogger Logger;
        private readonly ITokenProvider TokenProvider;
        private readonly IConfiguration Configuration;
        private readonly IFafApiClient ApiClient;
        private readonly IFafContentClient ContentClient;

        private FileSystemWatcher[] PatchWatchers;
        private readonly FileSystemWatcher PatchWatcher;
        private readonly Dictionary<string, string> FilesMD5 = new();

        public string Patch => Configuration.GetValue<string>("Paths:Patch");
        public string Bin => Path.Combine(Configuration.GetValue<string>("Paths:Patch"), "bin");
        public string Gamedata => Path.Combine(Configuration.GetValue<string>("Paths:Patch"), "gamedata");

        private bool IsFilesChanged = true;

        public PatchClient(ILogger<PatchClient> logger, ITokenProvider tokenProvider, IConfiguration configuration, IFafApiClient apiClient, IFafContentClient contentClient)
        {
            Logger = logger;
            TokenProvider = tokenProvider;
            Configuration = configuration;
            ApiClient = apiClient;
            ContentClient = contentClient;

            InitializePatchWatchers();
        }

        private void InitializePatchWatchers()
        {
            var bin = Bin;
            var gamedata = Gamedata;
            if (!Directory.Exists(bin)) Directory.CreateDirectory(bin);
            if (!Directory.Exists(gamedata)) Directory.CreateDirectory(gamedata);

            if (PatchWatchers is not null)
            {
                StopWatchers();
                foreach (var watcher in PatchWatchers)
                {
                    watcher.Dispose();
                }
                PatchWatchers = null;
            }

            PatchWatchers = new FileSystemWatcher[]
            {
                new FileSystemWatcher()
                {
                    Path = bin,
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
                },
                new FileSystemWatcher()
                {
                    Path = gamedata,
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                }
            };
        }

        public bool CopyOriginalFilesToFAForeverPatch(string game = null)
        {
            game = Configuration.GetValue<string>("Paths:Game");
            if (Directory.Exists(Bin)) Directory.CreateDirectory(Bin);
            if (Directory.Exists(Gamedata)) Directory.CreateDirectory(Gamedata);
            foreach (var item in FilesToCopy)
            {
                var file = Path.Combine(game, item);
                var target = Path.Combine(Patch, item);
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
                File.Copy(file, Path.Combine(Patch, target));
                Logger.LogTrace("File copied [{file}] to [{target}]", file, target);
            }
            return true;
        }

        public void InitializePatchWatching()
        {
            var tasks = new Task[PatchWatchers.Length];
            for (int i = 0; i < PatchWatchers.Length; i++)
            {
                var watcher = PatchWatchers[i];
                tasks[i] = Task.Run(() => ProcessPatchFiles(watcher.Path));
            }
            Task.WaitAll(tasks);
            StartWatchers();
        }

        private FeaturedMod LatestFeaturedMod;
        public async Task UpdatePatch(FeaturedMod mod, int version = 0, bool forceCheck = false, CancellationToken cancellationToken = default, IProgress<string> progress = null)
        {
            var path = $"featuredMods\\{(int)mod}\\files\\{(version == 0 ? "latest" : version)}";
            Logger.LogTrace("Patch confirmation...");
            Logger.LogTrace("Latest featured mod: [{mod}]", LatestFeaturedMod);
            Logger.LogTrace("Current featured mod: [{mod}]", mod);
            Logger.LogTrace("Force patch confirmation: [{force}]", forceCheck);
            Logger.LogTrace("Files changed: [{changed}]", IsFilesChanged);
            if (!IsFilesChanged && !forceCheck && LatestFeaturedMod == mod)
            {
                Logger.LogTrace("Confirmation skipped. All files up to date");
                progress?.Report("Confirmation skipped. All files up to date");
                return;
            }
            CopyOriginalFilesToFAForeverPatch();
            progress?.Report("Confirming patch from API");
            var accessToken = TokenProvider.GetToken();
            var apiResponse = version == 0 ? 
                await ApiClient.GetLatestAsync((int)mod, accessToken, cancellationToken) :
                await ApiClient.GetAsync((int)mod, version, accessToken, cancellationToken);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw apiResponse.Error;
            }
            LatestFeaturedMod = mod;
            var files = apiResponse.Content.Data;
            var requiredFiles = files
                .Where(f => !FilesMD5.TryGetValue(Path.Combine(f.Group.ToLower(), f.Name.ToLower()), out var cached) || cached != f.MD5)
                .ToArray();
            if (requiredFiles.Length == 0)
            {
                Logger.LogInformation("Confirmed from API. All files up to date");
                progress?.Report("Confirmed from API. All files up to date");
                IsFilesChanged = false;
                return;
            }
            StopWatchers();
            Logger.LogTrace("Downloading [{required}] out of [{total}] files", requiredFiles.Length, files.Length);
            for (int i = 1; i <= requiredFiles.Length; i++)
            {
                var file = requiredFiles[i - 1];
                var groupfile = Path.Combine(file.Group, file.Name);
                var url = new Uri(file.CacheableUrl);
                var fileResponse = await ContentClient.GetFileStreamAsync(url.LocalPath[1..], accessToken, file.HmacToken, cancellationToken);
                if (!fileResponse.IsSuccessStatusCode)
                {
                    Logger.LogError($"[{fileResponse.StatusCode}] Failed to download [{groupfile}] [{i}] of [{requiredFiles.Length}]");
                    continue;
                }
                Logger.LogTrace($"Downloading [{groupfile}] [{i}] out of [{requiredFiles.Length}]");
                progress?.Report($"Downloading [{groupfile}] [{i}] out of [{requiredFiles.Length}]");
                using var fs = new FileStream(Path.Combine(Patch, groupfile), FileMode.Create);
                await fileResponse.Content.CopyToAsync(fs, cancellationToken);
                await fileResponse.Content.DisposeAsync();
                fileResponse.Content.Close();
                AddOrUpdate(groupfile, file.MD5);
            }
            Logger.LogInformation("Updated from API. All files up to date");
            progress?.Report("Updated from API. All files up to date");
            IsFilesChanged = false;
            StartWatchers();
        }

        private void StopWatchers()
        {
            foreach (var watcher in PatchWatchers)
            {
                watcher.Changed -= OnFileChanged;
                watcher.Deleted -= OnFileDeleted;
            }
            Logger.LogTrace("File watchers stopped");
        }
        private void StartWatchers()
        {
            foreach (var watcher in PatchWatchers)
            {
                watcher.Changed += OnFileChanged;
                watcher.Deleted += OnFileDeleted;
            }
            Logger.LogTrace("File watchers started");
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            var data = e.FullPath.Split('\\','/');
            var group = data[^2];
            var path = Path.Combine(group, e.Name);
            Logger.LogTrace("File [{deleted}] [{changed}]", path, e.ChangeType);
            if (FilesMD5.Remove(path.ToLower()))
            {
                Logger.LogTrace("File removed from MD5 dictionary [{removed}] as [{path}]", e.FullPath, path);
            }
            else
            {
                Logger.LogWarning("File [{path}] not found in MD5 dictionary", path);
            }
            IsFilesChanged = true;
        }

        private string LastFileInWork;
        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (LastFileInWork == e.Name)
            {
                LastFileInWork = null;
                return;
            }
            LastFileInWork = e.Name;
            Logger.LogTrace("File [{edited}] [{change}]", e.FullPath, e.ChangeType);
            var data = e.FullPath.Split('\\','/');
            var group = data[^2];
            var path = Path.Combine(group, e.Name);
            path = path.ToLower();
            var md5 = await CalculateMD5(e.FullPath);
            AddOrUpdate(path, md5);
            IsFilesChanged = true;
        }

        private async Task ProcessPatchFiles(string path)
        {
            var patch = new DirectoryInfo(path);
            if (!Directory.Exists(path))
            {
                return;
            }
            Logger.LogTrace("Processing files in [{file}]", patch.FullName);
            foreach (var file in patch.EnumerateFiles())
            {
                var key = Path.Combine(file.Directory.Name, file.Name);
                if (!File.Exists(file.FullName))
                {
                    Logger.LogWarning("Calculating MD5 for removed file [{file}]", file.FullName);
                    continue;
                }
                var md5 = await CalculateMD5(file.FullName);
                AddOrUpdate(key, md5);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">group\\file</param>
        /// <param name="md5"></param>
        /// <returns>Returns <see cref="true"/> if added, <see cref="false"/> if updated</returns>
        private bool AddOrUpdate(string file, string md5)
        {
            var lower = file.ToLower();
            if (!FilesMD5.TryAdd(lower, md5))
            {
                Logger.LogTrace("File [{key}] MD5 updated from [{old}] to [{new}]", file, FilesMD5[lower], md5);
                FilesMD5[lower] = md5;
                return false;
            }
            Logger.LogTrace("File [{key}] added with MD5 [{md5}]", file, md5);
            return true;
        }
        public static async Task<string> CalculateMD5(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = new FileStream(path: filename, FileMode.Open);
            var hash = await md5.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
