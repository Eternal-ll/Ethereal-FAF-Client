using System.Windows;

namespace beta.Views.Windows
{
    /// <summary>
    /// Interaction logic for ServerDebugWindow.xaml
    /// </summary>
    public partial class ServerDebugWindow : Window
    {
        public ServerDebugWindow()
        {
            InitializeComponent();
        }
        public void LOG(string data)
        {
            Dispatcher.Invoke(() =>
            {
                ItemsControl.Items.Add(data);
                //ScrollViewer.ScrollToBottom();
            });

        }
    }
}
