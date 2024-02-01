using AsyncAwaitBestPractices;
using Ethereal.FAF.UI.Client.Infrastructure.OAuth;
using System;
using System.Windows;

namespace Ethereal.FAF.UI.Client.Views.Windows
{
    /// <summary>
    /// Interaction logic for WebViewWindow.xaml
    /// </summary>
    public partial class WebViewWindow
    {
        public WebViewWindow()
        {
            InitializeComponent();
            Unloaded += WebViewWindow_Unloaded;
        }

        private void WebViewWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WebView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            var uri = new Uri(e.Uri);
            if (uri.Host == "localhost")
            {
                Close();
            }
        }
        public void CodeReceived(object sender, (string code, string state) e)
        {
            if (sender is HttpAuthServer httpAuthServer)
            {
                httpAuthServer.CodeReceived -= CodeReceived;
            }
            Close();
        }
        private void Dispose()
        {
            WebView.NavigationStarting -= WebView_NavigationStarting;
            WebView.Stop();
            WebView.CoreWebView2.TrySuspendAsync().SafeFireAndForget();
            WebView.Dispose();
            Unloaded -= WebViewWindow_Unloaded;
        }
    }
}
