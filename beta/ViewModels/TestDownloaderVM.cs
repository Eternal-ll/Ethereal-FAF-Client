using beta.Infrastructure;
using beta.Infrastructure.Commands;
using beta.Models;
using beta.Models.API;
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
        private int _GlobalProgressValue;
        public int GlobalProgressValue
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
        public string CurrentFileDownloadedSize => GetSize(_CurrentFileDownloadedSize);
        #endregion

        #region DownloadedSize
        private long _DownloadedSize;
        public string DownloadedSize => GetSize(_DownloadedSize);
        #endregion

        #region FilesCount
        //private int _FilesCount;
        public int FilesCount => Models.Length;
        #endregion

        #region Speed
        private long _Speed;
        public string Speed => GetSize(_Speed) + "/sec";
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
            task.TrySetCanceled();
            task.Task.Dispose();
        }
        #endregion

        private DateTime lastUpdate;
        private long lastBytes = 0;
        private int globalProgressValue = 0;
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

            FilesSize = GetSize(fullSize);

            for (int i = 0; i < models.Length; i++)
            {
                if (IsCanceled) break;
                var item = models[i];

                var pathLocal = item.TargetFolder + item.FileName;

                CurrentState = $"Downloading: \"{item.FileName}\"";
                CurrentPathToFile = pathLocal;

                CurrentFileSize = GetSize(item.Size);

                await webClient.DownloadFileTaskAsync(item.Url.AbsoluteUri, pathLocal);
            }

            if (!IsCanceled) GlobalProgressValue = 100;

            webClient.DownloadProgressChanged -= DownloadProgressChanged;
            webClient.Dispose();
            webClient = null;
            _CancelCommand = null;
            DownloadFinished?.Invoke(this, !IsCanceled);

        }
        TaskCompletionSource<object> task;
        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var now = DateTime.UtcNow;
            var timeSpan = now - lastUpdate;
            if (timeSpan.Seconds == 1)
            {
                var bytesChange = e.BytesReceived - lastBytes;
                _Speed = bytesChange / timeSpan.Seconds;
                lastBytes = e.BytesReceived;
                lastUpdate = now;
            }
            else if (timeSpan.Seconds > 1)
            {
                lastBytes = e.BytesReceived;
                lastUpdate = now;
            }

            GlobalProgressValue = globalProgressValue + (e.ProgressPercentage / FilesCount);
            FileProgressValue = e.ProgressPercentage;

            _CurrentFileDownloadedSize += e.BytesReceived;
            _DownloadedSize = _CurrentFileDownloadedSize;

            if (e.ProgressPercentage == 100)
            {
                _CurrentFileDownloadedSize = 0;

                CurrentFileIndex++;
                lastBytes = 0;
                lastUpdate = now;
                globalProgressValue += e.ProgressPercentage / FilesCount;
            }


            OnPropertyChanged(nameof(DownloadedSize));
            OnPropertyChanged(nameof(Speed));
            OnPropertyChanged(nameof(CurrentFileDownloadedSize));
        }
    }
}
