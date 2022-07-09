using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FAF.UI.EtherealClient.Views.Windows
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
                Dispatcher.Invoke(() =>
                {
                    Width = 500;
                    Height = 500;
                }, System.Windows.Threading.DispatcherPriority.Send);
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                Dispatcher.Invoke(() =>
                {
                    Width = 500;
                    Height = 500;
                }, System.Windows.Threading.DispatcherPriority.Send);
            });
        }
    }
}
