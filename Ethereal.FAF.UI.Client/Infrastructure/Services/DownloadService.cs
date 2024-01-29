using Ethereal.FAF.UI.Client.Infrastructure.Helper;
using Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces;
using Ethereal.FAF.UI.Client.Models.Progress;
using Microsoft.Extensions.Logging;
using Polly.Contrib.WaitAndRetry;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services
{
    internal class DownloadService : IDownloadService
    {
        private readonly ILogger<DownloadService> logger;
        private readonly IHttpClientFactory httpClientFactory;
        private const int BufferSize = ushort.MaxValue;

        public DownloadService(
            ILogger<DownloadService> logger,
            IHttpClientFactory httpClientFactory
        )
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task DownloadToFileAsync(
            string downloadUrl,
            string downloadPath,
            IProgress<ProgressReport>? progress = null,
            string? httpClientName = null,
            CancellationToken cancellationToken = default
        )
        {
            using var client = string.IsNullOrWhiteSpace(httpClientName)
                ? httpClientFactory.CreateClient()
                : httpClientFactory.CreateClient(httpClientName);

            client.Timeout = TimeSpan.FromMinutes(10);
            AddUserAgentHeaders(client);

            await using var file = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None);

            long contentLength = 0;

            var response = await client
                .GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            contentLength = response.Content.Headers.ContentLength ?? 0;
            var isIndeterminate = contentLength == 0;
            
            if (contentLength > 0)
            {
                // check free space
                if (SystemInfo.GetDiskFreeSpaceBytes(Path.GetDirectoryName(downloadPath)) is { } freeSpace
                    && freeSpace < contentLength)
                {
                    throw new ApplicationException(
                        $"Not enough free space to download file. Free: {freeSpace} bytes, Required: {contentLength} bytes"
                    );
                }
            }

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var totalBytesRead = 0L;
            var buffer = new byte[BufferSize];
            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0)
                    break;
                await file.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);

                totalBytesRead += bytesRead;

                if (isIndeterminate)
                {
                    progress?.Report(new ProgressReport(-1, isIndeterminate: true));
                }
                else
                {
                    progress?.Report(
                        new ProgressReport(
                            current: Convert.ToUInt64(totalBytesRead),
                            total: Convert.ToUInt64(contentLength),
                            message: "Downloading..."
                        )
                    );
                }
            }

            await file.FlushAsync(cancellationToken).ConfigureAwait(false);

            progress?.Report(new ProgressReport(1f, message: "Download complete!"));
        }

        /// <inheritdoc />
        public async Task ResumeDownloadToFileAsync(
            string downloadUrl,
            string downloadPath,
            long existingFileSize,
            IProgress<ProgressReport>? progress = null,
            string? httpClientName = null,
            CancellationToken cancellationToken = default
        )
        {
            using var client = string.IsNullOrWhiteSpace(httpClientName)
                ? httpClientFactory.CreateClient()
                : httpClientFactory.CreateClient(httpClientName);

            client.Timeout = TimeSpan.FromMinutes(10);
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("StabilityMatrix", "2.0"));

            // Create file if it doesn't exist
            if (!File.Exists(downloadPath))
            {
                logger.LogInformation("Resume file doesn't exist, creating file {DownloadPath}", downloadPath);
                File.Create(downloadPath).Close();
            }

            await using var file = new FileStream(downloadPath, FileMode.Append, FileAccess.Write, FileShare.None);

            // Remaining content length
            long remainingContentLength = 0;
            // Total of the original content
            long originalContentLength = 0;

            using var noRedirectRequest = new HttpRequestMessage();
            noRedirectRequest.Method = HttpMethod.Get;
            noRedirectRequest.RequestUri = new Uri(downloadUrl);
            noRedirectRequest.Headers.Range = new RangeHeaderValue(existingFileSize, null);

            HttpResponseMessage? response = null;
            foreach (var delay in Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(50), retryCount: 4))
            {
                using var redirectRequest = new HttpRequestMessage();
                redirectRequest.Method = HttpMethod.Get;
                redirectRequest.RequestUri = new Uri(downloadUrl);
                redirectRequest.Headers.Range = new RangeHeaderValue(existingFileSize, null);

                response = await client
                    .SendAsync(redirectRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);

                remainingContentLength = response.Content.Headers.ContentLength ?? 0;
                originalContentLength = response.Content.Headers.ContentRange?.Length.GetValueOrDefault() ?? 0;

                if (remainingContentLength > 0)
                    break;

                logger.LogDebug("Retrying get-headers for content-length");
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }

            if (response == null)
            {
                throw new ApplicationException("Response is null");
            }

            var isIndeterminate = remainingContentLength == 0;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var totalBytesRead = 0L;
            var buffer = new byte[BufferSize];
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bytesRead = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0)
                    break;
                await file.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);

                totalBytesRead += bytesRead;

                if (isIndeterminate)
                {
                    progress?.Report(new ProgressReport(-1, isIndeterminate: true));
                }
                else
                {
                    progress?.Report(
                        new ProgressReport(
                            // Report the current as session current + original start size
                            current: Convert.ToUInt64(totalBytesRead + existingFileSize),
                            // Total as the original total
                            total: Convert.ToUInt64(originalContentLength),
                            message: "Downloading..."
                        )
                    );
                }
            }

            await file.FlushAsync(cancellationToken).ConfigureAwait(false);

            progress?.Report(new ProgressReport(1f, message: "Download complete!"));
        }

        /// <inheritdoc />
        public async Task<long> GetFileSizeAsync(
            string downloadUrl,
            string? httpClientName = null,
            CancellationToken cancellationToken = default
        )
        {
            using var client = string.IsNullOrWhiteSpace(httpClientName)
                ? httpClientFactory.CreateClient()
                : httpClientFactory.CreateClient(httpClientName);

            client.Timeout = TimeSpan.FromMinutes(10);
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("StabilityMatrix", "2.0"));

            var contentLength = 0L;

            foreach (var delay in Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(50), retryCount: 3))
            {
                var response = await client
                    .GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false);
                contentLength = response.Content.Headers.ContentLength ?? -1;

                if (contentLength > 0)
                    break;

                logger.LogDebug("Retrying get-headers for content-length");
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }

            return contentLength;
        }

        public async Task<Stream?> GetImageStreamFromUrl(string url)
        {
            using var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            AddUserAgentHeaders(client);
            try
            {
                var response = await client.GetAsync(url).ConfigureAwait(false);
                return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get image stream from url {Url}", url);
                return null;
            }
        }
        private void AddUserAgentHeaders(HttpClient client)
        {
            var version = VersionHelper.GetCurrentVersionInText();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ethereal-faf-client", version));
        }
    }
}
