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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LiveChartsSamples.Views
{
    /// <summary>
    /// Interaction logic for DialogTestV.xaml
    /// </summary>
    public partial class DialogTestV : UserControl
    {
        event EventHandler Event;
        public DialogTestV()
        {
            InitializeComponent();
            Task.Run(async () =>
            {
                Thread.Sleep(2000);
                for (int i = 0; i < 100; i++)
                {
                    if (i == 5)
                    {
                        var res = await App.Current.Dispatcher.InvokeAsync(() => MessageBox.Show("", "", MessageBoxButton.YesNoCancel));
                        switch (res)
                        {
                            case MessageBoxResult.None:
                                return;
                            case MessageBoxResult.OK:
                                break;
                            case MessageBoxResult.Cancel:
                                return;
                            case MessageBoxResult.Yes:
                                break;
                            case MessageBoxResult.No:
                                return;
                            default:
                                break;
                        }
                    }
                    Dispatcher.Invoke(() =>
                    ProgressBar.Value = i + 1);
                    Thread.Sleep(50);
                }
            });
        }
    }
}
