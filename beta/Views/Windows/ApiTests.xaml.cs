using beta.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Windows;

namespace beta.Views.Windows
{
    public class featuredModFile
    {
        public string type { get; set; }

        public string id { get; set; }
        public Dictionary<string, object> attributes { get; set; }
    }
    public class answer
    {
        public featuredModFile[] data { get; set; }
    }
    public partial class ApiTests : Window
    {
        private HttpClient HttpClient;
        public ApiTests()
        {
            InitializeComponent();
            HttpClient = new();
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "FAF Client");
        }

        private int counter = 0;
        int global = 0;
        private Thread Thread;
        DateTime lastUpdate;
        long lastBytes = 0;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            global = 0;
            ProgressBar.Value = 0;
            Thread = new Thread(async () =>
            {
                using var stream = HttpClient.GetAsync("https://api.faforever.com/featuredMods/0/files/latest").Result.Content.ReadAsStream();
                
                var json = new StreamReader(stream).ReadToEnd();
                var response = JsonSerializer.Deserialize<answer>(json);
                using WebClient webClient = new()
                {
                    Proxy = null
                };
                webClient.DownloadProgressChanged += DownloadProgressChanged;
                int len = 0;
                for (int i = 0; i < response.data.Length; i++)
                {
                    var item = response.data[i];

                    var path = App.GetPathToFolder(Models.Folder.ProgramData) + item.attributes["group"].ToString();

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    path = path + "\\" + item.attributes["name"].ToString();

                    if (File.Exists(path))
                    {
                        var md5 = Tools.CalculateMD5FromFile(path);
                        if (md5 == item.attributes["md5"].ToString())
                        {
                            response.data[i] = null;
                            continue;
                        }
                    }
                    len++;
                }

                counter = len;

                int index = 0;
                for (int i = 0; i < response.data.Length; i++)
                {
                    var item = response.data[i];
                    if (item is null) continue;

                    var path = App.GetPathToFolder(Models.Folder.Common)
                    + item.attributes["group"].ToString() + "\\"
                    + item.attributes["name"].ToString();

                    Dispatcher.Invoke(() =>
                    {
                        PathText.Text = path;
                        Files.Text = item.attributes["name"].ToString();
                        CurrentFile.Text = ++index + " / " + len;
                    });

                    await webClient.DownloadFileTaskAsync(item.attributes["url"].ToString(), path);
                }

                Dispatcher.Invoke(() =>
                {
                    ProgressBar.Value = 100;
                    Progress.Text = "100%";
                });

                webClient.DownloadProgressChanged -= DownloadProgressChanged;
                webClient.Dispose();
                Thread = null;
            });

            Thread.Start();

        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var now = DateTime.UtcNow;
            var timeSpan = now - lastUpdate;
            if (timeSpan.Seconds != 0)
            {
                var bytesChange = e.BytesReceived - lastBytes;
                var bytesPerSecond = bytesChange / timeSpan.Seconds;
                var mb = Convert.ToInt32(bytesPerSecond) / 1024;
                Dispatcher.Invoke(() => Speed.Text = mb > 0 ? mb + " KB/sec" : bytesPerSecond + " Byte/sec") ;
                lastBytes = e.BytesReceived;
                lastUpdate = now;
            }

            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = global + (e.ProgressPercentage / counter);
                Progress.Text = global + (e.ProgressPercentage / counter) + "/ 100%";
            });

            if (e.ProgressPercentage == 100)
            {
                global += e.ProgressPercentage / counter;
            }
        }
    }
}
