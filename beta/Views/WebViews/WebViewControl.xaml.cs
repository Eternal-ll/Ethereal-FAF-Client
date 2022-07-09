using Microsoft.Web.WebView2.Core;
using System;
using System.Net;
using System.Net.Http;
using System.Windows.Controls;

namespace beta.Views.WebViews
{
    /// <summary>
    /// Interaction logic for WebViewControl.xaml
    /// </summary>
    public partial class WebViewControl : UserControl
    {
        public WebViewControl()
        {
            IsVisibleChanged += OnVisibleChanged;
            InitializeComponent();
        }
        private async void InitializeWebViewWithUserAgent()
        {
            CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync();
            await WebView.EnsureCoreWebView2Async(environment);
            WebView.CoreWebView2.Settings.UserAgent = "FAF Client";
        }
        public async void Navigate(Uri targetUrl)
        {
            if (targetUrl.Segments.Length == 2 && targetUrl.Segments[1] == "newshub" && WebView.CoreWebView2 is null)
            {
                InitializeWebViewWithUserAgent();
            }
            WebView.Source = targetUrl;
            // preparing special conditions for kazbek analytics site
            if (targetUrl.Host == "kazbek.github.io") 
            {
                InjectDesignToKazbekSite();
            }
        }
        private void InjectDesignToKazbekSite()
        {
            string script = "";
            WebView.ExecuteScriptAsync(script);
        }

        private void OnVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) return;
            WebView.IsVisibleChanged -= OnVisibleChanged;
            Dispose();
        }

        public void Dispose()
        {
            WebView.Dispose();
            Content = null;
            UpdateLayout();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
