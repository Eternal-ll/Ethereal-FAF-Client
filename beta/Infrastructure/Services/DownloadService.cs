using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public class DownloadService : ViewModel, IDownloadService
    {
        public string DOWNLOAD_FOLDER => "C:\\tmp";

        public readonly ObservableCollection<Download> Downloads = new();

        private Download _Last;
        public Download Last
        {
            get => _Last;
            set => Set(ref _Last, value);
        }

        public async Task<Download> StartDownload(Uri uri)
        {
            var downloadModel = new Download(uri.AbsoluteUri);

            Downloads.Add(downloadModel);
            Last = downloadModel;

            await downloadModel.Start(Path.Combine(DOWNLOAD_FOLDER, uri.Segments[^1]));

            return downloadModel;
        }
        public Task<Download> StartDownload(string url) => StartDownload(new Uri(url));

        public void CancelDownload(Download download)
        {
            download.Cancel();
            Downloads.Remove(download);
        }

    }
}
