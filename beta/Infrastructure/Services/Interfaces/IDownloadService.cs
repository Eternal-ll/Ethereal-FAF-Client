using beta.Models;
using beta.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IDownloadService : INotifyPropertyChanged
    {
        public event EventHandler<DownloadViewModel> NewDownload;
        public event EventHandler<DownloadViewModel> DownloadEnded;
        public ObservableCollection<DownloadViewModel> Downloads { get; }
        public DownloadViewModel Latest { get; }
        public Task<DownloadViewModel> DownloadAsync(bool isAwaiting = true, params DownloadItem[] downloads);
        public Task Cancel(DownloadViewModel model);
        public DownloadViewModel GetDownload(params DownloadItem[] downloads);
    }
}
