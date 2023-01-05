using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
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

        public MapsService(IHttpClientFactory httpClientFactory, ILogger<MapsService> logger, IConfiguration configuration)
        {
            HttpClientFactory = httpClientFactory;
            Configuration = configuration;
            Logger = logger;
        }

        public bool IsExist(string map)
        {
            var folder = Path.Combine(Configuration.GetMapsLocation(), map);
            var data = map.Split('.');
            var mapname = data[0];
            if (!Directory.Exists(folder)) return false;
            var scenario = Configuration.GetMapFile(mapname, "_scenario.lua", maps: folder);
            var scmap    = Configuration.GetMapFile(mapname, ".scmap", maps: folder);
            var script   = Configuration.GetMapFile(mapname, "_script.lua", maps: folder);
            var save     = Configuration.GetMapFile(mapname, "_save.lua", maps: folder);
            var files = new string[] { scenario, scmap, script, save };
            foreach (var file in files)
            {
                if (!File.Exists(file)) return false;
            }
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
    }
}
