using beta.Infrastructure.Services;
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
            BindingOperations.EnableCollectionSynchronization(DownloadService.Downloads, _lock);
        }

        public ObservableCollection<DownloadViewModel> Downloads => DownloadService.Downloads;
    }
}
