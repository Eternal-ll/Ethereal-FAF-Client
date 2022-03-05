using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Tests
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
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
            public ObservableCollection<BitmapImage> Test { get; set; }
            public MainWindow()
            {
                InitializeComponent();
                DataContext = this;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void Button_Click(object sender, RoutedEventArgs e)
        {
            Test = new();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Test)));
                    DirectoryInfo d = new DirectoryInfo(@"C:\Users\Eternal\source\repos\Ethereal-FAF-Client\beta\bin\Release\net5.0-windows10.0.18362.0\cache\previews\small");
                    FileInfo[] Files = d.GetFiles();
                    Random rndm = new();
                    Stopwatch t = new();
                    t.Start();
                    for (int i = 0; i < 1000; i++)
                    {
                        var uri = new Uri(Files[rndm.Next(0, Files.Length - 1)].FullName);
                //using var stream = File.OpenRead(Files[rndm.Next(0, Files.Length - 1)].FullName);
                BitmapImage Image = new()
                {
                    DecodePixelHeight = 100,
                    DecodePixelWidth = 100,
                    CacheOption = BitmapCacheOption.None,
                    UriCachePolicy = new(System.Net.Cache.RequestCacheLevel.CacheOnly)
                };
                //Image.DecodePixelHeight = 100;
                //Image.DecodePixelWidth = 100;
                Image.BeginInit();
                //Image.CacheOption = BitmapCacheOption.None;
                Image.UriSource = uri;
                //Image.StreamSource = stream;
                Image.EndInit();
                Image.Freeze();
                Test.Add(Image);
                //stream.Dispose();
            }
            MessageBox.Show(t.Elapsed.ToString("c"));
                    //Dispatcher.Invoke(() =>
                    //{
                    //    for (int i = 0; i < Test.Count; i++)
                    //    { 
                    //        //WeakReference reference1 = new(Test[i].Image);
                    //        //Test[i].StreamSource.Dispose();
                    //        Test[i] = null;
                    //    }
                    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Test)));
                    //    Test.Clear();
                    //    Test = null;
                    //});
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Test.Count; i++)
            {
                Test[i] = null;
                Test.RemoveAt(i);
            }
            Test.Clear();
            Test = null;
            GC.Collect();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Test)));
        }
    }
}
