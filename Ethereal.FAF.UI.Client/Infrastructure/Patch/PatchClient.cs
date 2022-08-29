using beta.Models.API;
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

        private readonly FileSystemWatcher[] FolderWatchers;
        private readonly DirectoryInfo PatchDirectory;

        private readonly Dictionary<string, string> FilesMD5 = new();

        private bool Initalized;

        private bool IsFilesChanged;

        public PatchClient(ILogger<PatchClient> logger, IServiceProvider serviceProvider)
        {
            var baseDirectory = @"C:\ProgramData\FAForever\";
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
                Initalized = true;
                FolderWatchers = new FileSystemWatcher[]
                {
                    new FileSystemWatcher()
                    {
                        Path = bin,
                        Filter = "*.*",
                        EnableRaisingEvents = true,
                        NotifyFilter = NotifyFilters.LastWrite
                    },
                    new FileSystemWatcher()
                    {
                        Path = gamedata,
                        Filter = "*.*",
                        EnableRaisingEvents = true,
                        NotifyFilter = NotifyFilters.LastWrite,
                    }
                };
                foreach (var watcher in FolderWatchers)
                {
                    logger.LogTrace("File watcher initialized on [{}]", watcher.Path);
                    ProcessPatchFiles(watcher.Path);
                    watcher.Changed += OnFileChanged;
                }
                logger.LogTrace("Initialized with base directory: [{}]", baseDirectory);
            }
            ServiceProvider = serviceProvider;
        }

        public async Task UpdatePatch(FeaturedMod mod, string accessToken, int version = 0, bool forceCheck = false, CancellationToken cancellationToken = default, IProgress<string> progress = null)
        {
            var path = $"featuredMods\\{(int)mod}\\files\\{(version == 0 ? "latest" : version)}";
            Logger.LogTrace("Checking patch for [{}] version [{}] with forced confirmation [{}]", mod, version, forceCheck);
            // last patch url was the same
            if (!IsFilesChanged && !forceCheck)
            {
                Logger.LogTrace("All files up to date");
                progress?.Report("All files up to date");
                return;
            }
            var client = ServiceProvider.GetService<IFeaturedFilesClient>();
            var apiResponse = version == 0 ? 
                await client.GetLatestAsync((int)mod, accessToken, cancellationToken) :
                await client.GetAsync((int)mod, version, accessToken, cancellationToken);
            if (!apiResponse.IsSuccessStatusCode)
            {

            }
            var files = apiResponse.Content.Data;
            var requiredFiles = files.Where(f=>!FilesMD5.TryGetValue(f.Group + '\\' + f.Name, out var cached) || cached != f.MD5).ToArray();
            if (requiredFiles.Length == 0)
            {
                Logger.LogTrace("All files up to date");
                progress?.Report("All files up to date");
                return;
            }
            foreach (var file in requiredFiles)
            {
                var fileResponse = await client.GetFileStreamAsync(file.CacheableUrl, accessToken, file.HmacToken, cancellationToken);
                if (!fileResponse.IsSuccessStatusCode)
                {
                    continue;
                }
                using var fs = new FileStream(PatchDirectory.FullName + '\\' + file.Group + '\\' + file.Name, FileMode.Create);
                await fileResponse.Content.CopyToAsync(fs);
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {

        }

        private void ProcessPatchFiles(string path)
        {
            var patch = new DirectoryInfo(path);
            Logger.LogTrace("Processing files in [{}]", patch.FullName);
            foreach (var file in patch.EnumerateFiles())
            {

                var key = file.DirectoryName + '\\' + file.Name;
                var md5 = CalculateMD5(file.FullName);
                if (!FilesMD5.TryAdd(key, md5))
                {
                    Logger.LogTrace("File updated in [{}] [{}] with old MD5 [{}] to [{}]", key, file.Name, FilesMD5[key], md5);
                    FilesMD5[key] = md5;
                    return;
                }
                Logger.LogTrace("File added in [{}] [{}] with MD5 [{}]", key, file.Name, md5);
            }
        }
        static string CalculateMD5(string filename)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
