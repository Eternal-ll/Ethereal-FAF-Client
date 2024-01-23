using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class FileImageCacheService : IImageCacheService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FileImageCacheService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private string GetPathToEmptyImage() => "C:\\ProgramData\\FAForever\\cache\\empty.png";
        private string GetPathToCacheLocation() => "C:\\ProgramData\\FAForever\\cache\\";

        public async Task<string> EnsureCachedAsync(string url, CancellationToken cancellationToken)
        {
            //url: https://content.faforever.com/maps/previews/small/scmp_009.png

            // / maps/ previews/ small/ scmp_009.png
            var segments = new UriBuilder(url).Uri.Segments;

            // location maps/ previews/ small/ scmp_009.png
            segments[0] = GetPathToCacheLocation();

            var cacheFile = Path.Combine(segments);
            if (!Directory.Exists(Path.GetDirectoryName(cacheFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(cacheFile));
            }
            if (File.Exists(cacheFile))
            {
                return cacheFile;
            }
            using var client = _httpClientFactory.CreateClient();
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using var fs = new FileStream(cacheFile, FileMode.OpenOrCreate);
                    await response.Content.CopyToAsync(fs, cancellationToken);
                }
                catch (IOException ex)
                {

                }
                return cacheFile;
            }
            else
            {
                return GetPathToEmptyImage();
            }
        }
    }
}
