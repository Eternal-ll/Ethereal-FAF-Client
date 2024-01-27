using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Patch
{
    /// <summary>
    /// 
    /// </summary>
    public class PatchWatcher
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ILogger _logger;

        public readonly Dictionary<string, string> FilesMD5 = new();

        private bool IsFilesChanged = true;

        private FileSystemWatcher[] PatchWatchers;

        public PatchWatcher(ILogger<PatchWatcher> logger, ISettingsManager settingsManager)
        {
            _logger = logger;
            _settingsManager = settingsManager;
        }

        public bool IsChanged()
        {
            var failed = false;
            foreach (var roots in PatchWatchers?.Select(x => x.Path) ?? Array.Empty<string>())
            {
                var directory = new DirectoryInfo(roots);
                if (!directory.Exists)
                {
                    foreach (var key in FilesMD5.Keys.Where(x => x.StartsWith(directory.Name)).ToArray()) 
                    {
                        FilesMD5.Remove(key);
                    }
                    failed = true;
                }
            }
            if (failed) return false;
            return IsFilesChanged;
        }

        public void SetState(bool filesChanged) => IsFilesChanged = filesChanged;

        public void InitializePatchWatchers()
        {
            if (PatchWatchers != null) return;
            var root = _settingsManager.Settings.FAForeverLocation;
            var bin = Path.Combine(root, ForgedAllianceHelper.BinFolder);
            var gamedata = Path.Combine(root, ForgedAllianceHelper.GamedataFolder);
            if (!Directory.Exists(bin)) Directory.CreateDirectory(bin);
            if (!Directory.Exists(gamedata)) Directory.CreateDirectory(gamedata);
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
        private bool Initializde = false;
        public async void InitializePatchWatching()
        {
            if (Initializde) return;
            Initializde = true;
            var tasks = new Task[PatchWatchers.Length];
            for (int i = 0; i < PatchWatchers.Length; i++)
            {
                var watcher = PatchWatchers[i];
                tasks[i] = Task.Run(() => ProcessPatchFiles(watcher.Path));
            }
            Task.WaitAll(tasks);
            StartWatchers();
        }

        public void StopWatchers()
        {
            if (PatchWatchers is null)
            {
                InitializePatchWatchers();
            }
            foreach (var watcher in PatchWatchers)
            {
                watcher.Changed -= OnFileChanged;
                watcher.Deleted -= OnFileDeleted;
            }
            _logger.LogTrace("File watchers stopped");
        }
        public void StartWatchers()
        {
            if (PatchWatchers is null)
            {
                InitializePatchWatchers();
            }
            var root = _settingsManager.Settings.FAForeverLocation;
            var bin = Path.Combine(root, ForgedAllianceHelper.BinFolder);
            var gamedata = Path.Combine(root, ForgedAllianceHelper.GamedataFolder);
            foreach (var watcher in PatchWatchers)
            {
                watcher.Changed += OnFileChanged;
                watcher.Deleted += OnFileDeleted;
            }
            _logger.LogTrace("File watchers started");
        }

        private string LastFileInWork;
        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("File changed: {File} {event}", e.FullPath, e.ChangeType.ToString());
            if (LastFileInWork == e.Name)
            {
                LastFileInWork = null;
                return;
            }
            LastFileInWork = e.Name;
            _logger.LogTrace("File [{edited}] [{change}]", e.FullPath, e.ChangeType);
            var data = e.FullPath.Split('\\', '/');
            var group = data[^2];
            var path = Path.Combine(group, e.Name);
            path = path.ToLower();
            if (!File.Exists(e.FullPath))
            {
                FilesMD5.Remove(path);
                IsFilesChanged = true;
            }
            try
            {
                var md5 = await CalculateMD5(e.FullPath);
                AddOrUpdate(path, md5);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            finally
            {
                //AddOrUpdate(path, null);
                IsFilesChanged = true;
            }
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("File deleted: {File} {event}", e.FullPath, e.ChangeType.ToString());
            var data = e.FullPath.Split('\\', '/');
            var group = data[^2];
            var path = Path.Combine(group, e.Name);
            _logger.LogTrace("File [{deleted}] [{changed}]", path, e.ChangeType);
            if (!Path.HasExtension(e.FullPath))
            {
                foreach (var key in FilesMD5.Keys.Where(x => x.StartsWith(e.Name)).ToArray())
                {
                    FilesMD5.Remove(key);
                }
            }
            else
            {
                if (FilesMD5.Remove(path.ToLower()))
                {
                    _logger.LogTrace("File removed from MD5 dictionary [{removed}] as [{path}]", e.FullPath, path);
                }
                else
                {
                    _logger.LogWarning("File [{path}] not found in MD5 dictionary", path);
                }
            }
            IsFilesChanged = true;
        }

        private async Task ProcessPatchFiles(string path)
        {
            var patch = new DirectoryInfo(path);
            if (!Directory.Exists(path))
            {
                return;
            }
            _logger.LogTrace("Processing files in [{file}]", patch.FullName);
            foreach (var file in patch.EnumerateFiles())
            {
                var key = Path.Combine(file.Directory.Name, file.Name);
                if (!File.Exists(file.FullName))
                {
                    _logger.LogWarning("Calculating MD5 for removed file [{file}]", file.FullName);
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
        public bool AddOrUpdate(string file, string md5)
        {
            var lower = file.ToLower();
            if (!FilesMD5.TryAdd(lower, md5))
            {
                _logger.LogTrace("File [{key}] MD5 updated from [{old}] to [{new}]", file, FilesMD5[lower], md5);
                FilesMD5[lower] = md5;
                return false;
            }
            _logger.LogTrace("File [{key}] added with MD5 [{md5}]", file, md5);
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
