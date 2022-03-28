using beta.Models;
using beta.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IDownloadService
    {
        //public event EventHandler NewDownload;
        public ObservableCollection<DownloadViewModel> Downloads { get; }
        public Task<DownloadViewModel> DownloadAsync(params DownloadItem[] downloads);
        public Task Cancel(DownloadViewModel model);
    }
}
