using beta.Infrastructure;
using beta.Infrastructure.Commands;
using beta.Models.API;
using beta.ViewModels.Base;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;

namespace beta.ViewModels
{
    public class TestDownloaderModel : ViewModel
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
        private int _FilesCount;
        public int FilesCount => _FilesCount;
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
        }
        #endregion


        private DateTime lastUpdate;
        private long lastBytes = 0;
        private int globalProgressValue = 0;
        private WebClient webClient;
        private bool IsCanceled = false;

        private Record Record;

        public TestDownloaderModel(Record record)
        {
            _FilesCount = record.FeaturedModFiles.Count;
            Record = record;

            webClient = new()
            {
                Proxy = null,
                CachePolicy = new(System.Net.Cache.RequestCacheLevel.NoCacheNoStore)
            };
            webClient.DownloadProgressChanged += DownloadProgressChanged;
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            webClient.OpenWriteCompleted += WebClient_OpenWriteCompleted;
            webClient.OpenReadCompleted += WebClient_OpenReadCompleted;


            _CancelCommand = new LambdaCommand(OnCancelCommand);
        }

        private void WebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void WebClient_OpenWriteCompleted(object sender, OpenWriteCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            throw new NotImplementedException();
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
            var kb = Convert.ToInt32(bytes) / 1024;
            var mb = Math.Round(kb * .001, 1);
            return kb > 1000 ? mb + " MB" : kb > 1 ? kb + " KB" : bytes + " B";
        }

        public async Task Download()
        {
            var record = Record;

            var pathLocal = App.GetPathToFolder(Models.Folder.ProgramData);

            long fullSize = 0;

            for (int i = 0; i < record.FeaturedModFiles.Count; i++)
            {
                var sizeBytes = GetFileSize(record.FeaturedModFiles[i].attributes["url"].ToString());
                fullSize += sizeBytes;
                record.FeaturedModFiles[i].FileSize = GetSize(sizeBytes);
            }

            FilesSize = GetSize(fullSize);

            for (int i = 0; i < record.FeaturedModFiles.Count; i++)
            {
                if (IsCanceled) break;

                var item = record.FeaturedModFiles[i];
                if (item is null) continue;

                var path = pathLocal
                + item.attributes["group"].ToString() + "\\"
                + item.attributes["name"].ToString();

                CurrentState = $"Downloading: \"{item.attributes["name"]}\"";
                CurrentPathToFile = path;

                CurrentFileSize = item.FileSize;

                await webClient.DownloadFileTaskAsync(item.attributes["url"].ToString(), path);
            }
                
            GlobalProgressValue = 100;

            webClient.DownloadProgressChanged -= DownloadProgressChanged;
            webClient.Dispose();
            webClient = null;
            _CancelCommand = null;
            DownloadFinished?.Invoke(this, !IsCanceled);

        }
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

            GlobalProgressValue = globalProgressValue + (e.ProgressPercentage / _FilesCount);
            FileProgressValue = e.ProgressPercentage;

            _CurrentFileDownloadedSize += e.BytesReceived;

            if (e.ProgressPercentage == 100)
            {
                _DownloadedSize += e.TotalBytesToReceive;
                _CurrentFileDownloadedSize = 0;

                CurrentFileIndex++;
                lastBytes = 0;
                lastUpdate = now;
                globalProgressValue += e.ProgressPercentage / _FilesCount;
                OnPropertyChanged(nameof(DownloadedSize));
            }


            OnPropertyChanged(nameof(Speed));
            OnPropertyChanged(nameof(CurrentFileDownloadedSize));
        }
    }
}
