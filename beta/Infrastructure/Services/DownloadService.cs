using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.ViewModels;
using beta.ViewModels.Base;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public class DownloadService : ViewModel, IDownloadService
    {
        public ObservableCollection<DownloadViewModel> Downloads { get; } = new();

        public async Task Cancel(DownloadViewModel model)
        {

        }

        public async Task<DownloadViewModel> DownloadAsync(params DownloadItem[] downloads)
        {
            DownloadViewModel model = new(downloads);

            model.Completed += OnDownloadCompleted;

            await model.DownloadAll().ConfigureAwait(false);
            Downloads.Add(model);
            return model;
        }

        private void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var model = (DownloadViewModel)sender;
            model.Completed -= OnDownloadCompleted;
            // TODO Dispose?
            Downloads.Remove(model);
        }
    }
}
