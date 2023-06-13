using Downloader;
using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class DownloadViewModel : Base.ViewModel
    {
        public DownloadViewModel(HttpClient httpClient, string url, string file)
        {
            Url = url;
            DestinationFile = file;
            HttpClient = httpClient;
            CancellationTokenSource = new();

            Download = DownloadBuilder.New()
                .WithUrl(Url)
                .WithFileLocation(file)
                .Build();

            CancelCommand = new LambdaCommand((object arg) =>
            {
                Download.Stop();
                OnPropertyChanged(nameof(Status));
            });
            PauseCommand = new LambdaCommand((object arg) =>
            {
                Download.Pause();
                OnPropertyChanged(nameof(Status));
            });
            ResumeCommand = new LambdaCommand((object arg) =>
            {
                Download.Resume();
                OnPropertyChanged(nameof(Status));
            });

            Download.DownloadStarted += Download_DownloadStarted;
            Download.DownloadFileCompleted += Download_DownloadFileCompleted;
            Download.ChunkDownloadProgressChanged += Download_ChunkDownloadProgressChanged;
            Download.DownloadProgressChanged += Download_DownloadProgressChanged;
        }

        public event EventHandler Downloaded;
        public event EventHandler<Exception> Faulted;

        private readonly IDownload Download;

        public string Url { get; set; }

        public string DestinationFile { get; set; }

        public DownloadStatus Status => Download.Status;

        #region Downloaded
        private double _Progress;
        public double Progress { get => _Progress; set => Set(ref _Progress, value); }
        #endregion

        public CancellationTokenSource CancellationTokenSource { get; set; }
        public HttpClient HttpClient { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand PauseCommand { get; set; }
        public ICommand ResumeCommand { get; set; }



        private void Download_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
            Console.WriteLine(e.ProgressPercentage);
        }

        private void Download_ChunkDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {

        }

        private void Download_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            OnPropertyChanged(nameof(Status));
        }

        private void Download_DownloadStarted(object sender, DownloadStartedEventArgs e)
        {
            OnPropertyChanged(nameof(Status));
        }


        public async Task Start()
        {
            await Download.StartAsync();
            //var directory = Path.GetDirectoryName(DestinationFile);
            //if (Directory.Exists(directory)) Directory.CreateDirectory(directory);
            //using var fs = new FileStream(DestinationFile, FileMode.OpenOrCreate);
            //var progress = new Progress<float>((x) =>
            //{
            //    Progress = x;
            //});
            //try
            //{
            //    await HttpClient.DownloadDataAsync(Url, fs, progress, CancellationTokenSource.Token);
            //    Downloaded?.Invoke(this, EventArgs.Empty);
            //}
            //catch(Exception ex)
            //{
            //    fs.Close();
            //    fs.Dispose();
            //    Faulted?.Invoke(this, ex);
            //}
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                HttpClient?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    public class DownloadsViewModel : Base.ViewModel
    {
        private readonly IHttpClientFactory HttpClientFactory;
        public DownloadsViewModel(IHttpClientFactory httpClientFactory)
        {
            DownloadCommand = new LambdaCommand((object arg) =>
            {
                //arg = "https://content.faforever.com/legacy-featured-mod-files/updates_faf_files/ForgedAlliance.3757.exe";
                if (arg is not string url) return;
                if (!System.Uri.IsWellFormedUriString(url, System.UriKind.Absolute)) return;
                var dest = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(url));
                //var client = HttpClientFactory.CreateClient();
                //client.DefaultRequestHeaders.Add("Authorization", "Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6InB1YmxpYzo5N2U2ZmQxMy0zNDcxLTQ4ZDgtYTA3OC1jYzVhMWIzNTZiMzYiLCJ0eXAiOiJKV1QifQ.eyJhdWQiOltdLCJjbGllbnRfaWQiOiJldGhlcmVhbC1mYWYtY2xpZW50IiwiZXhwIjoxNjg2MTYzNDQ3LCJleHQiOnsicm9sZXMiOlsiVVNFUiJdLCJ1c2VybmFtZSI6IkV0ZXJuYWwtIn0sImlhdCI6MTY4NjE1OTg0NywiaXNzIjoiaHR0cHM6Ly9oeWRyYS5mYWZvcmV2ZXIuY29tLyIsImp0aSI6ImM1YTU5Y2I0LTllZGUtNDZiMS1hZDRmLTRjZjJmYjM5YTQ0MiIsIm5iZiI6MTY4NjE1OTg0Nywic2NwIjpbIm9wZW5pZCIsIm9mZmxpbmUiLCJwdWJsaWNfcHJvZmlsZSIsImxvYmJ5IiwidXBsb2FkX21hcCIsInVwbG9hZF9tb2QiXSwic3ViIjoiMzAyMTc2In0.mNyT5RbRvTETTbrg_ssDG99EzUcvAn6W2rlV2LOes7E-b88CfrprRYq9t2u-yCFHecOIFbMm5dXLSJIt4l_om4vnBQcWeuecSHI0x84ABksf1KvXQJP2izG9OskVVF236Nxg4neZlByC_7ayzV1j6PIfwKyxEFECQSugGnGF_iyRmeO1RczLLmzn2E1Q-qvDgBbCpuisOpsDdyM1hsPGN7hb8el8V7lRbIulKgJ3FinHG06SXIu71o5XTwc4Vm3xRlLFRAo7r3gi8M1tM3YxpIQWM9Md-XAfCZfWJAOEDRRold8znI-dYG8WIQGW91ks3ubbEdSnt_-SUxr9ls4qRT7pfhdVzUfXrycdcM708tAvsf30j88kMr8254xLNe3A8IuFT-plbOtVbzfYNxDoIv3YYk0azmdIGuySylcUxiYNL5eKF2iHnCkxksfQhp-1HPH0niX3ZD8ryCp-krQYOEOEBOL6Ts7tCccLmQQ-WbOzNvkCp1YRtLrORqvaNm_YkS9IpGRIQQuOl9adfas0HfsiI2sKBFPLmq-qSLawc9B4clIuAdPed4xInbCz4YqnfIuD5HZgXZ21FW8LMJKQvd4WaRNxnJ0EtAvswY6QOXIln4zy04H8wRhacK7dTjoD9ex2SB-s0JsZnRctqGypY0xUNxPn-1btiE70-Cc9yAM");
                //client.DefaultRequestHeaders.Add("Vefiry", "1686162440-Ky4XsNgKgolC8hr5kpe4Y8V7R2%2Bf1RaDViwPOBavJwo%3D");
                var model = new DownloadViewModel (null, url, dest);
                model.Downloaded += Model_Downloaded;
                model.Faulted += Model_Faulted;
                Downloads.Add(model);
                Task.Run(model.Start).ContinueWith(t => Model_Faulted(model, t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            });
            Downloads = new();
            HttpClientFactory = httpClientFactory;
        }

        private void Model_Faulted(object sender, Exception e)
        {
            App.Current.Dispatcher.Invoke(() => Downloads.Remove((DownloadViewModel)sender), DispatcherPriority.Background);
        }

        private void Model_Downloaded(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => Downloads.Remove((DownloadViewModel)sender), DispatcherPriority.Background);
        }

        public ObservableCollection<DownloadViewModel> Downloads { get; }
        public ICommand DownloadCommand { get; set; }
    }
}
