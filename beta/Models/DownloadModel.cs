using Downloader;

namespace beta.Models
{
    /// <summary>
    /// Wrap class for Downloader.DownloadService. See https://github.com/bezzad/Downloader
    /// </summary>
    public class DownloadModel : DownloadService
    {
        public DownloadModel(DownloadConfiguration options) : base(options) { }
    }
}
