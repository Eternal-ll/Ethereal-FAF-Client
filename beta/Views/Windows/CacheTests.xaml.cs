using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace beta.Views.Windows
{
    public class Img
    {
        public BitmapImage Image { get; set; }
    }
    public partial class CacheTests : Window
    {
        public ObservableCollection<Img> Test { get; set; } = new();
        public CacheTests()
        {
            InitializeComponent();
            DataContext = this;
            new Thread(() =>
            {
                DirectoryInfo d = new DirectoryInfo(App.GetPathToFolder(Models.Folder.MapsSmallPreviews));
                FileInfo[] Files = d.GetFiles();
                Random rndm = new();
                Stopwatch t = new();
                t.Start();
                for (int i = 0; i < 1000; i++)
                {
                    Dispatcher.Invoke(() => 
                    {
                        var uri = new Uri(Files[rndm.Next(0, Files.Length - 1)].FullName);

                        BitmapImage img = new();
                        img.BeginInit();
                        img.UriSource = uri;
                        img.CacheOption = BitmapCacheOption.OnDemand;
                        img.DecodePixelHeight = 100;
                        img.DecodePixelWidth = 100;
                        img.EndInit();
                        img.Freeze();
                        Test.Add(new() { Image = img });
                    },
                        System.Windows.Threading.DispatcherPriority.Background);
                    //Thread.Sleep(250);
                }
                Dispatcher.Invoke(() => MessageBox.Show(t.Elapsed.ToString("c")));
                Thread.Sleep(2000);
                Dispatcher.Invoke(() =>
                {
                    for (int i = 0; i < Test.Count; i++)
                    {
                        Test[i].Image = null;
                        Test[i] = null;
                    }
                    Test.Clear();
                    Test = null;
                });
            }).Start();
        }
    }
}
