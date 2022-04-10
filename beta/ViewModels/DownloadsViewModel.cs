using beta.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace beta.ViewModels
{
    internal class DownloadsViewModel : Base.ViewModel
    {
        private readonly IDownloadService DownloadService;
        private readonly object _lock = new();
        public DownloadsViewModel()
        {
            DownloadService = App.Services.GetService<IDownloadService>();

            DownloadService.NewDownload += DownloadService_NewDownload;
            DownloadService.DownloadEnded += DownloadService_DownloadEnded;
            
            BindingOperations.EnableCollectionSynchronization(DownloadService.Downloads, _lock);
        }

        private void DownloadService_DownloadEnded(object sender, DownloadViewModel e) => Latest = DownloadService.Latest;
        private void DownloadService_NewDownload(object sender, DownloadViewModel e) => Latest = DownloadService.Latest;

        public ObservableCollection<DownloadViewModel> Downloads => DownloadService.Downloads;

        #region Latest
        private DownloadViewModel _Latest;
        public DownloadViewModel Latest
        {
            get => _Latest;
            set => Set(ref _Latest, value);
        }
        #endregion
    }
}
