using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.ViewModels;
using beta.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    internal class DownloadService : ViewModel, IDownloadService
    {
        public event EventHandler<DownloadViewModel> NewDownload;
        public event EventHandler<DownloadViewModel> DownloadEnded;

        public ObservableCollection<DownloadViewModel> Downloads { get; } = new();

        #region Latest
        private DownloadViewModel _Latest;


        public DownloadViewModel Latest
        {
            get => _Latest;
            set
            {
                if (Set(ref _Latest, value))
                {
                }
            }
        }
        #endregion

        public async Task Cancel(DownloadViewModel model)
        {

        }
        private void AddDownloadViewModel(DownloadViewModel vm)
        {
            Downloads.Add(vm);
            Latest = vm;
            vm.Completed += OnDownloadCompleted;
            OnNewDownload(vm);
        }
        public DownloadViewModel GetDownload(params DownloadItem[] downloads)
        {
            DownloadViewModel model = new(downloads);
            AddDownloadViewModel(model);
            return model;
        }

        public async Task<DownloadViewModel> DownloadAsync(bool isAwaiting = true, params DownloadItem[] downloads)
        {
            DownloadViewModel model = new(downloads);

            model.Completed += OnDownloadCompleted;

            await model.DownloadAll().ConfigureAwait(isAwaiting);
            AddDownloadViewModel(model);
            return model;
        }

        private void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var model = (DownloadViewModel)sender;
            model.Completed -= OnDownloadCompleted;
            // TODO Dispose?
            Downloads.Remove(model);
            OnDownloadEnded(model);
        }

        private void OnNewDownload(DownloadViewModel e) => NewDownload?.Invoke(this, e);
        private void OnDownloadEnded(DownloadViewModel e) => DownloadEnded?.Invoke(this, e);
    }
}
