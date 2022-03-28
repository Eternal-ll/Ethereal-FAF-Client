using beta.Infrastructure;
using beta.Infrastructure.Commands;
using beta.Infrastructure.Utils;
using beta.ViewModels.Base;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;

namespace beta.ViewModels
{
    public class TestDownloaderVM : ViewModel
    {
        public event EventHandler<EventArgs<bool>> DownloadFinished;

        #region GlobalProgressValue
        private double _GlobalProgressValue;
        public double GlobalProgressValue
        {
            get => _GlobalProgressValue;
            set => Set(ref _GlobalProgressValue, value);
        }
        #endregion

        #region FileProgressValue
        private int _FileProgressValue;
        public int FileProgressValue
        {
            get => _FileProgressValue;
            set => Set(ref _FileProgressValue, value);
        }
        #endregion

        #region CurrentState
        private string _CurrentState;
        public string CurrentState
        {
            get => _CurrentState;
            set => Set(ref _CurrentState, value);
        }
        #endregion

        #region CurrentFileIndex
        private int _CurrentFileIndex;
        public int CurrentFileIndex
        {
            get => _CurrentFileIndex;
            set => Set(ref _CurrentFileIndex, value);
        }
        #endregion

        #region CurrentPathToFile
        private string _CurrentPathToFile;
        public string CurrentPathToFile
        {
            get => _CurrentPathToFile;
            set => Set(ref _CurrentPathToFile, value);
        }
        #endregion

        #region CurrentFileSize
        private string _CurrentFileSize;
        public string CurrentFileSize
        {
            get => _CurrentFileSize;
            set => Set(ref _CurrentFileSize, value);
        }
        #endregion

        #region CurrentFileDownloadedSize
        private long _CurrentFileDownloadedSize;
        public string CurrentFileDownloadedSize => _CurrentFileDownloadedSize.ToFileSize();
        #endregion

        #region DownloadedSize
        private long _DownloadedSize;
        public string DownloadedSize => _DownloadedSize.ToFileSize();
        #endregion

        #region FilesCount
        //private int _FilesCount;
        public int FilesCount => Models.Length;
        #endregion

        #region Speed
        private long _Speed;
        public string Speed => _Speed.ToFileSize() + "/sec";//GetSize(_Speed) + "/sec";
        #endregion

        #region FilesSize
        private string _FilesSize;
        public string FilesSize
        {
            get => _FilesSize;
            set => Set(ref _FilesSize, value);
        }
        #endregion

        #region CancelCommand
        public ICommand _CancelCommand;
        public ICommand CancelCommand => _CancelCommand;
        private void OnCancelCommand(object parameter)
        {
            IsCanceled = true;

            CurrentState = $"Cancelled. Wait until the current file is loaded";
            webClient.CancelAsync();
        }
        #endregion

        private DateTime lastUpdate;
        private long lastBytes = 0;
        private double globalProgressValue = 0;
        private WebClient webClient;
        private bool IsCanceled = false;

        private readonly DownloadModel[] Models;

        public TestDownloaderVM(params DownloadModel[] models)
        {
            Models = models;

            webClient = new()
            {
                Proxy = null,
                CachePolicy = new(System.Net.Cache.RequestCacheLevel.NoCacheNoStore)
            };
            webClient.DownloadProgressChanged += DownloadProgressChanged;

            _CancelCommand = new LambdaCommand(OnCancelCommand);
        }

        public long GetFileSize(string url)
        {
            long result = -1;
            var req = WebRequest.Create(url);
            req.Method = "HEAD";
            using (WebResponse resp = req.GetResponse())
            {
                if (long.TryParse(resp.Headers.Get("Content-Length"), out long ContentLength))
                {
                    result = ContentLength;
                }
            }
            return result;
        }

        private string GetSize(long bytes)
        {
            var kb = Convert.ToInt64(bytes) / 1024;
            var mb = Math.Round(kb * .001, 1);
            return kb > 1000 ? mb + " MB" : kb > 1 ? kb + " KB" : bytes + " B";
        }

        public void DoDownload() => Task.Run(Download);
        public async Task Download()
        {
            var models = Models;

            long fullSize = 0;

            for (int i = 0; i < models.Length; i++)
            {
                fullSize += models[i].Size;
            }

            FilesSize = fullSize.ToFileSize();

            for (int i = 0; i < models.Length; i++)
            {
                if (IsCanceled) break;
                var item = models[i];

                var pathLocal = item.TargetFolder + item.FileName;

                CurrentState = $"Downloading: \"{item.FileName}\"";
                CurrentPathToFile = pathLocal;

                CurrentFileSize = item.Size.ToFileSize();

                CurrentFileIndex++;

                await webClient.DownloadFileTaskAsync(item.Url.AbsoluteUri, pathLocal);
            }

            if (!IsCanceled) GlobalProgressValue = 100;

            webClient.DownloadProgressChanged -= DownloadProgressChanged;
            webClient.Dispose();
            webClient = null;
            _CancelCommand = null;
            DownloadFinished?.Invoke(this, !IsCanceled);

        }
        long backup = 0;
        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            GlobalProgressValue = globalProgressValue + (e.ProgressPercentage / FilesCount);

            //var now = DateTime.UtcNow;
            //var timeSpan = now - lastUpdate;
            var bytesChange = e.BytesReceived - lastBytes;
            _Speed = bytesChange;
            //var now = DateTime.UtcNow;
            //var timeSpan = now - lastUpdate;

            //if (timeSpan.Seconds < 1)
            //{
            //    lastBytes = e.BytesReceived;
            //    lastUpdate = now;
            //    return;
            //}

            //if (timeSpan.Seconds == 1)
            //{
            //    var bytesChange = e.BytesReceived - lastBytes + backup;
            //    _Speed = bytesChange / timeSpan.Seconds;
            //    lastBytes = e.BytesReceived + backup;
            //    lastUpdate = now;
            //    backup = 0;
            //}
            //else
            //{
            //    lastBytes = e.BytesReceived;
            //    lastUpdate = now;
            //}
            //FileProgressValue = e.ProgressPercentage;

            //_CurrentFileDownloadedSize += e.BytesReceived;
            //_DownloadedSize = _CurrentFileDownloadedSize;

            if (e.ProgressPercentage == 100)
            {
                //_CurrentFileDownloadedSize = 0;

                lastBytes = 0;
                backup = 0;
                globalProgressValue += FilesCount * .001;
            }
            lastBytes = e.BytesReceived;
            //lastUpdate = now;

            OnPropertyChanged(nameof(DownloadedSize));
            OnPropertyChanged(nameof(Speed));
            OnPropertyChanged(nameof(CurrentFileDownloadedSize));
        }
    }
}
