using System.Threading;
using System.Windows.Controls;

namespace beta.Views
{
    public partial class AnalyticsView : UserControl
    {
        private readonly string injectionScript = "";
            //"if(injected)return; 
            //   "document.body.style.color='White';
            //   "document.body.style.fontSize='14px';

            //"document.styleSheets[3].rules[2].cssRules[0].style['backgroundColor'] = 'rgb(20, 20, 20)';

            //"document.getElementById('blazor-error-ui').style.background='transparent';

            //"document.getElementsByClassName('alert alert-secondary')[0].style.background = 'rgb(30,30,30)';
            //"document.getElementsByClassName('alert alert-secondary')[0].style.border = 'none';
            //"document.getElementsByClassName('alert alert-secondary')[0].style.color = 'white';

            //   "document.getElementsByClassName('page')[0].style.flexDirection='Column';

            //   "document.getElementsByClassName('nav')[0].style.margin = '0 30px';
            //   "document.getElementsByClassName('nav')[0].style.padding = '10px 0';

            //   "document.getElementsByClassName('sidebar')[0].style.width = '100%';
            //   "document.getElementsByClassName('sidebar')[0].style.height = 'auto';
            //   "document.getElementsByClassName('sidebar')[0].style.flexDirection = 'row';
            //   "document.getElementsByClassName('sidebar')[0].style.display = 'flex';
            //   "document.getElementsByClassName('sidebar')[0].style.zIndex = 100;
            //   "document.getElementsByClassName('sidebar')[0].style.display = 'flex';
            //   "document.getElementsByClassName('sidebar')[0].style.background = 'rgb(30,30,30)';
            //"document.getElementsByClassName('sidebar')[0].removeChild(document.getElementsByClassName('sidebar')[0].children[0]);

            //   "document.getElementsByClassName('collapse')[0].style.display ='flex';

            //   "document.getElementsByClassName('flex-column')[0].className = 'nav';

            //"document.getElementsByClassName('alert alert-secondary')[0].style.background='rgba(255,255,255,.1)';
            //"document.getElementsByClassName('alert alert-secondary')[0].style.color='White';

            //   //"document.getElementsByClassName('navbar')[0].style.height= 0;
            //   //"document.getElementsByClassName('navbar')[0].style.visibility= 'collapse';
            //   //"document.getElementsByClassName('navbar')[0].innerText= Null;

            //   "var navs = document.getElementsByClassName('nav-item');

            //   "for(let i =0; i<navs.length; i++){
            //   "var item = navs[i];
            //   "item.style.padding = 0;
            //   "item.style.margin = '0 10px 0 0';
            //   "item.classList.remove('px-3');

            //   "var inner = item.children[0];
            //   "inner.style.lineHeight = 0;
            //   "inner.style.height = 'Auto';
            //   "inner.style.display = 'block';
            //   "inner.style.padding = '10px 0';
            //   "inner.children[0].style = null;
            //   "inner.children[0].style.margin = '0 10px 2px 10px';
            //   "inner.children[0].classList.remove('mr-3');
            //   "inner.style.paddingRight = '10px';
            //   "};

            //"var fields = document.getElementsByTagName('fieldset');
            //"for(let i = 0; i<fields.length; i++){
            //"var field = fields[i];
            //"field.style.background = 'rgba(30,30,30, 1)';
            //"field.style.borderColor= 'transparent';
            //"field.style.borderRadius ='4px';};

            //   "document.getElementsByClassName('collapse')[0].removeChild(document.getElementsByClassName('collapse')[0].children[1]);

            //   "document.getElementsByClassName('top-row px-4')[0].style.visibility = 'collapse';
            //   "document.getElementsByClassName('top-row px-4')[0].style.height = 0;

            //   "document.getElementsByClassName('navbar-toggler')[0].style.visibility = 'collapse';
            //"if (document.getElementsByClassName('navbar-toggler')[0]) var injected = true;
               
        public AnalyticsView()
        {
            InitializeComponent();

            IsVisibleChanged += AnalyticsView_IsVisibleChanged;
            WebView.NavigationCompleted += WebView_NavigationCompleted;
        }

        private void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            WebView.ExecuteScriptAsync(@"

var myElement = document.getElementById('app');
if(window.addEventListener) {
   // Normal browsers
   myElement.addEventListener('DOMSubtreeModified', contentChanged, false);
} else
   if(window.attachEvent) {
      // IE
      myElement.attachEvent('DOMSubtreeModified', contentChanged);
   }

function contentChanged() {
   // this function will run each time the content of the DIV changes

document.styleSheets[3].rules[2].cssRules[0].style['backgroundColor'] = 'rgb(20, 20, 20)';

document.getElementsByClassName('alert alert-secondary')[0].style.background = 'rgb(30,30,30)';
document.getElementsByClassName('alert alert-secondary')[0].style.border = 'none';
document.getElementsByClassName('alert alert-secondary')[0].style.color = 'white';

document.getElementsByClassName('page')[0].style.flexDirection='Column';

document.getElementsByClassName('nav')[0].style.margin = '0 30px';
document.getElementsByClassName('nav')[0].style.padding = '10px 0';

document.getElementsByClassName('sidebar')[0].style.width = '100%';
document.getElementsByClassName('sidebar')[0].style.height = 'auto';
document.getElementsByClassName('sidebar')[0].style.flexDirection = 'row';
document.getElementsByClassName('sidebar')[0].style.display = 'flex';
document.getElementsByClassName('sidebar')[0].style.zIndex = 100;
document.getElementsByClassName('sidebar')[0].style.display = 'flex';
document.getElementsByClassName('sidebar')[0].style.background = 'rgb(30,30,30)';
document.getElementsByClassName('sidebar')[0].removeChild(document.getElementsByClassName('sidebar')[0].children[0]);

document.getElementsByClassName('collapse')[0].style.display ='flex';

document.getElementsByClassName('flex-column')[0].className = 'nav';

document.getElementsByClassName('alert alert-secondary')[0].style.background='rgba(255,255,255,.1)';
document.getElementsByClassName('alert alert-secondary')[0].style.color='White';

document.getElementsByClassName('navbar')[0].style.height= 0;
document.getElementsByClassName('navbar')[0].style.visibility= 'collapse';
document.getElementsByClassName('navbar')[0].innerText= Null;

var navs = document.getElementsByClassName('nav-item');

for(let i =0; i<navs.length; i++){
var item = navs[i];
item.style.padding = 0;
item.style.margin = '0 10px 0 0';
item.classList.remove('px-3');

var inner = item.children[0];
inner.style.lineHeight = 0;
inner.style.height = 'Auto';
inner.style.display = 'block';
inner.style.padding = '10px 0';
inner.children[0].style = null;
inner.children[0].style.margin = '0 10px 2px 10px';
inner.children[0].classList.remove('mr-3');
inner.style.paddingRight = '10px';
};

var fields = document.getElementsByTagName('fieldset');
for(let i = 0; i<fields.length; i++){
var field = fields[i];
field.style.background = 'rgba(30,30,30, 1)';
field.style.borderColor= 'transparent';
field.style.borderRadius ='4px';};

//document.getElementsByClassName('collapse')[0].removeChild(document.getElementsByClassName('collapse')[0].children[1]);

//document.getElementsByClassName('top-row px-4')[0].style.visibility = 'collapse';
//document.getElementsByClassName('top-row px-4')[0].style.height = 0;

//document.getElementsByClassName('navbar-toggler')[0].style.visibility = 'collapse';
//if (document.getElementsByClassName('navbar-toggler')[0]) var injected = true;

");
        }

        private bool StopInjection;
        private void Inject()
        {
            //while (!StopInjection)
            //{
            //    WebView.ExecuteScriptAsync(injectionScript);
            //    Thread.Sleep(10000);
            //}
        }

        private Thread InjectionThread;

        bool IsControlInitialized = false;
        private void AnalyticsView_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (IsControlInitialized & !(bool)e.NewValue)
            {
                try
                {

                    WebView.Stop();

                    WebView.Dispose();
                }
                catch
                {

                }
                StopInjection = true;
                InjectionThread = null;
            }
            if ((bool)e.NewValue)
            {
                IsControlInitialized = true;
            }
        }
    }
}
