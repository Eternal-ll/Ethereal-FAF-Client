using beta.Models;
using Downloader;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using beta.Infrastructure.Utils;

namespace beta.ViewModels
{
    public class DownloadViewModel : Base.ViewModel
    {
        public event EventHandler<AsyncCompletedEventArgs> Completed;

        public DownloadViewModel(string name, params DownloadItem[] downloads) : this(downloads)
        {
            Name = name;
        }

        public DownloadViewModel(params DownloadItem[] downloads)
        {
            Downloads = downloads;
            Task.Factory.StartNew(async () => _TotalSize = await CalculateTotalSize(downloads).ConfigureAwait(false));
        }

        private static DownloadConfiguration GetDownloadConfiguration()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1";
            //var cookies = new CookieContainer();
            //cookies.Add(new Cookie("download-type", "test") { Domain = "domain.com" });

            return new DownloadConfiguration
            {
                BufferBlockSize = 8000, // usually, hosts support max to 8000 bytes, default values is 8000
                ChunkCount = 8, // file parts to download, default value is 1
                MaximumBytesPerSecond = 0, //1024 * 1024 * 2, // download speed limited to 2MB/s, default values is zero or unlimited
                MaxTryAgainOnFailover = int.MaxValue, // the maximum number of times to fail
                OnTheFlyDownload = false, // caching in-memory or not? default values is true
                ParallelDownload = true, // download parts of file as parallel or not. Default value is false
                TempDirectory = Path.GetTempPath(), // Set the temp path for buffering chunk files, the default path is Path.GetTempPath()
                Timeout = 1000, // timeout (millisecond) per stream block reader, default values is 1000
                RequestConfiguration = {
                    // config and customize request headers
                    Accept = "*/*",
                    //CookieContainer = cookies,
                    Headers = new WebHeaderCollection(), // { Add your custom headers }
                    KeepAlive = true,
                    ProtocolVersion = HttpVersion.Version11, // Default value is HTTP 1.1
                    UseDefaultCredentials = false,
                    UserAgent = $"Ethereal FAF Client {version}",
                    //Proxy = new WebProxy() {
                    //    Address = new Uri("http://YourProxyServer/proxy.pac"),
                    //    UseDefaultCredentials = false,
                    //    Credentials = System.Net.CredentialCache.DefaultNetworkCredentials,
                    //    BypassProxyOnLocal = true
                    //}
                }
            };
        }

        #region Name
        private string _Name = "Unknown";
        public string Name
        {
            get => _Name;
            set => Set(ref _Name, value);
        }
        #endregion

        #region CurrentDownloadModel
        private DownloadModel _CurrentDownloadModel;
        public DownloadModel CurrentDownloadModel
        {
            get => _CurrentDownloadModel;
            set => Set(ref _CurrentDownloadModel, value);
        }
        #endregion

        #region TotalSize
        private long _TotalSize;
        public string TotalSize => _TotalSize.CalcMemoryMensurableUnit();
        #endregion

        #region CurrentFileIndex
        private int _CurrentFileIndex;
        public int CurrentFileIndex
        {
            get => _CurrentFileIndex;
            set => Set(ref _CurrentFileIndex, value);
        }
        #endregion

        #region CurrentFileDownloadProgress
        private double _CurrentFileDownloadProgress;
        public double CurrentFileDownloadProgress
        {
            get => _CurrentFileDownloadProgress;
            set => Set(ref _CurrentFileDownloadProgress, value);
        }
        #endregion

        #region DownloadProgress
        private double _DownloadProgress;
        public double DownloadProgress
        {
            get => DownloadProgress;
            set => Set(ref _DownloadProgress, value);
        }
        #endregion

        public int FilesToDownload => Downloads.Length;

        private DownloadConfiguration CurrentDownloadConfiguration;

        private readonly DownloadItem[] Downloads;

        private static async Task<long> CalculateTotalSize(DownloadItem[] downloads)
        {
            long total = 0;

            for (int i = 0; i < downloads.Length; i++)
            {
                var download = downloads[i];
                WebRequest request = WebRequest.Create(download.Url);
                request.Method = "HEAD";
                var response = await request.GetResponseAsync();
                total += response.ContentLength;
            }

            return total;
        }

        //public async Task Cancel() => CurrentDownloadModel.CancelAsync();

        public async Task DownloadAll() => await Task.Factory.StartNew(async () => await DownloadAll(Downloads).ConfigureAwait(false)).ConfigureAwait(false);

        private async Task DownloadAll(DownloadItem[] downloads)
        {
            for (int i = 0; i < downloads.Length; i++)
            {
                CurrentFileIndex = i + 1;
                // begin download from url
                DownloadService ds = await DownloadFile(downloads[i]).ConfigureAwait(false);
                // clear download to order new of one
                ds.Clear();
            }
        }

        private  async Task<DownloadModel> DownloadFile(DownloadItem downloadItem)
        {
            CurrentDownloadConfiguration = GetDownloadConfiguration();
            CurrentDownloadModel = new (CurrentDownloadConfiguration);
            //CurrentDownloadModel.ChunkDownloadProgressChanged += OnChunkDownloadProgressChanged;
            CurrentDownloadModel.DownloadProgressChanged += OnDownloadProgressChanged;
            CurrentDownloadModel.DownloadFileCompleted += OnDownloadFileCompleted;
            CurrentDownloadModel.DownloadStarted += OnDownloadStarted;

            if (string.IsNullOrWhiteSpace(downloadItem.FileName))
            {
                await CurrentDownloadModel.DownloadFileTaskAsync(downloadItem.Url, new DirectoryInfo(downloadItem.FolderPath)).ConfigureAwait(false);
            }
            //else
            //{
            //    await CurrentDownloadModel.DownloadFileTaskAsync(downloadItem.Url, downloadItem.FileName).ConfigureAwait(false);
            //}

            return CurrentDownloadModel;
        }

        private void OnDownloadStarted(object sender, DownloadStartedEventArgs e)
        {

        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            CurrentFileDownloadProgress = 0;
            total = DownloadProgress;
            if (CurrentFileIndex == Downloads.Length || e.Cancelled)
            {
                Completed?.Invoke(this, e);
            }
        }

        private double total = 0;
        private void OnDownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            CurrentFileDownloadProgress = e.ProgressPercentage;
            DownloadProgress = (total + e.ProgressPercentage) / Downloads.Length;
        }
    }
}
