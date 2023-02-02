using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Patch
{
    /// <summary>
    /// 
    /// </summary>
    public class PatchWatcher
    {
        private readonly IConfiguration Configuration;
        private readonly ILogger Logger;

        public readonly Dictionary<string, string> FilesMD5 = new();

        public bool IsFilesChanged = true;

        private FileSystemWatcher[] PatchWatchers;

        public PatchWatcher(IConfiguration configuration, ILogger<PatchWatcher> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public void InitializePatchWatchers()
        {
            if (PatchWatchers is not null)
            {
                return;
            }
            var bin = Path.Combine(Configuration.GetForgedAlliancePatchLocation(), ForgedAllianceHelper.BinFolder);
            var gamedata = Path.Combine(Configuration.GetForgedAlliancePatchLocation(), ForgedAllianceHelper.GamedataFolder);
            if (!Directory.Exists(bin)) Directory.CreateDirectory(bin);
            if (!Directory.Exists(gamedata)) Directory.CreateDirectory(gamedata);

            //if (PatchWatchers is not null)
            //{
            //    StopWatchers();
            //    foreach (var watcher in PatchWatchers)
            //    {
            //        watcher.Dispose();
            //    }
            //    PatchWatchers = null;
            //}

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
        public void InitializePatchWatching()
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
            Logger.LogTrace("File watchers stopped");
        }
        public void StartWatchers()
        {
            if (PatchWatchers is null)
            {
                InitializePatchWatchers();
            }
            foreach (var watcher in PatchWatchers)
            {
                watcher.Changed += OnFileChanged;
                watcher.Deleted += OnFileDeleted;
            }
            Logger.LogTrace("File watchers started");
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
            var data = e.FullPath.Split('\\', '/');
            var group = data[^2];
            var path = Path.Combine(group, e.Name);
            path = path.ToLower();
            try
            {
                var md5 = await CalculateMD5(e.FullPath);
                AddOrUpdate(path, md5);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
            finally
            {
                //AddOrUpdate(path, null);
                IsFilesChanged = true;
            }
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            var data = e.FullPath.Split('\\', '/');
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
        public bool AddOrUpdate(string file, string md5)
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
