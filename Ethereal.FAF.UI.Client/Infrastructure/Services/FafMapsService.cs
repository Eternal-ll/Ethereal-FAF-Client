﻿using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.MapGen;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Progress;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class FafMapsService : IMapsService
    {
        private readonly IDownloadService _downloadService;
        private readonly ISettingsManager _settingsManager;
        private readonly ILogger _logger;

        public FafMapsService(IDownloadService downloadService, ISettingsManager settingsManager, ILogger<FafMapsService> logger)
        {
            _downloadService = downloadService;
            _settingsManager = settingsManager;
            _logger = logger;
        }

        public async Task EnsureMapExist(string map, IProgress<ProgressReport> progress = null, CancellationToken cancellationToken = default)
        {
            if (IsExist(map)) return;
            //https://content.faforever.com/maps/astro_crater_battles_4x4_rich_huge.v0004.zip
            var uri = $"https://content.faforever.com/maps/{map}.zip";

            FileInfo mapArchive = new(Path.Combine(_settingsManager.Settings.ForgedAllianceMapsLocation, Path.GetFileName(uri)));
            if (!mapArchive.Directory.Exists) mapArchive.Directory.Create();

            await _downloadService.DownloadToFileAsync(uri, mapArchive.FullName, null, "FafContent", cancellationToken);
            await ArchiveHelper.Extract(mapArchive.FullName, _settingsManager.Settings.ForgedAllianceMapsLocation, progress);

            mapArchive.Delete();
        }
        public bool IsExist(string map)
        {
            _logger.LogInformation("[{map}] Confirming map exist...", map);
            var folder = Path.Combine(_settingsManager.Settings.ForgedAllianceMapsLocation, map);
            if (!Directory.Exists(folder))
            {
                _logger.LogInformation("[{map}] Folder not exist [{folder}]", map, folder);
                return false;
            }
            var mapname = MapGenerator.IsGeneratedMap(map) ? map : Path.GetFileNameWithoutExtension(map);
            var scenario = Path.Combine(folder, mapname + "_scenario.lua");
            var scmap = Path.Combine(folder, mapname + ".scmap");
            var script = Path.Combine(folder, mapname + "_script.lua");
            var save = Path.Combine(folder, mapname + "_save.lua");
            var files = new string[] { scenario, scmap, script, save };
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    _logger.LogInformation("[{map}] Missing reqiuired map file [{file}]", map, file);
                    return false;
                }
            }
            _logger.LogInformation("[{map}] Map existance confirmed", map);
            return true;
        }
    }
}