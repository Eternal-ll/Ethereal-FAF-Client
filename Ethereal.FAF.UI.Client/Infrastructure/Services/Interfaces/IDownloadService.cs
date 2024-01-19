using Ethereal.FAF.UI.Client.Models.Progress;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Services.Interfaces
{
    public interface IDownloadService
    {
        Task DownloadToFileAsync(
            string downloadUrl,
            string downloadPath,
            IProgress<ProgressReport>? progress = null,
            string? httpClientName = null,
            CancellationToken cancellationToken = default
        );

        Task ResumeDownloadToFileAsync(
            string downloadUrl,
            string downloadPath,
            long existingFileSize,
            IProgress<ProgressReport>? progress = null,
            string? httpClientName = null,
            CancellationToken cancellationToken = default
        );

        Task<long> GetFileSizeAsync(
            string downloadUrl,
            string? httpClientName = null,
            CancellationToken cancellationToken = default
        );

        Task<Stream?> GetImageStreamFromUrl(string url);
    }
}
