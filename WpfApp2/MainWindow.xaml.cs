using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp2.HostingExternal;

namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Process process;
        public MainWindow()
        {
            InitializeComponent();

            process = new Process()
            {
                StartInfo = new()
                {
                    FileName = @"C:\Users\Eternal\source\repos\Ethereal-FAF-Client\Ethereal.FAF.UI.Client\bin\Debug\net6.0-windows\External\jre\bin\java.exe",
                    Arguments = @"-jar ""C:\Users\Eternal\source\repos\Ethereal-FAF-Client\Ethereal.FAF.UI.Client\bin\Debug\net6.0-windows\External\MapGenerator_1.8.5.jar"" --visualize"
                }
            };
            process.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var data = Process.GetProcessesByName("java");
            var hostedChild = new HwndHostEx(data[0].MainWindowHandle);

            // Any FrameworkElement that inherits from System.Windows.Controls.Decorator can host the child.
            // No need to use WindowsFormsHost!
            Host.Content = hostedChild;
        }
    }
}
