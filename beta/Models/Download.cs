using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace beta.Models
{
    public class Download
    {
        public event EventHandler<long> BytesReceivedPerSec;
        public event EventHandler<int> DownloadProgressChanged;
        public event EventHandler<long> SizeReceived;

        private static readonly HttpClient Client = new();
        private readonly string _fileUrl;

        public long? Size;

        public Download(string fileUrl) => _fileUrl = fileUrl;

        bool IsCanceled;

        long Received;
        int last = 0;
        public async Task Start(string saveTo)
        {
            var response = await Client.GetAsync(_fileUrl);

            long? size = response.Content.Headers.ContentLength.Value;

            var _responseStream = await response.Content.ReadAsStreamAsync();
            var _fileStream = File.Create(saveTo);

            byte[] buffer = new byte[32768];
            int read;
            while ((read = _responseStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (IsCanceled)
                {
                    _responseStream.Dispose();
                    _fileStream.Dispose();
                    return;
                }
                Received += read;
                _fileStream.Write(buffer, 0, read);

                if (size.HasValue)
                {
                    int downloaded = (int)(Received / size.Value * 100);

                    if (last != downloaded)
                    {
                        last = downloaded;
                        DownloadProgressChanged?.Invoke(this, downloaded);
                    }
                }
            }
            _responseStream.Dispose();
            _fileStream.Dispose();
        }

        public void Cancel() => IsCanceled = true;
    }
}
