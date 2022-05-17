using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace beta
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] Args) =>
            Host.CreateDefaultBuilder(Args)
            .ConfigureLogging(logging =>
            {

            })
            .UseContentRoot(App.CurrentDirectory)
            .ConfigureAppConfiguration((host, cfg) => cfg
            .SetBasePath(App.CurrentDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true))
            .ConfigureServices(App.ConfigureServices);
    }
}