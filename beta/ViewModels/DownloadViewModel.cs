using beta.Infrastructure.Commands;
using beta.Infrastructure.Utils;
using beta.Models;
using Downloader;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace beta.ViewModels
{
    public class DownloadViewModel : Base.ViewModel
    {
        public event EventHandler<AsyncCompletedEventArgs> Completed;
        public bool IsCompleted { get; private set; }

        public DownloadViewModel(string name, params DownloadItem[] downloads) : this(downloads)
        {
            Name = name;
        }

        public DownloadViewModel(params DownloadItem[] downloads)
        {
            Downloads = downloads;
            //Task.Factory.StartNew(async () => _TotalSize = await CalculateTotalSize(downloads).ConfigureAwait(false));
            //Task.Run(() => DownloadAll());
        }
        private bool IsCanceled = false;
        public void Cancel()
        {
            IsCanceled = true;
            CurrentDownloadModel.CancelAsync();
            CurrentDownloadModel.Clear();
            IsCompleted = false;
        }

        #region CancelCommand
        private ICommand _CancelCommand;
        public ICommand CancelCommand => _CancelCommand ??= new LambdaCommand(OnCancelCommand, CanCancelCommand);
        private void OnCancelCommand(object parameter) => Cancel();
        private bool CanCancelCommand(object parameter) => true;
        #endregion

        private static DownloadConfiguration GetDownloadConfiguration()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1";
            //var cookies = new CookieContainer();
            //cookies.Add(new Cookie("download-type", "test") { Domain = "domain.com" });

            return new DownloadConfiguration
            {
                BufferBlockSize = 8000, // usually, hosts support max to 8000 bytes, default values is 8000
                ChunkCount = 1, // file parts to download, default value is 1
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
        private string _Name;
        public string Name
        {
            get => _Name ?? Downloads[CurrentFileIndex - 1].FileName;
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

        #region DownloadedSize
        private long _DownloadedSize;
        public string DownloadedSize => _DownloadedSize.CalcMemoryMensurableUnit();
        #endregion

        #region CurrentFileIndex
        private int _CurrentFileIndex;
        public int CurrentFileIndex
        {
            get => _CurrentFileIndex;
            set
            {
                if (Set(ref _CurrentFileIndex, value))
                {
                    OnPropertyChanged(nameof(Name));

                    CurrentDownloadItem = Downloads[value - 1];
                    OnPropertyChanged(nameof(CurrentDownloadItem));
                }
            }
        }
        #endregion

        public DownloadItem CurrentDownloadItem { get; private set; }

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
            get => _DownloadProgress;
            set => Set(ref _DownloadProgress, value);
        }
        #endregion

        #region AveragePerSecond
        private double _AveragePerSecond;
        public string AveragePerSecond => _AveragePerSecond.CalcMemoryMensurableUnit();
        #endregion

        #region PerSecond
        private double _PerSecond;
        public string PerSecond => _PerSecond.CalcMemoryMensurableUnit();
        #endregion

        #region EstimateTime
        private string _EstimateTime;
        public string EstimateTime
        {
            get => _EstimateTime;
            set => Set(ref _EstimateTime, value);
        }
        #endregion

        public int FilesToDownload => Downloads.Length;


        #region PauseDownloadCommand
        //private ICommand _PauseDownloadCommand;
        //public ICommand PauseDownloadCommand => _PauseDownloadCommand ??= new LambdaCommand(OnPauseDownloadCommand, CanPauseDownloadCommand);
        //private bool CanPauseDownloadCommand(object parameter) => CurrentDownloadModel.
        #endregion


        private DownloadConfiguration CurrentDownloadConfiguration;

        public DownloadItem[] Downloads { get; private set; }

        private static async Task<long> CalculateTotalSize(DownloadItem[] downloads)
        {
            long total = 0;

            //for (int i = 0; i < downloads.Length; i++)
            //{
            //    var download = downloads[i];
            //    HttpClient client = new();
                
            //    var t = await client.GetAsync(download.Url, HttpCompletionOption.ResponseContentRead);
            //    WebRequest request = WebRequest.Create(download.Url);
            //    request.Method = "GET";
            //    var response = await request.GetResponseAsync();
            //    total += response.ContentLength;
            //}

            return total;
        }

        //public async Task Cancel() => CurrentDownloadModel.CancelAsync();

        public Task DownloadAll() => Task.Run(() => DownloadAll(Downloads));
        public async Task DownloadAllAsync() => await DownloadAll(Downloads);

        private async Task DownloadAll(DownloadItem[] downloads)
        {
            for (int i = 0; i < downloads.Length; i++)
            {
                if (IsCanceled) return;
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

            if (File.Exists(downloadItem.FolderPath + downloadItem.FileName))
                File.Delete(downloadItem.FolderPath + downloadItem.FileName);

            if (!string.IsNullOrWhiteSpace(downloadItem.FileName))
            {
                await CurrentDownloadModel.DownloadFileTaskAsync(downloadItem.Url, downloadItem.FolderPath + downloadItem.FileName).ConfigureAwait(false);
            }
            else
            {
                await CurrentDownloadModel.DownloadFileTaskAsync(downloadItem.Url, new DirectoryInfo(downloadItem.FolderPath)).ConfigureAwait(false);
            }

            return CurrentDownloadModel;
        }

        private void OnDownloadStarted(object sender, DownloadStartedEventArgs e)
        {

        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            CurrentFileDownloadProgress = 0;
            TotalBytesReceived = ((DownloadModel)sender).Package.ReceivedBytesSize;
            total = DownloadProgress;
            if (CurrentFileIndex == Downloads.Length || e.Cancelled)
            {
                Completed?.Invoke(this, e);
                IsCompleted = !e.Cancelled;
            }
        }

        private double total = 0;
        private double TotalBytesReceived = 0;
        private void OnDownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            double nonZeroSpeed = e.BytesPerSecondSpeed + 0.0001;
            int estimateTime = (int)((e.TotalBytesToReceive - e.ReceivedBytesSize) / nonZeroSpeed);
            bool isMinutes = estimateTime >= 60;
            string timeLeftUnit = "seconds";

            if (isMinutes)
            {
                timeLeftUnit = "minutes";
                estimateTime /= 60;
            }

            if (estimateTime < 0)
            {
                estimateTime = 0;
                timeLeftUnit = "unknown";
            }
            EstimateTime = $"{estimateTime} {timeLeftUnit} left";

            CurrentFileDownloadProgress = e.ProgressPercentage;

            if (e.ProgressPercentage == 0)
            {
                DownloadProgress = CurrentFileIndex / Downloads.Length;
            }
            else
            {
                DownloadProgress = (total + e.ProgressPercentage) / Downloads.Length;
            }

            _DownloadedSize = e.ReceivedBytesSize;

            _PerSecond = e.BytesPerSecondSpeed;
            _AveragePerSecond = e.AverageBytesPerSecondSpeed;

            _TotalSize = e.TotalBytesToReceive;

            OnPropertyChanged(nameof(TotalSize));
            OnPropertyChanged(nameof(DownloadedSize));
            OnPropertyChanged(nameof(PerSecond));
            OnPropertyChanged(nameof(AveragePerSecond));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }
    }
}
