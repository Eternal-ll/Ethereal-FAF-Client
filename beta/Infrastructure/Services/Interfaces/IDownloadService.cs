using beta.Models;
using System;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface IDownloadService
    {
        public Task<Download> StartDownload(Uri fileUrl);
        public Task<Download> StartDownload(string url);
        public void CancelDownload(Download download);
        public Download Last { get; }
    }
}
