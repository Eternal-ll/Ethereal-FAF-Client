using Ethereal.FAF.UI.Client.Infrastructure.Helper;
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
        private readonly INeroxisMapGenerator _neroxisMapGenerator;
        private readonly ClientManager _clientManager;
        private readonly ILogger _logger;

        public FafMapsService(IDownloadService downloadService, ISettingsManager settingsManager, ILogger<FafMapsService> logger, ClientManager clientManager, INeroxisMapGenerator neroxisMapGenerator)
        {
            _downloadService = downloadService;
            _settingsManager = settingsManager;
            _logger = logger;
            _clientManager = clientManager;
            _neroxisMapGenerator = neroxisMapGenerator;
        }

        public async Task EnsureMapExist(string map, IProgress<ProgressReport> progress = null, CancellationToken cancellationToken = default)
        {
            if (IsExist(map)) return;
            progress?.Report(new(-1, "Map service", "Map not found", true));

            var folder = _settingsManager.Settings.ForgedAllianceMapsLocation;
            if (_neroxisMapGenerator.IsNeroxisGeneratedMap(map))
            {
                await _neroxisMapGenerator.GenerateMapAsync(map, folder, progress, cancellationToken);
            }
            else
            {
                await DownloadCustomMapAsync(map, folder, progress, cancellationToken);
            }
        }
        private async Task DownloadCustomMapAsync(
            string map,
            string folder,
            IProgress<ProgressReport> progress = null,
            CancellationToken cancellationToken = default)
        {
            var ub = new UriBuilder(_clientManager.GetServer().Content)
            {
                Path = $"/maps/{map}.zip"
            };
            var uri = ub.ToString();

            FileInfo mapArchive = new(Path.Combine(folder, Path.GetFileName(uri)));
            if (!mapArchive.Directory.Exists) mapArchive.Directory.Create();

            await _downloadService.DownloadToFileAsync(uri, mapArchive.FullName, progress, "FafContent", cancellationToken);
            await ArchiveHelper.Extract(mapArchive.FullName, folder, progress);

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
