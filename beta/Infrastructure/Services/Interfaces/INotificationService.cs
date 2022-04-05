using beta.ViewModels;
using ModernWpf.Controls;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    internal interface INotificationService
    {
        public Task ShowPopupAsync(string text);
        public Task ShowPopupAsync(object model);
        public Task<ContentDialogResult> ShowDialog(string text);
        public Task<ContentDialogResult> ShowDialog(string text, string primary = null, string secondary = null, string close = null);
        public Task<ContentDialogResult> ShowDialog(object model, string primary = null, string secondary = null, string close = null);
        public Task<bool> ShowDownloadDialog(DownloadViewModel model, string close = null);
    }
}
