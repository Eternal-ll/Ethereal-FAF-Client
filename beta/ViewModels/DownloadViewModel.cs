using beta.Infrastructure.Utils;
using beta.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace beta.ViewModels
{
    public class DownloadModel
    {
        public Uri Url { get; }
        private string _FileName;
        public string FileName => _FileName is not null ? _FileName : Url.Segments[^1];
        public string TargetFolder { get; }

        public long? _Size;
        public long Size
        {
            get
            {
                if (_Size is null)
                {
                    _Size = Tools.GetFileSize(Url);
                }
                return _Size.Value;
            }
        }

        public DownloadModel(Uri url, string targetFolder, string fileName = null)
        {
            Url = url;

            if (!targetFolder.EndsWith('\\'))
            {
                throw new Exception($"Path to target folder should end with '\\' {targetFolder}");
            }

            TargetFolder = targetFolder;

            if (fileName is not null)
            {
                _FileName = fileName;
            }
        }

    }
    public class DownloadViewModel : Base.ViewModel
    {
        #region Properties

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
        public int FilesCount => DownloadModels.Length;
        #endregion

        #region Speed
        private long _Speed;
        public string Speed => GetSize(_Speed) + "/sec";
        #endregion

        // on fly
        #region FilesSize
        private long _FilesSize;
        public string FilesSize => GetSize(_FilesSize);
        #endregion

        private DownloadModel[] DownloadModels;
        private DownloadModel CurrentDownloadModel;
        private Download CurrentDownload;

        #endregion
        public DownloadViewModel(params DownloadModel[] downloads)
        {
            DownloadModels = downloads;

            #region Calculating full size
            long fullSize = 0;

            for (int i = 0; i < downloads.Length; i++)
            {
                fullSize += downloads[i].Size;
            }

            _FilesSize = fullSize; 
            #endregion
        }

        #region Methods

        public void Cancel()
        {
            CurrentDownload.Cancel();
        }

        public void DoDownload()
        {
            new Thread(async () => await Download()).Start();
        }

        private DateTime lastUpdate;
        private long lastBytes = 0;
        private int globalProgressValue = 0;
        private async Task Download()
        {
            for (int i = 0; i < DownloadModels.Length; i++)
            {
                var dm = DownloadModels[i];
                Download d = new(dm.Url.AbsoluteUri);
                d.BytesReceivedPerSec += (s, e) =>
                {
                    var bytesChange = e - lastBytes;
                    _Speed = bytesChange;
                    lastBytes = e;

                    var percentage = (int)(dm.Size / 100 * e);
                    GlobalProgressValue = globalProgressValue + (percentage/ FilesCount);
                    FileProgressValue = percentage;

                    _CurrentFileDownloadedSize += e;

                    if (percentage == 100)
                    {
                        _DownloadedSize += e;
                        _CurrentFileDownloadedSize = 0;

                        CurrentFileIndex++;
                        lastBytes = 0;
                        globalProgressValue += percentage / FilesCount;
                        OnPropertyChanged(nameof(DownloadedSize));
                    }


                    OnPropertyChanged(nameof(Speed));
                    OnPropertyChanged(nameof(CurrentFileDownloadedSize));
                };
                await d.Start(dm.TargetFolder);
            }
        }

        private static string GetSize(long bytes)
        {
            var kb = Convert.ToInt32(bytes) / 1024;
            var mb = Math.Round(kb * .001, 1);
            return kb > 1000 ? mb + " MB" : kb > 1 ? kb + " KB" : bytes + " B";
        }

        #endregion
    }
}
