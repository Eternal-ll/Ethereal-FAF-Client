using beta.Infrastructure.Extensions;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Debugger;
using beta.Models.Enums;
using beta.Views.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ZstdNet;
//using ZstdSharp;

namespace beta
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static IServiceProvider Services => Hosting.Services;
        
        public static bool IsDesignMode { get; private set; } = false;
        private static IHost _Hosting;
        public static IHost Hosting => _Hosting ??= Program.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

        public static void Restart()
        {
            System.Diagnostics.Process.Start(ResourceAssembly.Location[..^3] + "exe");
            Current.Shutdown();
        }

        public static NewWindow Window;

        public static string GetPathToFolder(Folder folder)
        {
            return folder switch
            {
                Folder.Game => beta.Properties.Settings.Default.PathToGame,

                // Mods & Maps
                Folder.Maps => Environment.ExpandEnvironmentVariables(beta.Properties.Settings.Default.PathToMaps),
                Folder.Mods => Environment.ExpandEnvironmentVariables(beta.Properties.Settings.Default.PathToMods),

                // CACHE
                Folder.MapsSmallPreviews => CurrentDirectory + "\\cache\\previews\\small\\",
                Folder.MapsLargePreviews => CurrentDirectory + "\\cache\\previews\\large\\",
                Folder.PlayerAvatars => CurrentDirectory + "\\cache\\players\\avatars\\",

                Folder.Emoji => CurrentDirectory + "\\Resources\\Images\\Emoji",
                
                // PATCH
                Folder.ProgramData => "C:\\ProgramData\\FAForever\\",

                Folder.Common => CurrentDirectory + "\\cache\\common\\",

                _ => throw new ArgumentOutOfRangeException(nameof(folder), folder, null)
            };
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            AppDebugger.Init();
            IsDesignMode = false;

            //new Window()
            //{
            //    Content = new ProfileViewModel(new()
            //    {
            //        id = 165,
            //        ratings = new()
            //        {
            //            { "global", new() { rating = new double[] { 10, 1 } } },
            //            { "ladder_1v1", new() { rating = new double[] { 10, 1 } } },
            //            { "tmm_2v2", new() { rating = new double[] { 10, 1 } } },
            //            { "tmm_4v4_share_until_death", new() { rating = new double[] { 10, 1 } } },
            //        }
            //    })
            //}.Show();
            Window = new NewWindow();
            Window.Show();

            string mapPreviews = GetPathToFolder(Folder.MapsSmallPreviews);
            if (Directory.Exists(mapPreviews))
                Directory.CreateDirectory(mapPreviews);

            string emojisCache = GetPathToFolder(Folder.Emoji);
            if (Directory.Exists(emojisCache))
                Directory.CreateDirectory(emojisCache);
            emojisCache = GetPathToFolder(Folder.Common);
            if (Directory.Exists(emojisCache))
                Directory.CreateDirectory(emojisCache);
            emojisCache = GetPathToFolder(Folder.Maps);
            if (Directory.Exists(emojisCache))
                Directory.CreateDirectory(emojisCache);
            emojisCache = GetPathToFolder(Folder.Mods);
            if (Directory.Exists(emojisCache))
                Directory.CreateDirectory(emojisCache);

            Test();
            var host = Hosting;

            base.OnStartup(e);
            await host.StartAsync().ConfigureAwait(false);
        }
        private void Test()
        {
            var test = "KLUv/WBzC50jAOb8xEYAkSgd/Lv5uFyk/9eWQKWLf9W1Q71W+MdBC7kBfbLvTvbm2Fv0N0l5/N/GVfHbB3HYZ+OBA0Ao+p/0iCH1WaBGnvnM5mUKsgCkAKkAcUqAtV8qj1IS7DrFWii5KXK5KN/la4Ea7NpUISfjh6IC4sOvXtnSU2Rvw1eIz8GXjUr+y0ZJXcMV3tye+HwYGGGwemwi899WCGt53XVYIisP650Wq5QCtVYXWpeLvuxX6eRD0FofoRPpAX/SeL1YNws53vwBCnA2/wHcAEfHve4228myDAbygX1snL2FWK+7EI4OB7GWHkydo+Mkn5IPSDYC0oF9VMjN7joa+oAdrN6nCpynjCjwG8ieFV4Rb25demW2DPEzPqm0wr+2XdBT8UVkw9oUE138CdOe6h++vR+FXVzhxzo4IR9uEsS3Nnb8Cl9ql7wujdjZXoyIm01i1rxC23KCoVKC+Bay+HZ2lBEWQ7IhDKb4sNJNxX/dhpqMtz8JYlt8KOOIxtoTbqqX/VLUVOooNztVqfRiIL/2BNfJhY4svd+Jfyfs4JaBePhXfiU4vQ7dK9CM/SLXdYSExTkJipEZEZnPCZ8wjF1tPmg04sOuJ28ucza3Hq1eR6/9Nt6cq83Nvrbr5xJdk4ZIVh8WhDWsrWfFGlmx5vbWvrbFf9nY1fhi+dCLXeZoJ5rJ5vBCtMx5lmWdSflxdid7ZfxaYEXxs0npbRLKZcbXbRQ1za5TAj+GtnCzLT9W6d0iC1/Klv3NLvEzB3xPFmsJtQ3OSTcbNeZoNGT+GW+nONd4fLCRQqMEihArOoTMaDGAxI3x4B4aTgQGYwYFRB9gwGJyqCCCZd5D49HgQYONFAlYLKhRY4bnhYyYi3to+AIe+gEC13BkOBY2jDKoMKFCQYkIDn0D5h6af8MaAhhW3DjnX73zoyE+eY9Yo0FC8TsnKel4sg9fU6lwwpwYZQSLSJvdfRFOxEBuqlwbSbEbZU9qK0SpgPgarlFiXMMUm8hoJikmrjDWEsUgVKWShmJdBbGJzKWeSsWiACf/TSBRiREw/9dw/gmAmajxOKaMERkSERGZSdIGUAIhhMqqBxKAFUHPIYZIUSiVXVJYUqOeA8QbHf0FSm1nAvU32mEaL1CAtTQztPJPHFuoKQjkdC+woK1HRIJprzjzIjphHhAz0uBVmESVuKVwuwlMp/lB+TnvIVZBs3Z8ItfyI/HmFT9lfsiJPYj7fjeM4onUzP4lOy/Gw1JGJaWBEqNCA2sAiM8emrAAqCCi0+egEdw6w252D8+RthA26QoiWAVdMjgBe416YRJVEAjWgEIaS5OBAG2cQCAFSXfR0WhVHguV3oAyytrg4PL5z68ZF8MZNi9BHGMaNg2zgh8Wo4kfIARu+WlL/wf+vXuKstLdFOXh6Zjsl1SHS1racEXmGi3un6onlQ3NA98tdTowwy80BoSCltYMxYOey+345/LYSdxv2D+1W1/43lf5GKeRqMB6xxBpRxABsLigAAaAeXpeAf2uCu6NjfAH";
            var data = Convert.FromBase64String(test);
            var bg = Convert.ToBase64String(data);

            if (bg == test)
            {

            }
            else
            {

            }
            using var decompressor = new Decompressor();
            var compressedData = decompressor.Unwrap(data);


            using var compressor = new Compressor();
            var compressed = compressor.Wrap(compressedData);
            var data2 = Convert.ToBase64String(compressed);

            if (data2 == test)
            {

            }
            else
            {

            }
        }
        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            var host = Hosting;

            var gameSession = Services.GetService<IGameSessionService>();
            gameSession.Close();

            await host.StopAsync().ConfigureAwait(false);
            host.Dispose();
            _Hosting = null;

            Environment.Exit(-1);
        }

        public static void ConfigureServices(HostBuilderContext host, IServiceCollection services) => services
            .RegisterServices();

        // TODO: maybe will be useful for future
        public static string CurrentDirectory => IsDesignMode 
            ? Path.GetDirectoryName(GetSourceCodePath())
            : Environment.CurrentDirectory;

        private static string GetSourceCodePath([CallerFilePath] string Path = null) => Path;

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var win = new Window()
            {
                Content = new ExceptionWrapper(e.Exception)
            };
            win.Closed += Win_Closed;
            win.Show();
            // Prevent default unhandled exception processing
            //Task.Run(() =>
            //{
            //    Thread.Sleep(10000);
            //    Environment.Exit(-1);
            //});
            e.Handled = true;
        }

        private void Win_Closed(object sender, EventArgs e)
        {
            Environment.Exit(-1);
        }
    }
}
