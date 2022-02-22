using System.Threading;
using System.Windows.Controls;

namespace beta.Views
{
    /// <summary>
    /// Interaction logic for AnalyticsView.xaml
    /// </summary>
    public partial class AnalyticsView : UserControl
    {
        private readonly string injectionScript =
               "document.body.style.color='White';" +
               "document.body.style.fontSize='14px';" +

            "document.styleSheets[3].rules[2].cssRules[0].style['backgroundColor'] = 'rgb(20, 20, 20)';" +

            "document.getElementById('blazor-error-ui').style.background='transparent';" +

            "document.getElementsByClassName('alert alert-secondary')[0].style.background = 'rgb(30,30,30)';" +
            "document.getElementsByClassName('alert alert-secondary')[0].style.border = 'none';" +
            "document.getElementsByClassName('alert alert-secondary')[0].style.color = 'white';" +

               "document.getElementsByClassName('page')[0].style.flexDirection='Column';" +

               "document.getElementsByClassName('nav')[0].style.margin = '0 30px';" +
               "document.getElementsByClassName('nav')[0].style.padding = '10px 0';" +

               "document.getElementsByClassName('sidebar')[0].style.width = '100%';" +
               "document.getElementsByClassName('sidebar')[0].style.height = 'auto';" +
               "document.getElementsByClassName('sidebar')[0].style.flexDirection = 'row';" +
               "document.getElementsByClassName('sidebar')[0].style.display = 'flex';" +
               "document.getElementsByClassName('sidebar')[0].style.zIndex = 100;" +
               "document.getElementsByClassName('sidebar')[0].style.display = 'flex';" +
               "document.getElementsByClassName('sidebar')[0].style.background = 'rgb(30,30,30)';" +
            "document.getElementsByClassName('sidebar')[0].removeChild(document.getElementsByClassName('sidebar')[0].children[0]);" +

               "document.getElementsByClassName('collapse')[0].style.display ='flex';" +

               "document.getElementsByClassName('flex-column')[0].className = 'nav';" +

            "document.getElementsByClassName('alert alert-secondary')[0].style.background='rgba(255,255,255,.1)';" +
            "document.getElementsByClassName('alert alert-secondary')[0].style.color='White';" +

               //"document.getElementsByClassName('navbar')[0].style.height= 0;" +
               //"document.getElementsByClassName('navbar')[0].style.visibility= 'collapse';" +
               //"document.getElementsByClassName('navbar')[0].innerText= Null;" +

               "var navs = document.getElementsByClassName('nav-item');" +

               "for(let i =0; i<navs.length; i++){" +
               "var item = navs[i];" +
               "item.style.padding = 0;" +
               "item.style.margin = '0 10px 0 0';" +
               "item.classList.remove('px-3');" +

               "var inner = item.children[0];" +
               "inner.style.lineHeight = 0;" +
               "inner.style.height = 'Auto';" +
               "inner.style.display = 'block';" +
               "inner.style.padding = '10px 0';" +
               "inner.children[0].style = null;" +
               "inner.children[0].style.margin = '0 10px 2px 10px';" +
               "inner.children[0].classList.remove('mr-3');" +
               "inner.style.paddingRight = '10px';" +
               "};" +

            "var fields = document.getElementsByTagName('fieldset');" +
            "for(let i = 0; i<fields.length; i++){" +
            "var field = fields[i];" +
            "field.style.background = 'rgba(30,30,30, 1)';" +
            "field.style.borderColor= 'transparent';" +
            "field.style.borderRadius ='4px';};" +

               "document.getElementsByClassName('collapse')[0].removeChild(document.getElementsByClassName('collapse')[0].children[1]);" +

               "document.getElementsByClassName('top-row px-4')[0].style.visibility = 'collapse';" +
               "document.getElementsByClassName('top-row px-4')[0].style.height = 0;" +

               "document.getElementsByClassName('navbar-toggler')[0].style.visibility = 'collapse';"
               ;
        public AnalyticsView()
        {
            InitializeComponent();
            WebView.NavigationCompleted += WebView_NavigationCompleted;
        }

        private void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            new Thread(() =>
            {
                Thread.Sleep(10000);
                while (true)
                {
                    Dispatcher.Invoke(() => WebView.ExecuteScriptAsync(injectionScript));
                    Dispatcher.Invoke(() => WebView.Visibility=System.Windows.Visibility.Visible);
                    break;
                    //Thread.Sleep(500);
                }
            }).Start();
        }
    }
}
