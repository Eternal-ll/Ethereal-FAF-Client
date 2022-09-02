using System;
using System.Threading.Tasks;
using System.Windows;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Ethereal.UI.Notifications
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Task.Run(async () =>
            {
                await Task.Delay(5000);

                string title = "featured picture of the day";
                string content = "beautiful scenery";
                string image = "https://picsum.photos/360/180?image=104";
                string logo = "https://picsum.photos/64?image=883";

                string xmlString =
                $@"<toast><visual>
       <binding template='ToastGeneric'>
       <text>{title}</text>
       <text>{content}</text>
       <image src='{image}'/>
       <image src='{logo}' placement='appLogoOverride' hint-crop='circle'/>
       </binding>
      </visual></toast>";

                XmlDocument toastXml = new XmlDocument();
                toastXml.LoadXml(xmlString);

                ToastNotification toast = new ToastNotification(toastXml);

                try
                {

                    ToastNotificationManager.CreateToastNotifier().Show(toast);
                }
                catch(Exception ex)
                {

                }
            });
        }

        private void MainWindow_Initialized(object? sender, EventArgs e)
        {

        }
    }
}
