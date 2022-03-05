using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace beta.Views.Windows
{
    public struct Img : IDisposable
    {
        public BitmapImage Image { get; set; }

        public void Dispose()
        {
            //Image.UriSource = null;
            //Image.BaseUri = null;
            Image.StreamSource.Dispose();
            //Image.StreamSource = null;
            Image = null;
        }
    }
    public partial class CacheTests : Window, INotifyPropertyChanged
    {
        public ObservableCollection<BitmapImage> Test { get; set; }
        public CacheTests()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Test)));
                Test = new();
                Thread.Sleep(2000);
                DirectoryInfo d = new DirectoryInfo(App.GetPathToFolder(Models.Folder.MapsSmallPreviews));
                FileInfo[] Files = d.GetFiles();
                Random rndm = new();
                Stopwatch t = new();
                t.Start();
                for (int i = 0; i < 1000; i++)
                {
                    //var uri = new Uri(Files[rndm.Next(0, Files.Length - 1)].FullName);
                    using var stream = File.OpenRead(Files[rndm.Next(0, Files.Length - 1)].FullName);
                    Dispatcher.Invoke(() =>
                    {
                        BitmapImage Image = new()
                        {
                            DecodePixelHeight = 100,
                            DecodePixelWidth = 100,
                        };
                        //Image.DecodePixelHeight = 100;
                        //Image.DecodePixelWidth = 100;
                        Image.BeginInit();
                        Image.CacheOption = BitmapCacheOption.None;
                        //img.UriSource = uri;
                        Image.StreamSource = stream;
                        Image.EndInit();
                        Image.Freeze();
                        Test.Add(Image);
                    },
                        System.Windows.Threading.DispatcherPriority.Background);
                    stream.Dispose();
                }
                Dispatcher.Invoke(() => MessageBox.Show(t.Elapsed.ToString("c")));
                Thread.Sleep(2000);
                Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < Test.Count; i++)
                    {
                        //WeakReference reference1 = new(Test[i].Image);
                        Test[i].StreamSource.Dispose();
                        Test[i] = null;
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Test)));
                    Test.Clear();
                    Test = null;
                });
            }).Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
