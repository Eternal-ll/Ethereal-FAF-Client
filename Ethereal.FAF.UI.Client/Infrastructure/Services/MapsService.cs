using Ethereal.FAF.API.Client;
using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.MapGen;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    public class MapsService
    {
        private readonly IHttpClientFactory HttpClientFactory;
        private readonly IConfiguration Configuration;
        private readonly ILogger Logger;

        private readonly MapGenerator MapGenerator;

        public MapsService(IHttpClientFactory httpClientFactory, ILogger<MapsService> logger, IConfiguration configuration, MapGenerator mapGenerator)
        {
            HttpClientFactory = httpClientFactory;
            Configuration = configuration;
            Logger = logger;
            MapGenerator = mapGenerator;
        }

        public bool IsExist(string map)
        {
            Logger.LogInformation("[{map}] Confirming map exist...", map);
            var folder = Path.Combine(Configuration.GetMapsLocation(), map);
            if (!Directory.Exists(folder))
            {
                Logger.LogInformation("[{map}] Folder not exist [{folder}]", map, folder);
                return false;
            }
            var mapname = MapGenerator.IsGeneratedMap(map) ? map : Path.GetFileNameWithoutExtension(map);
            var scenario = Configuration.GetMapFile(mapname, "_scenario.lua", maps: folder);
            var scmap    = Configuration.GetMapFile(mapname, ".scmap", maps: folder);
            var script   = Configuration.GetMapFile(mapname, "_script.lua", maps: folder);
            var save     = Configuration.GetMapFile(mapname, "_save.lua", maps: folder);
            var files = new string[] { scenario, scmap, script, save };
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    Logger.LogInformation("[{map}] Missing reqiuired map file [{file}]", map, file);
                    return false;
                }
            }
            Logger.LogInformation("[{map}] Map existance confirmed", map);
            return true;
        } 
        public async Task<bool> DownloadAsync(string map, string contentUrl, string filePath = null, IProgress<string> progress = null, CancellationToken cancellationToken = default)
        {
            using var client = HttpClientFactory.CreateClient();
            var zip = map + ".zip";
            using var fs = new FileStream(zip, FileMode.Create);
            // https://content.faforever.com/maps/mayhem_of_64_acus_v2.v0002.zip 
            // TODO create faf content httpclient?
            var response = await client.GetAsync(contentUrl + (filePath ?? $"/maps/{map}.zip"), cancellationToken);
            progress?.Report($"Downloading map [{zip}]");
            await response.Content.CopyToAsync(fs, cancellationToken);
            response.Content.Dispose();
            await fs.DisposeAsync();
            fs.Close();
            progress?.Report($"Extracting map [{zip}]");
            ZipFile.ExtractToDirectory(zip, Configuration.GetMapsLocation(), true);
            File.Delete(zip);
            return true;
        }
        /// <summary>
        /// Ensures that given map exist, otherwise generates/downloads map
        /// </summary>
        /// <param name="map">Map name</param>
        /// <param name="contentClient">Content client</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task EnsureMapExistAsync(string map, IFafContentClient contentClient, CancellationToken cancellationToken = default)
        {
            if (IsExist(map)) return;
            if (MapGenerator.IsGeneratedMap(map))
            {
                await MapGenerator.GenerateMapAsync(map, null, null, cancellationToken: cancellationToken);
                return;
            }
            var temp = Path.Combine(Path.GetTempPath(), map);
            Logger.LogInformation("[{map}] Preparing temp file [{temp}]", map, temp);
            using (var fs = new FileStream(temp, FileMode.Create))
            {
                using var response = await contentClient.GetMapStreamAsync(map, cancellationToken);
                await response.EnsureSuccessStatusCodeAsync();
                Logger.LogInformation("[{map}] Downloading map archive...", map);
                await response.Content.CopyToAsync(fs, cancellationToken);
                Logger.LogInformation("[{map}] Map archive downloaded", map);
            }

            var saveLocation = Configuration.GetMapsLocation();
            if (!Directory.Exists(saveLocation)) Directory.CreateDirectory(saveLocation);

            Logger.LogInformation("[{map}] Extracting map from [{from}] to [{to}]", map, temp, saveLocation);
            ZipFile.ExtractToDirectory(temp, saveLocation, true);
            Logger.LogInformation("[{map}] Removing temp file", map);
            File.Delete(temp);
        }
    }
}
