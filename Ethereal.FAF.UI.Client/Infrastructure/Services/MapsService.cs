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
        private readonly ILogger Logger;
        private readonly IHttpClientFactory HttpClientFactory;
        public readonly string MapsFolder;
        private readonly string BaseAddress;

        public MapsService(string mapsFolder, string baseAddress, IHttpClientFactory httpClientFactory, ILogger logger)
        {
            if (!(mapsFolder[^1] == '/' || mapsFolder[^1] == '\\'))
            {
                MapsFolder += '/';
            }
            mapsFolder = Environment.ExpandEnvironmentVariables(mapsFolder);
            MapsFolder = mapsFolder;
            BaseAddress = baseAddress;
            HttpClientFactory = httpClientFactory;
            Logger = logger;
        }

        public bool IsExist(string map) => Directory.Exists(MapsFolder + map);
        public async Task<bool> DownloadAsync(string map, string mapFilePath, IProgress<string> progress = null, CancellationToken cancellationToken = default)
        {
            var client = HttpClientFactory.CreateClient();
            using var fs = new FileStream(map+ ".zip", FileMode.Create);
            var response = await client.GetAsync(BaseAddress + mapFilePath, cancellationToken);
            progress?.Report($"Downloading map [{map + ".zip"}]");
            await response.Content.CopyToAsync(fs, cancellationToken);
            fs.Close();
            progress?.Report($"Extracting map [{map + ".zip"}]");
            ZipFile.ExtractToDirectory(map + ".zip", MapsFolder, true);
            File.Delete(map + ".zip");
            return true;
        }
    }
}
