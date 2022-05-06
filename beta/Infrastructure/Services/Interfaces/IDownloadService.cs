using beta.Models;
using beta.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Download service for downloading stuff in background
    /// </summary>
    public interface IDownloadService : INotifyPropertyChanged
    {
        /// <summary>
        /// New download started
        /// </summary>
        public event EventHandler<DownloadViewModel> NewDownload;
        /// <summary>
        /// Download completed
        /// </summary>
        public event EventHandler<DownloadViewModel> DownloadEnded;
        /// <summary>
        /// All download models
        /// </summary>
        public ObservableCollection<DownloadViewModel> Downloads { get; }
        /// <summary>
        /// Latest download model
        /// </summary>
        public DownloadViewModel Latest { get; }
        /// <summary>
        /// Start downloading and returns download model
        /// </summary>
        /// <param name="isAwaiting"></param>
        /// <param name="downloads"></param>
        /// <returns></returns>
        public Task<DownloadViewModel> DownloadAsync(bool isAwaiting = true, params DownloadItem[] downloads);
        /// <summary>
        /// Cancel download
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Task Cancel(DownloadViewModel model);
        /// <summary>
        /// Gets download model
        /// </summary>
        /// <param name="downloads"></param>
        /// <returns></returns>
        public DownloadViewModel GetDownload(params DownloadItem[] downloads);
    }
}
